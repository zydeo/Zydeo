using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace ZD.Gui.Zen
{
    /// <summary>
    /// A button with animated gradient background color.
    /// </summary>
    public class ZenGradientButton : ZenControl
    {
        /// <summary>
        /// Padding from edge, in real (absolute) pixels.
        /// </summary>
        private int padding = 0;

        /// <summary>
        /// See <see cref="ImageExtraPadding"/>.
        /// </summary>
        private int imageExtraPadding = 0;

        /// <summary>
        /// Font face of button text.
        /// </summary>
        private string fontFace = ZenParams.GenericFontFamily;

        /// <summary>
        /// Font size of button text.
        /// </summary>
        private float fontSize = ZenParams.StandardFontSize;

        /// <summary>
        /// Cached font for drawing text; owned here.
        /// </summary>
        private Font fntText;

        /// <summary>
        /// The button's label.
        /// </summary>
        private string text = string.Empty;

        /// <summary>
        /// Measured and cached size of text.
        /// </summary>
        private SizeF textSize = new SizeF(0, 0);

        /// <summary>
        /// If specified, use this height for aligning label - needed for Chinese text.
        /// </summary>
        private float forcedCharHeight = 0;

        /// <summary>
        /// If specified, offset seeingly idealy position of text - needed for Chinese text.
        /// </summary>
        private float forcedCharVertOfs = 0;

        /// <summary>
        /// Image to show on button.
        /// </summary>
        private Image image = null;

        /// <summary>
        /// Disabled image: calculated on the fly by greyscaling.
        /// </summary>
        private Image disabledImage = null;

        /// <summary>
        /// Whether or not button is enabled.
        /// </summary>
        private bool enabled = true;

        /// <summary>
        /// If true, text is drawn with only Anti-Alis, not ClearType Grid Fit.
        /// </summary>
        private bool onlyAntiAlias = false;

        /// <summary>
        /// Ctor: take parent.
        /// </summary>
        public ZenGradientButton(ZenControlBase parent)
            : base(parent)
        {
            fntText = new Font(fontFace, fontSize);
        }

        /// <summary>
        /// Dispose: free owned resources.
        /// </summary>
        public override void Dispose()
        {
            if (fntText != null) fntText.Dispose();
            if (image != null) image.Dispose();
            if (disabledImage != null) image.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Sets the button's display font for showing the text.
        /// </summary>
        public void SetFont(string fontFace, float fontSize)
        {
            if (fntText != null) fntText.Dispose();
            fntText = null;
            this.fontFace = fontFace;
            this.fontSize = fontSize;
            fntText = new Font(fontFace, fontSize);
            textSize = measure(text);
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Gets the font face for showing the button's text.
        /// </summary>
        public string FontFace
        {
            get { return fontFace; }
        }

        /// <summary>
        /// Gets the font size for showing the button's text.
        /// </summary>
        public float FontSize
        {
            get { return fontSize; }
        }

        /// <summary>
        /// Gets or sets the button's display text.
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                this.text = value == null ? string.Empty : value;
                textSize = measure(text);
                // Text setter can be called from animation thread; repaint request can lead to deadlock
                //MakeMePaint(false, RenderMode.Invalidate);
            }
        }

        /// <summary>
        /// Sets known real height of Hanzi characters for correct vertical alignment of text.
        /// </summary>
        public float ForcedCharHeight
        {
            set { forcedCharHeight = value; }
        }

        /// <summary>
        /// Set vertical offset of text, for correct display of Hanzi.
        /// </summary>
        public float ForcedCharVertOfs
        {
            set { forcedCharVertOfs = value; }
        }

        /// <summary>
        /// Gets or sets image. Button takes ownership: will dispose image.
        /// </summary>
        public Image Image
        {
            get { return image; }
            set
            {
                if (image != null) { image.Dispose(); image = null; }
                if (disabledImage != null) { disabledImage.Dispose(); image = null; }
                image = value;
                disabledImage = makeDisabledImage(image);
                //MakeMePaint(false, RenderMode.Invalidate);
            }
        }

        /// <summary>
        /// Gets or sets button's padding (from edge, not from border). Affects image scaling. Real pixels.
        /// </summary>
        public int Padding
        {
            get { return padding; }
            set
            {
                padding = value;
                //MakeMePaint(false, RenderMode.Invalidate);
            }
        }

        /// <summary>
        /// Extra padding around image, within button's normal <see cref="Padding"/>. Real pixels.
        /// </summary>
        public int ImageExtraPadding
        {
            get { return imageExtraPadding; }
            set
            {
                imageExtraPadding = value;
            }
        }

        /// <summary>
        /// Gets or sets whether button is enabled.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                // Kill any animation that may be in progress
                lock (animLO)
                {
                    pressAnimState = hoverAnimState = 0;
                    pressAnimVal = hoverAnimVal = 0;
                }
                enabled = value;
                MakeMePaint(false, RenderMode.Invalidate);
            }
        }

        /// <summary>
        /// If true, text is drawn with only Anti-Alis, not ClearType Grid Fit.
        /// </summary>
        public bool OnlyAntiAlias
        {
            get { return onlyAntiAlias; }
            set { onlyAntiAlias = value; }
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
            // Otherwise, padded image takes up control height on left; plus text; plus pad right
            return Height + w + padding;
        }

        /// <summary>
        /// Flashes up button (animation) as if it were clicked.
        /// </summary>
        public void Flash()
        {
            if (!enabled) return;
            doStartPressAnim(PressType.Flash);
        }

        /// <summary>
        /// Handles mouse enter: transition into hover state.
        /// </summary>
        public override void DoMouseEnter()
        {
            if (!enabled) return;
            base.DoMouseEnter();
            doStartHoverAnim(true);
        }

        /// <summary>
        /// Handles mouse leave: transition out of hover/pressed state.
        /// </summary>
        public override void DoMouseLeave()
        {
            if (!enabled) return;
            base.DoMouseLeave();
            doStartHoverAnim(false);
            doStartPressAnim(PressType.NotPressed);
        }

        /// <summary>
        /// Handles mouse click.
        /// </summary>
        public override bool DoMouseClick(Point p, MouseButtons button)
        {
            // Hide tooltip if it wants to be hidden on click
            IZenTooltip tt = Tooltip;
            if (tt != null && tt.HideOnClick) base.KillTooltip();
            // Prevent base class from handling click - we already sent Click event from mouse up.
            return true;
        }

        /// <summary>
        /// Handles mouse down: transitions into pressed state.
        /// </summary>
        public override bool DoMouseDown(Point p, MouseButtons button)
        {
            if (!enabled) return true;
            doStartPressAnim(PressType.Pressed);
            return true;
        }

        /// <summary>
        /// Handles mouse up: broadcasts click event, transitions out of pressed state.
        /// </summary>
        public override bool DoMouseUp(Point p, MouseButtons button)
        {
            if (!enabled) return true;
            doStartPressAnim(PressType.NotPressed);
            FireClick();
            return true;
        }

        /// <summary>
        /// Lock object around all subsequent animation-related members.
        /// </summary>
        private object animLO = new object();

        /// <summary>
        /// Between 0 and 100; 0 means base state, 100 means fully achieved hover state.
        /// </summary>
        private int hoverAnimVal = 0;

        /// <summary>
        /// State of the hover animation. 0: none; 1: transitioning into hover; -1: transitioning back to base.
        /// </summary>
        private int hoverAnimState = 0;

        /// <summary>
        /// Between 0 and 100; 0 means not pressed, 100 means fully achieved pressed state.
        /// </summary>
        private int pressAnimVal = 0;

        /// <summary>
        /// Press animation state.
        /// 1: transitioning into
        /// 2: transitioning into, but must revert when completed (continue with -1)
        /// -1: transitioning out of pressed
        /// </summary>
        private int pressAnimState = 0;

        /// <summary>
        /// Types of press/release animations.
        /// </summary>
        private enum PressType
        {
            // Button is pressed (mouse down).
            Pressed,
            // Button is released (mouse up).
            NotPressed,
            // Button must flash up.
            Flash,
        }

        /// <summary>
        /// Starts the button pressed animation.
        /// </summary>
        /// <param name="isPressed">If true, transitions into presesd; if false, transitions out of it.</param>
        private void doStartPressAnim(PressType pressType)
        {
            lock (animLO)
            {
                // If invoked but we're fully unpressed: nothing to do.
                if (pressType == PressType.NotPressed && pressAnimVal == 0) return;
                // If "not pressed" but we're just transitioning into pressed: make it a return trip.
                // Also make it a return trip if we're fully unpressed and just need to flash.
                if (pressType == PressType.NotPressed && pressAnimState == 1)
                    pressAnimState = 2;
                else if (pressType == PressType.Flash)
                    pressAnimState = 2;
                else
                    pressAnimState = pressType == PressType.Pressed ? 1 : -1;
            }
            SubscribeToTimer();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Updates the "pressed" animation's state.
        /// </summary>
        /// <returns>True if animation is still in progress, false if completed.</returns>
        private bool doTimerPress()
        {
            lock (animLO)
            {
                if (pressAnimState == 0) return false;
                if (pressAnimState >0)
                {
                    pressAnimVal += 25;
                    if (pressAnimVal > 100)
                    {
                        pressAnimVal = 100;
                        if (pressAnimState == 2) pressAnimState = -1;
                        else pressAnimState = 0;
                    }
                }
                else
                {
                    pressAnimVal -= 15;
                    if (pressAnimVal < 0)
                    {
                        pressAnimVal = 0;
                        pressAnimState = 0;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Starts the hover animation.
        /// </summary>
        /// <param name="isHover">True if current state is hover, false otherwise.</param>
        private void doStartHoverAnim(bool isHover)
        {
            lock (animLO)
            {
                hoverAnimState = isHover ? 1 : -1;
            }
            SubscribeToTimer();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Updates the "hover" animation's state.
        /// </summary>
        /// <returns>True if animation is still in progress, false if completed.</returns>
        private bool doTimerHeat()
        {
            if (hoverAnimState == 0) return false;
            if (hoverAnimState == 1)
            {
                hoverAnimVal += 15;
                if (hoverAnimVal > 100)
                {
                    hoverAnimVal = 100;
                    hoverAnimState = 0;
                }
            }
            else
            {
                hoverAnimVal -= 8;
                if (hoverAnimVal < 0)
                {
                    hoverAnimVal = 0;
                    hoverAnimState = 0;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns button's current state for painting.
        /// </summary>
        /// <param name="alfaHover">Alfa blend of hover vs. base state (255: fully in hover).</param>
        /// <param name="alfaPress">Alfa blend of pressed state.</param>
        private void getAnimValues(out int alfaHover, out int alfaPress)
        {
            lock (animLO)
            {
                // Hover alfa
                if (hoverAnimVal == 0) alfaHover = 0;
                else if (hoverAnimVal == 100) alfaHover = 255;
                else alfaHover = (int)(255.0F * ((float)hoverAnimVal) / 100.0F);
                // Press ala
                if (pressAnimVal == 0) alfaPress = 0;
                else if (pressAnimVal == 100) alfaPress = 255;
                else alfaPress = (int)(255.0F * ((float)pressAnimVal) / 100.0F);
            }
        }

        /// <summary>
        /// Handles timer event to update animations and request repaint; unsubscribes if all
        /// animations have completed.
        /// </summary>
        public override void DoTimer(out bool? needBackground, out RenderMode? renderMode)
        {
            lock (animLO)
            {
                bool timerNeeded = false;
                timerNeeded |= doTimerHeat();
                timerNeeded |= doTimerPress();
                if (!timerNeeded) UnsubscribeFromTimer();
            }
            needBackground = false;
            renderMode = RenderMode.Invalidate;
        }

        /// <summary>
        /// Measures display text with current font size.
        /// </summary>
        private SizeF measure(string text)
        {
            StringFormat sf = StringFormat.GenericTypographic;
            return MeasureText(text, fntText, sf);
        }

        private Image makeDisabledImage(Image image)
        {
            Bitmap bmp = new Bitmap(image);
            for (int x = 0; x != bmp.Width; ++x)
            {
                for (int y = 0; y != bmp.Height; ++y)
                {
                    Color c = bmp.GetPixel(x, y);
                    int avg = c.R + c.G + c.B;
                    avg /= 3;
                    bmp.SetPixel(x, y, Color.FromArgb(c.A /2, avg, avg, avg));
                }
            }
            return bmp;
        }

        /// <summary>
        /// Paints gradient background blend.
        /// </summary>
        /// <param name="g">Graphics to paint with.</param>
        /// <param name="colLight">Gradient's light color</param>
        /// <param name="colDark">Gradient's dark color</param>
        /// <param name="alfa">Alfa value to use for layer.</param>
        /// <param name="inverted">If true, light corner is top right, not bottom left.</param>
        private void doPaintGradientBg(Graphics g, Color colLight, Color colDark, byte alfa, bool inverted)
        {
            using (GraphicsPath gp = new GraphicsPath())
            {
                double sqrt2 = Math.Sqrt(2.0);
                double ewD = 2.0 * ((double)Width) * sqrt2 + 1;
                double ehD = 2.0 * ((double)Height) * sqrt2 + 1;
                int ew = (int)ewD;
                int eh = (int)ehD;
                int ex = inverted ? (Width - ew / 2) : (-ew / 2);
                int ey = inverted ? (-eh / 2) : (Height - eh / 2);
                gp.AddEllipse(ex, ey, ew, eh);
                using (PathGradientBrush pgb = new PathGradientBrush(gp))
                {
                    pgb.CenterColor = Color.FromArgb(alfa, colLight.R, colLight.G, colLight.B);
                    pgb.CenterPoint = new PointF(ex + ew / 2, ey + eh / 2);
                    pgb.SurroundColors = new Color[] { Color.FromArgb(alfa, colDark.R, colDark.G, colDark.B) };
                    g.FillRectangle(pgb, ex, ey, ew, eh);
                }
            }
        }

        /// <summary>
        /// Paints the button.
        /// </summary>
        public override void DoPaint(Graphics g)
        {
            // Current animation states: how much of base, hover and pressed to blend together.
            int hoverAlfa, pressAlfa;
            getAnimValues(out hoverAlfa, out pressAlfa);

            // Must disable smoothing for gradients to work, and to get true 1px lines.
            g.SmoothingMode = SmoothingMode.None;

            // Our area without border
            Rectangle rect = new Rectangle(1, 1, Width - 2, Height - 2);

            // Paint background white. Will alfa-blend layers on top of white.
            using (Brush b = new SolidBrush(Color.White))
            {
                g.FillRectangle(b, rect);
            }

            if (enabled)
            {
                // Three gradients, with different alfa levels depending on animation state
                doPaintGradientBg(g, ZenParams.BtnGradLightColor, ZenParams.BtnGradDarkColorBase, (byte)(255 - hoverAlfa), false);
                doPaintGradientBg(g, ZenParams.BtnGradLightColor, ZenParams.BtnGradDarkColorHover, (byte)hoverAlfa, true);
                doPaintGradientBg(g, ZenParams.BtnGradLightColor, ZenParams.BtnPressColor, (byte)pressAlfa, true);
            }
            else
            {
                // For a disabled button, only one gradient, no blending b/c no states
                doPaintGradientBg(g, ZenParams.BtnGradLightColor, ZenParams.BtnGradDarkColorDisabled, 255, false);
            }

            // Image, if we have any
            if (image != null)
            {
                RectangleF imgRect = new RectangleF(padding + imageExtraPadding, padding + imageExtraPadding,
                    Height - 2 * (padding + imageExtraPadding), Height - 2 * (padding + imageExtraPadding));
                if (enabled) g.DrawImage(image, imgRect);
                else g.DrawImage(disabledImage, imgRect);
            }

            // Text, if we have any
            if (text != string.Empty)
            {
                int txtLeft;
                // Centered if no image
                if (image == null)
                {
                    txtLeft = (int)Math.Round((((float)Width) - textSize.Width) / 2.0F);
                }
                // To the right of left-aligned image otherwise
                else
                {
                    txtLeft = Height + padding;
                }
                // Aligner to center, both horizontally & vertically
                int txtTop = (int)(((float)Height) * 0.5F - (textSize.Height / 2.0F));
                // For Hanzi, need different strategy
                if (forcedCharHeight != 0)
                    txtTop = (int)(((float)Height) * 0.5F - (forcedCharHeight / 2.0F) + forcedCharVertOfs);
                RectangleF textRect = new RectangleF(txtLeft, txtTop, textSize.Width + 1, textSize.Height + 1);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                StringFormat sf = StringFormat.GenericTypographic;
                Color txtColor = enabled ? ZenParams.StandardTextColor : ZenParams.DisabledTextColor;
                using (Brush b = new SolidBrush(txtColor))
                {
                    if (onlyAntiAlias) g.TextRenderingHint = TextRenderingHint.AntiAlias;
                    else g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    g.DrawString(text, fntText, b, textRect, sf);
                }
            }


            // Border
            using (Pen p = new Pen(ZenParams.BorderColor))
            {
                g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
            }
        }
    }
}
