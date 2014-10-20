using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using ZD.Common;
using ZD.Gui.Zen;

namespace ZD.Gui
{
    partial class LookupControl
    {
        /// <summary>
        /// Tooltip source for the "search language" and "script" buttons.
        /// </summary>
        private class SearchOptionsTooltip : IZenTooltip
        {
            private readonly ZenGradientButton button;
            private readonly int needleHeight;
            private readonly int topOrSide;
            private readonly string text;

            public int NeedlePos
            {
                get { return button.Width / 2; }
            }

            public TooltipLocation TooltipLocation
            {
                get { return Zen.TooltipLocation.North; }
            }

            public int NeedleHeight
            {
                get { return needleHeight; }
            }

            public int TopOrSide
            {
                get { return topOrSide; }
            }

            public bool HideOnClick
            {
                get { return false; }
            }

            public string Text
            {
                get { return text; }
            }

            /// <summary>
            /// Initializes tooltip provider for "search language" or "script" button.
            /// </summary>
            /// <param name="button">The actual button.</param>
            /// <param name="isLang">If true, this is tooltip for "search lang"; otherwise, for "script".</param>
            /// <param name="tprov">Localized UI strings provider.</param>
            /// <param name="script">Current search script.</param>
            /// <param name="lang">Current search language.</param>
            /// <param name="needleHeight">Needle's height at today's scaling.</param>
            public SearchOptionsTooltip(ZenGradientButton button, bool isLang, ITextProvider tprov,
                SearchScript script, SearchLang lang, int needleHeight, int boxRight)
            {
                this.button = button;
                this.needleHeight = needleHeight;
                this.topOrSide = -boxRight;
                if (isLang)
                {
                    if (lang == SearchLang.Chinese) text = tprov.GetString("LangZhoTooltip");
                    else text = tprov.GetString("LangTrgTooltip");
                }
                else
                {
                    if (script == SearchScript.Simplified) text = tprov.GetString("ScriptSimpTooltip");
                    else if (script == SearchScript.Traditional) text = tprov.GetString("ScriptTradTooltip");
                    else text = tprov.GetString("ScriptBothTooltip");
                }
            }
        }

        /// <summary>
        /// Tooltip source provider for Clear and Undo buttons under writing pad.
        /// </summary>
        private class ClearUndoTooltips : IZenTooltip
        {
            private readonly bool isClear;
            private readonly ZenGradientButton button;
            private readonly int needleHeight;
            private readonly string text;

            public int NeedlePos
            {
                get { return isClear ? button.Width / 3 : button.Width * 2 / 3; }
            }

            public TooltipLocation TooltipLocation
            {
                get { return Zen.TooltipLocation.North; }
            }

            public int NeedleHeight
            {
                get { return needleHeight; }
            }

            public int TopOrSide
            {
                get { return isClear ? 0 : -button.Width; }
            }

            public bool HideOnClick
            {
                get { return true; }
            }

            public string Text
            {
                get { return text; }
            }

            public ClearUndoTooltips(ZenGradientButton button, bool isClear, ITextProvider tprov, int needleHeight)
            {
                this.isClear = isClear;
                this.button = button;
                this.needleHeight = needleHeight;
                if (isClear) text = "Clear the writing pad";
                else text = "Undo last stroke";
            }
        }
    }
}
