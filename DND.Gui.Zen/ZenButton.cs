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
        private bool visible = true;
        private bool hasBorder = true;
        private int padding = 0;
        private Image image = null;
        private string text = string.Empty;
        private float fontSize = 12.0F;
        private SizeF textSize = new SizeF(0, 0);
        private Font fntText;

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

        private enum SizeAnimState
        {
            None,
            ShrinkBeforeGrow,
            GrowAfterShrink,
            Shrink,
            Grow,
        }
        private readonly object animLO = new object();
        private const double smallProp = 0.8;
        private SizeAnimState sizeAnimState = SizeAnimState.None;
        private double sizeAnimVal = double.MinValue;
        private bool stableShrinkedState = false;

        private float getAnimSizeNow()
        {
            lock (animLO)
            {
                if (sizeAnimVal == double.MinValue) return stableShrinkedState ? (float)smallProp : 1.0F;
                double val;
                val = 1.0 - Math.Pow(sizeAnimVal, 2.0);
                val = (Math.Cos(sizeAnimVal * Math.PI) + 1.0) / 2.0;
                val *= (1.0 - smallProp);
                float res = (float)(1.0 - val);
                return res;
            }
        }

        private void doStartAnimShrinkGrow()
        {
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

        private void doStartAnimShrink()
        {
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

        private void doStartAnimGrow()
        {
            lock (animLO)
            {
                if (sizeAnimState == SizeAnimState.None)
                    sizeAnimVal = 0;
                else if (sizeAnimState == SizeAnimState.ShrinkBeforeGrow || sizeAnimState == SizeAnimState.Shrink)
                    sizeAnimVal = -sizeAnimVal;
                sizeAnimState = SizeAnimState.Grow;
                stableShrinkedState = false;
            }
            SubscribeToTimer();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        public override void DoTimer()
        {
            lock (animLO)
            {
                if (sizeAnimState == SizeAnimState.None)
                {
                    UnsubscribeFromTimer();
                    return;
                }
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
                else if (sizeAnimState == SizeAnimState.Grow || sizeAnimState == SizeAnimState.GrowAfterShrink)
                {
                    sizeAnimVal += 0.2;
                    if (sizeAnimVal >= 1)
                    {
                        sizeAnimState = SizeAnimState.None;
                        sizeAnimVal = double.MinValue;
                    }
                }
            }
            MakeMePaint(false, RenderMode.Invalidate);
        }

        public override bool DoMouseClick(Point p, MouseButtons button)
        {
            // Make sure control's base does not send click event
            // We already did in MouseUp
            return visible;
        }

        public override bool DoMouseDown(Point p, MouseButtons button)
        {
            if (!Visible) return false;
            doStartAnimShrink();
            return true;
        }

        public override bool DoMouseUp(Point p, MouseButtons button)
        {
            if (!Visible) return false;
            doStartAnimShrinkGrow();
            FireClick();
            return true;
        }

        public override void DoMouseEnter()
        {
            if (!Visible) return;
            doStartAnimShrinkGrow();
        }

        // TO-DO: mouse down, mouse up

        public override void DoPaint(Graphics g)
        {
            // If not visible: no painting.
            if (!visible) return;

            // Background: solid white, for now
            // This will definitely evolve
            using (Brush b = new SolidBrush(Color.White))
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
                double asNow = getAnimSizeNow();
                if (asNow != 1.0)
                {
                    float sz = (float)(Height - 2 * (padding + border));
                    sz *= getAnimSizeNow();
                    float szHalf = sz / 2.0F;
                    float imgMid = ((float)(Height + padding)) / 2.0F;
                    imgRect = new RectangleF(imgMid - szHalf, imgMid - szHalf, sz, sz);
                }
                else
                {
                    imgRect = new RectangleF(padding + border, padding + border,
                       Height - 2 * (padding + border), Height - 2 * (padding + border));
                }
                g.DrawImage(image, imgRect);
            }
            // Text, if there is one
            if (text != string.Empty)
            {
                // To the right of image
                int txtLeft = (int)Math.Round((((float)Width) - textSize.Width) / 2.0F);
                if (image != null) txtLeft += Height - padding;
                // Aligner to center, both horizontally & vertically
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
