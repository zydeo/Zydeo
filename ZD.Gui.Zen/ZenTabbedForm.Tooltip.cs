using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZD.Common;

namespace ZD.Gui.Zen
{
    partial class ZenTabbedForm
    {
        private class TooltipInfo
        {
            public readonly IZenTooltip Tooltip;
            /// <summary>
            /// Animation state of this tooltip.
            /// </summary>
            /// <remarks>
            /// float.MinValue: make tooltip disappear; remove from collection; repaint
            /// 0: not shown
            /// 0..100: countdown to be shown, but not visible yet
            /// 100..200: fade-in animation
            /// 200..300: countdown to hide
            /// 300..400: fade-out animation
            /// </remarks>
            public float AnimState = 0;
            /// <summary>
            /// If true, tooltip must be repainted in next timer cycle; used to change text or location of shown TT.
            /// </summary>
            public bool Repaint = false;
            /// <summary>
            /// Ctor: takes tooltip info provider.
            /// </summary>
            public TooltipInfo(IZenTooltip tooltip)
            {
                Tooltip = tooltip;
            }
        }

        /// <summary>
        /// Info about tooltips to draw, for the paint function
        /// </summary>
        private class TooltipToPaint
        {
            /// <summary>
            /// Control whose tooltip this is.
            /// </summary>
            public readonly ZenControlBase Ctrl;
            /// <summary>
            /// Info about tooltip.
            /// </summary>
            public readonly TooltipInfo TTI;
            /// <summary>
            /// Strength of tooltip graphics. 0: not visible; 1: fully visible.
            /// </summary>
            public readonly float Strength;
            /// <summary>
            /// Ctor: init immutable instance.
            /// </summary>
            public TooltipToPaint(ZenControlBase ctrl, TooltipInfo tti, float strength)
            {
                Ctrl = ctrl;
                TTI = tti;
                Strength = strength;
            }
        }

        /// <summary>
        /// Registers a control for showing tooltips; or removes it if null is passed.
        /// </summary>
        internal sealed override void RegisterControlForTooltip(ZenControlBase ctrl, IZenTooltip tt)
        {
            lock (tooltipInfos)
            {
                // Removing existing tooltip?
                if (tt == null && tooltipInfos.ContainsKey(ctrl))
                {
                    // Tooltip will disappear when timer next hits.
                    tooltipInfos[ctrl].AnimState = float.MinValue;
                    // Make sure timer does hit.
                    SubscribeToTimer();
                }
                // Adding or resetting tooltip?
                else if (tt != null)
                {
                    // Not there yet: add now
                    if (!tooltipInfos.ContainsKey(ctrl))
                    {
                        TooltipInfo tti = new TooltipInfo(tt);
                        tooltipInfos[ctrl] = tti;
                    }
                    // There already: replace; keep animation state; make it repaint
                    else
                    {
                        TooltipInfo ttiExisting = tooltipInfos[ctrl];
                        TooltipInfo tti = new TooltipInfo(tt);
                        tti.AnimState = ttiExisting.AnimState;
                        tti.Repaint = ttiExisting.AnimState != 0;
                        tooltipInfos[ctrl] = tti;
                        // If we need a repaint, make sure timer does hit.
                        if (tti.Repaint) SubscribeToTimer();
                    }
                }
            }
        }

        /// <summary>
        /// Handles UI changes (mouse enter, leave etc.) affecting tooltip visibility
        /// </summary>
        internal sealed override void TooltipMouseAction(ZenControlBase ctrl, bool show)
        {
            lock (tooltipInfos)
            {
                // Control does not have a tooltip - nothing to do
                if (!tooltipInfos.ContainsKey(ctrl)) return;
                TooltipInfo tti = tooltipInfos[ctrl];
                // Mouse enter: fade in from 0, or fade back in
                if (show)
                {
                    // Currently nowhere: start countdown to show
                    if (tti.AnimState == 0) tti.AnimState = 0.01F;
                    // Countdown already in progress or already fading in: nothing to do
                    else if (tti.AnimState > 0 && tti.AnimState <= 100) { /* NOP */ }
                    // Currently fading out: fade back in
                    else if (tti.AnimState > 300)
                    {
                        // Already practically faded out: start fade-in all over again
                        if (tti.AnimState > 399) tti.AnimState = 200.01F;
                        // Otherwise, fade back in from same place
                        else tti.AnimState = 100.01F + (400F - tti.AnimState);
                    }
                    // Countdown to hide is in progress: keep as is
                    // float.MinValue to remove highlight: keep as is
                    else { /* NOP */ }
                    // For mouse enter, we always want to make sure timer is running
                    SubscribeToTimer();
                }
                // Mouse leave, or other reason to take down tooltip
                else
                {
                    // Currently not shown: nothing to do; also not start timer b/c of this.
                    if (tti.AnimState == 0) return;
                    // Currently counting down to show: reset to not shown; do not request timer
                    if (tti.AnimState > 0 && tti.AnimState <= 99.09F)
                    {
                        tti.AnimState = 0;
                        return;
                    }
                    // All other state changes will require timer
                    SubscribeToTimer();
                    // Not faded in much yet: just make it disappear
                    if (tti.AnimState < 101F) { tti.AnimState = 0; tti.Repaint = true; }
                    // Fading in: reverse
                    else if (tti.AnimState > 100F && tti.AnimState < 199F) tti.AnimState = 300F + (200F - tti.AnimState);
                    // Fully shown, not yet fading out: fade out
                    else if (tti.AnimState < 301F) tti.AnimState = 300F;
                    // Otherwise, it's request to remove (float.MinValue) or already fading out: no change
                    else { /* NOP */ }
                }
            }
        }

        /// <summary>
        /// Handles tooltip animations.
        /// </summary>
        /// <returns>True if timer is still needed.</returns>
        private void doTimerTooltip(out bool timerNeeded, out bool paintNeeded)
        {
            paintNeeded = false;
            timerNeeded = false;
            lock (tooltipInfos)
            {
                List<ZenControlBase> toRemove = new List<ZenControlBase>();
                foreach (var x in tooltipInfos)
                {
                    TooltipInfo tti = x.Value;
                    if (tti.Repaint) paintNeeded = true;
                    if (tti.AnimState == 0) continue;
                    // Only case we don't need the timer if all tooltip controls are inactive
                    timerNeeded = true;
                    // Countdown to show
                    if (tti.AnimState > 0 && tti.AnimState <= 100F) tti.AnimState += 4.0F;
                    // Fade-in
                    else if (tti.AnimState > 100F && tti.AnimState <= 200F)
                    {
                        tti.AnimState += 5F;
                        paintNeeded = true;
                    }
                    // Keep shown
                    else if (tti.AnimState > 200F && tti.AnimState <= 300F) tti.AnimState += 0.4F;
                    // Fade out
                    else if (tti.AnimState <= 400F)
                    {
                        tti.AnimState += 15F;
                        paintNeeded = true;
                    }
                    // Already faded out: take off
                    else if (tti.AnimState > 400F)
                    {
                        tti.AnimState = 0;
                        paintNeeded = true;
                    }
                    // Must remove from collection and make it go away
                    else
                    {
                        toRemove.Add(x.Key);
                        paintNeeded = true;
                    }
                }
                foreach (var ctrl in toRemove) tooltipInfos.Remove(ctrl);
            }
        }

        /// <summary>
        /// Gets info for paint function about tooltips to paint.
        /// </summary>
        /// <remarks>
        /// Typically there's only ever one tooltip at a time, but we may get more, depending on timing,
        /// if one is fading out and another is fading in.
        /// </remarks>
        private List<TooltipToPaint> getTooltipsToPaint()
        {
            List<TooltipToPaint> res = new List<TooltipToPaint>();
            lock (tooltipInfos)
            {
                foreach (var x in tooltipInfos)
                {
                    float ast = x.Value.AnimState;
                    // To be removed; not visible; counting down to show: nothing.
                    if (ast < 100F) continue;
                    // Float-in animation
                    else if (ast <= 200F)
                    {
                        float strength = (ast - 100F) / 100F;
                        res.Add(new TooltipToPaint(x.Key, x.Value, strength));
                    }
                    // Visible
                    else if (ast <= 300F) res.Add(new TooltipToPaint(x.Key, x.Value, 1F));
                    // Fading out
                    else if (ast < 400F)
                    {
                        float strength = (100F - (ast - 300F)) / 100F;
                        res.Add(new TooltipToPaint(x.Key, x.Value, strength));
                    }
                    // Other: already faded out, "paint" one last time with 0 strength to make sure it disappears.
                    else res.Add(new TooltipToPaint(x.Key, x.Value, 0));
                }
            }
            return res;
        }

        #region My own tooltip source - for system buttons

        private class SysBtnTooltips : IZenTooltip
        {
            private readonly ZenSystemButton button;
            private readonly string text;

            public int NeedlePos
            {
                get { return button.Height / 2; }
            }

            public TooltipLocation TooltipLocation
            {
                get { return Zen.TooltipLocation.West; }
            }

            public int NeedleHeight
            {
                get { return button.Height / 4; }
            }

            public int TopOrSide
            {
                get { return 0; }
            }

            public bool HideOnClick
            {
                get { return true; }
            }

            public string Text
            {
                get { return text; }
            }

            public SysBtnTooltips(ZenSystemButton button, ITextProvider tprov)
            {
                this.button = button;
                if (button.BtnType == SystemButtonType.Close) text = tprov.GetString("MainCloseTooltip");
                else if (button.BtnType == SystemButtonType.Minimize) text = tprov.GetString("MainMinimizeTooltip");
            }
        }
 

        #endregion
    }
}
