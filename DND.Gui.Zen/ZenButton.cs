using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace DND.Gui.Zen
{
    public class ZenButton : ZenControl
    {
        private static readonly Color defaultBgColor = Color.White;

        private bool visible = true;
        private bool hasBorder = true;
        private int padding = 0;
        private Image image = null;
        private string text = string.Empty;
        private float fontSize = 12.0F;
        private SizeF textSize = new SizeF(0, 0);
        private Font fntText;
        private Color backColor = defaultBgColor;
        private Color hoverBackColor = defaultBgColor;
        private Color pressedBackColor = defaultBgColor;

        /// <summary>
        /// <para>Gets or sets whether the button is visible.</para>
        /// <para>Invisible buttons also don't animate, interact, or fire the click event.</para>
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        /// <summary>
        /// Gets or sets whether button draws a border.
        /// </summary>
        public bool HasBorder
        {
            get { return hasBorder; }
            set { hasBorder = value; }
        }

        /// <summary>
        /// Gets or sets button's padding (from edge, not from border). Affects image scaling.
        /// </summary>
        public int Padding
        {
            get { return padding; }
            set { padding = value; }
        }

        /// <summary>
        /// Gets or sets image. Button takes ownership: will dispose image.
        /// </summary>
        public Image Image
        {
            get { return image; }
            set
            {
                if (image != null) image.Dispose();
                image = value;
            }
        }

        /// <summary>
        /// Gets or sets the button's display text.
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                text = value == null ? string.Empty : value;
                textSize = measure(text);
            }
        }

        /// <summary>
        /// Gets or sets button text's display font size.
        /// </summary>
        public float FontSize
        {
            get { return fontSize; }
            set
            {
                fontSize = value;
                fntText.Dispose();
                fntText = new Font(ZenParams.GenericFontFamily, fontSize);
                textSize = measure(text);
            }
        }

        /// <summary>
        /// Gets or sets the button's back color.
        /// </summary>
        public Color BackColor
        {
            get { return backColor; }
            set { backColor = value; heatCurrent = value; }
        }

        /// <summary>
        /// Gets or sets the button's back color in in hover mode, or when button has focus.
        /// </summary>
        public Color HoverBackColor
        {
            get { return hoverBackColor; }
            set { hoverBackColor = value; }
        }

        /// <summary>
        /// Gets or sets the button's back color when it is pressed (mouse down).
        /// </summary>
        public Color PressedBackColor
        {
            get { return pressedBackColor; }
            set { pressedBackColor = value; }
        }

        /// <summary>
        /// Ctor: takes owner.
        /// </summary>
        public ZenButton(ZenControlBase parent)
            : base(parent)
        {
            fntText = new Font(ZenParams.GenericFontFamily, fontSize);
        }

        public override void Dispose()
        {
            if (image != null) image.Dispose();
            fntText.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// <para>Gets the button's preferred width, depending on presence of image, and assuming display text.</para>
        /// <para>Uses current font size. May depend on button's height due to image scaling.</para>
        /// </summary>
        public int GetPreferredWidth(bool withImage, string text)
        {
            if (text == null) text = string.Empty;
            double wf = Math.Round(measure(text).Width);
            int w = (int)wf;
            // If there is no image, preferred width is text plus padding on left and right
            if (image == null) return w + 2 * padding;
            // Otherwise, image takes up control height on left; plus text; plud pad right
            return Height + w + padding;

        }

        /// <summary>
        /// Measures display text with current font size.
        /// </summary>
        private SizeF measure(string text)
        {
            StringFormat sf = StringFormat.GenericTypographic;
            return MeasureText(text, fntText, sf);
        }

        /// <summary>
        /// Represents shrink/grow animation states.
        /// </summary>
        private enum SizeAnimState
        {
            /// <summary>
            /// No animation in progress.
            /// </summary>
            None,
            /// <summary>
            /// Image is being shrunk, to be grown again. Used on hover.
            /// </summary>
            ShrinkBeforeGrow,
            /// <summary>
            /// Image is being grown after shrinking. Used on hover.
            /// </summary>
            GrowAfterShrink,
            /// <summary>
            /// Image is being shrunk, to stay shrunk. Used on mouse down.
            /// </summary>
            Shrink,
            /// <summary>
            /// Image is being grown back. Used on mouse up.
            /// </summary>
            Grow,
        }

        private enum HeatAnimState
        {
            None,
            HeatUp,
            HeatUpFast,
            CoolDown,
            CoolDownFast,
        }

        /// <summary>
        /// Lock object around animation's status parameters.
        /// </summary>
        private readonly object animLO = new object();
        /// <summary>
        /// Proportion to original to shrink to (relative minimum size of image).
        /// </summary>
        private const double smallProp = 0.8;
        /// <summary>
        /// Current animation state.
        /// </summary>
        private SizeAnimState sizeAnimState = SizeAnimState.None;
        /// <summary>
        /// Current value in animation; between -1 and 1.
        /// </summary>
        private double sizeAnimVal = double.MinValue;
        /// <summary>
        /// If true, image must remain shrunk after finished shrinking. Used while mouse is down.
        /// </summary>
        private bool stableShrinkedState = false;

        private HeatAnimState heatAnimState = HeatAnimState.None;
        private Color heatStart;
        private Color heatTarget;
        private Color heatCoolBackTo;
        private Color heatCurrent = defaultBgColor;
        private bool heatAutoCoolBack = false;
        private double heatAnimVal = double.MinValue;

        /// <summary>
        /// Gets the current transitional values for painting.
        /// </summary>
        private void getAnimValues(out float prop, out Color bgCol)
        {
            lock (animLO)
            {
                // Image size
                if (sizeAnimVal == double.MinValue)
                    prop = stableShrinkedState ? (float)smallProp : 1.0F;
                else
                {
                    double val;
                    val = 1.0 - Math.Pow(sizeAnimVal, 2.0);
                    val = (Math.Cos(sizeAnimVal * Math.PI) + 1.0) / 2.0;
                    val *= (1.0 - smallProp);
                    prop = (float)(1.0 - val);
                }
                // Heat
                bgCol = heatCurrent;
            }
        }

        /// <summary>
        /// Starts a shrink-grow animation (on hover). Can transition from other animation in progress.
        /// </summary>
        private void doStartAnimShrinkGrow()
        {
            // Only bother if there is an image
            if (image == null) return;
            lock (animLO)
            {
                if (sizeAnimState == SizeAnimState.None)
                    sizeAnimVal = -1.0;
                else if (sizeAnimState == SizeAnimState.GrowAfterShrink || sizeAnimState == SizeAnimState.Grow)
                    sizeAnimVal = -sizeAnimVal;
                sizeAnimState = SizeAnimState.ShrinkBeforeGrow;
                if (stableShrinkedState)
                {
                    sizeAnimVal = 0;
                    sizeAnimState = SizeAnimState.GrowAfterShrink;
                }
                stableShrinkedState = false;
            }
            SubscribeToTimer();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Starts a shrink animation (on mouse down). Can transition from other animation in progress.
        /// </summary>
        private void doStartAnimShrink()
        {
            // Only bother if there is an image
            if (image == null) return;
            lock (animLO)
            {
                if (sizeAnimState == SizeAnimState.None)
                    sizeAnimVal = -1.0;
                else if (sizeAnimState == SizeAnimState.GrowAfterShrink || sizeAnimState == SizeAnimState.Grow)
                    sizeAnimVal = -sizeAnimVal;
                sizeAnimState = SizeAnimState.Shrink;
                stableShrinkedState = false;
            }
            SubscribeToTimer();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Starts a grow animation (on mouse up). Can transition from other animation in progress.
        /// </summary>
        private void doStartAnimGrow()
        {
            // Only bother if there is an image
            if (image == null) return;
            lock (animLO)
            {
                if (sizeAnimState == SizeAnimState.None)
                {
                    // Only grow back if image is permanently shrunk. Would be ugly to start growing
                    // from small when we're actually large.
                    if (!stableShrinkedState) return;
                    sizeAnimVal = 0;
                }
                else if (sizeAnimState == SizeAnimState.ShrinkBeforeGrow || sizeAnimState == SizeAnimState.Shrink)
                    sizeAnimVal = -sizeAnimVal;
                sizeAnimState = SizeAnimState.Grow;
                stableShrinkedState = false;
            }
            SubscribeToTimer();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Start to warm up (to get glow when mouse enters)
        /// </summary>
        private void doStartAnimWarm()
        {
            // Only bother if we have different BG colors
            if (backColor == hoverBackColor && backColor == pressedBackColor) return;
            lock (animLO)
            {
                // If we're already warming to any target, won't start again
                if (heatAnimState == HeatAnimState.HeatUp) return;
                // In the middle of quickly heating up (mouse press) w/o bounceback
                // Nothing to do; should not even happen
                if (heatAnimState == HeatAnimState.HeatUpFast && !heatAutoCoolBack) return;
                // No animation: start to war,
                if (heatAnimState == HeatAnimState.None)
                {
                    heatAnimVal = 0;
                    heatStart = heatCurrent;
                    heatTarget = hoverBackColor;
                    heatAnimState = HeatAnimState.HeatUp;
                }
                // Cooling down: reverse
                else if (heatAnimState == HeatAnimState.CoolDown)
                {
                    heatAnimVal = 1 - heatAnimVal;
                    heatStart = heatTarget;
                    heatTarget = hoverBackColor;
                    heatAnimState = HeatAnimState.HeatUp;
                }
                // In the middle of a bounceback heatup: make sure we'll cool back to hover
                else if (heatAnimState == HeatAnimState.HeatUpFast && heatAutoCoolBack)
                {
                    heatCoolBackTo = hoverBackColor;
                }
                // Cooling down fast from fast heatup: restart from quick cool from current color, go to hover
                else if (heatAnimState == HeatAnimState.CoolDownFast)
                {
                    heatAnimVal = 0;
                    heatStart = heatCurrent;
                    heatTarget = hoverBackColor;
                }
            }
            SubscribeToTimer();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Start to cool down (to get to normal, no-hover heat)
        /// </summary>
        private void doStartAnimCool()
        {
            // Only bother if we have different BG colors
            if (backColor == hoverBackColor && backColor == pressedBackColor) return;
            lock (animLO)
            {
                // If we're already cooling down: nothing to do
                if (heatAnimState == HeatAnimState.CoolDown) return;
                // Heating up fast without bounceback
                // Not animating
                // Start cool from zero, cool to normal
                if ((heatAnimState == HeatAnimState.HeatUpFast && !heatAutoCoolBack) ||
                    heatAnimState == HeatAnimState.None)
                {
                    heatAnimVal = 0;
                    heatStart = heatCurrent;
                    heatTarget = backColor;
                    heatAnimState = HeatAnimState.CoolDown;
                }
                // Heating up fast with bounceback: make sure we'll cool down to BG color
                else if (heatAnimState == HeatAnimState.HeatUpFast && heatAutoCoolBack)
                {
                    heatCoolBackTo = backColor;
                }
                // Cooling down fast: start coolig slowly, to BG color
                else if (heatAnimState == HeatAnimState.CoolDownFast)
                {
                    heatAnimVal = 0;
                    heatStart = heatCurrent;
                    heatTarget = backColor;
                    heatAnimState = HeatAnimState.CoolDown;
                }
                // Heating up slowly: reverse
                else if (heatAnimState == HeatAnimState.HeatUp)
                {
                    heatAnimVal = 1 - heatAnimVal;
                    heatStart = heatTarget;
                    heatTarget = backColor;
                    heatAnimState = HeatAnimState.CoolDown;
                }
            }
            SubscribeToTimer();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        private void doStartAnimHeatPress()
        {
            // Only bother if we have different BG colors
            if (backColor == hoverBackColor && backColor == pressedBackColor) return;
            lock (animLO)
            {
                // If we're already heating fast: nothing to do
                if (heatAnimState == HeatAnimState.HeatUpFast) return;
                // Go from current to hottest, fast
                heatAnimState = HeatAnimState.HeatUpFast;
                heatStart = heatCurrent;
                heatTarget = pressedBackColor;
                heatAnimVal = 0;
            }
            SubscribeToTimer();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        private void doStartAnimHeatRelease()
        {
            // Only bother if we have different BG colors
            if (backColor == hoverBackColor && backColor == pressedBackColor) return;
            lock (animLO)
            {
                // If we're already heating fast: make sure we return to hover color
                if (heatAnimState == HeatAnimState.HeatUpFast)
                {
                    heatAutoCoolBack = true;
                    heatCoolBackTo = hoverBackColor;
                }
                // Go from current to hover, fast
                else
                {
                    heatAnimState = HeatAnimState.CoolDownFast;
                    heatStart = heatCurrent;
                    heatTarget = hoverBackColor;
                    heatAnimVal = 0;
                }
            }
            SubscribeToTimer();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        private bool doTimerHeat()
        {
            // Done: tell we don't need timer no more.
            if (heatAnimState == HeatAnimState.None) return false;
            if (heatAnimState == HeatAnimState.HeatUpFast || heatAnimState == HeatAnimState.CoolDownFast)
                heatAnimVal += 0.2;
            else
                heatAnimVal += 0.1;
            if (heatAnimVal >= 1.0)
            {
                if (heatAnimState == HeatAnimState.HeatUpFast && heatAutoCoolBack)
                {
                    heatAutoCoolBack = false;
                    heatAnimVal = 0;
                    heatStart = heatCurrent;
                    heatTarget = heatCoolBackTo;
                }
                else
                {
                    heatAnimState = HeatAnimState.None;
                    heatCurrent = heatTarget;
                }
            }
            else
            {
                double a = ((double)heatStart.A) + heatAnimVal * ((double)(heatTarget.A - heatStart.A));
                double r = ((double)heatStart.R) + heatAnimVal * ((double)(heatTarget.R - heatStart.R));
                double g = ((double)heatStart.G) + heatAnimVal * ((double)(heatTarget.G - heatStart.G));
                double b = ((double)heatStart.B) + heatAnimVal * ((double)(heatTarget.B - heatStart.B));
                heatCurrent = Color.FromArgb((int)a, (int)r, (int)g, (int)b);
            }
            // Keep up the timer
            return true;
        }

        /// <summary>
        /// <para>Nudges size animation to next state.</para>
        /// <para>Must be called from within critical section through lock object.</para>
        /// </summary>
        /// <returns>True if timer is still needed; false otherwise.</returns>
        private bool doTimerSize()
        {
            // Done: say we don't need timer no more.
            if (sizeAnimState == SizeAnimState.None) return false;
            // Shrinking
            if (sizeAnimState == SizeAnimState.Shrink || sizeAnimState == SizeAnimState.ShrinkBeforeGrow)
            {
                sizeAnimVal += 0.2;
                if (sizeAnimVal >= 0)
                {
                    if (sizeAnimState == SizeAnimState.Shrink)
                    {
                        sizeAnimState = SizeAnimState.None;
                        sizeAnimVal = double.MinValue;
                        stableShrinkedState = true;
                    }
                    else sizeAnimState = SizeAnimState.GrowAfterShrink;
                }
            }
            // Growing
            else if (sizeAnimState == SizeAnimState.Grow || sizeAnimState == SizeAnimState.GrowAfterShrink)
            {
                sizeAnimVal += 0.2;
                if (sizeAnimVal >= 1)
                {
                    sizeAnimState = SizeAnimState.None;
                    sizeAnimVal = double.MinValue;
                }
            }
            // Keep up the timer
            return true;
        }

        /// <summary>
        /// Handles timer tick: nudges animation on to next state.
        /// </summary>
        public override void DoTimer()
        {
            lock (animLO)
            {
                bool timerNeeded = false;
                timerNeeded |= doTimerSize();
                timerNeeded |= doTimerHeat();
                if (!timerNeeded) UnsubscribeFromTimer();
            }
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Handles (or rather: consciously ignores) mouse click. We emit our click on mouse up.
        /// </summary>
        public override bool DoMouseClick(Point p, MouseButtons button)
        {
            // Make sure control's base does not send click event
            // We already did in MouseUp
            return visible;
        }

        /// <summary>
        /// Handles mouse down (triggers required animations).
        /// </summary>
        public override bool DoMouseDown(Point p, MouseButtons button)
        {
            if (!Visible) return false;
            doStartAnimShrink();
            doStartAnimHeatPress();
            return true;
        }

        /// <summary>
        /// Handles mouse up (triggers required animations). Emits click.
        /// </summary>
        public override bool DoMouseUp(Point p, MouseButtons button)
        {
            if (!Visible) return false;
            doStartAnimShrinkGrow();
            doStartAnimHeatRelease();
            FireClick();
            return true;
        }

        /// <summary>
        /// Handles mouse enter (triggers required animations).
        /// </summary>
        public override void DoMouseEnter()
        {
            if (!Visible) return;
            doStartAnimShrinkGrow();
            doStartAnimWarm();
        }

        /// <summary>
        /// Handles mouse leave (triggers animation to return to normal).
        /// </summary>
        public override void DoMouseLeave()
        {
            if (!Visible) return;
            // This is only really needed if mouse leaves button while shrinking or permanently shrunk.
            doStartAnimGrow();
            doStartAnimCool();
        }

        /// <summary>
        /// Paints control.
        /// </summary>
        public override void DoPaint(Graphics g)
        {
            // If not visible: no painting.
            if (!visible) return;

            // Proportion (shrink effect)
            // Current heat
            float prop;
            Color bgCol;
            getAnimValues(out prop, out bgCol);

            // Background: solid white, for now
            // This will definitely evolve
            using (Brush b = new SolidBrush(bgCol))
            {
                g.FillRectangle(b, 0, 0, Width, Height);
            }
            // Border, if requested
            if (hasBorder)
            {
                using (Pen p = new Pen(ZenParams.BorderColor))
                {
                    g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
                }
            }
            // Image, if we have one
            if (image != null)
            {
                int border = hasBorder ? 1 : 0;
                RectangleF imgRect;
                // Yes, there is shrinkging: calculate
                if (prop != 1.0)
                {
                    float sz = (float)(Height - 2 * (padding + border));
                    sz *= prop;
                    float szHalf = sz / 2.0F;
                    float imgMid = ((float)(Height + padding)) / 2.0F;
                    imgRect = new RectangleF(imgMid - szHalf, imgMid - szHalf, sz, sz);
                }
                // Full size: easier
                else
                {
                    imgRect = new RectangleF(padding + border, padding + border,
                       Height - 2 * (padding + border), Height - 2 * (padding + border));
                }
                // Draw image, shrunk to rectangle
                g.DrawImage(image, imgRect);
            }
            // There is text. This we never shrink. Tried it, looks ugly.
            if (text != string.Empty)
            {
                // To the right of image
                int txtLeft = (int)Math.Round((((float)Width) - textSize.Width) / 2.0F);
                if (image != null) txtLeft += Height - padding;
                // Aligner to center, both horizontally & vertically
                // Take proportion (shrink effect) into consideration
                int txtTop = (int)(((float)Height) * 0.5F - (textSize.Height / 2.0F));
                RectangleF textRect = new RectangleF(txtLeft, txtTop, textSize.Width, textSize.Height);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                StringFormat sf = StringFormat.GenericTypographic;
                using (Brush b = new SolidBrush(Color.Black))
                {
                    g.DrawString(text, fntText, b, textRect, sf);
                }
            }
        }
    }
}
