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

        /// <summary>
        /// Fires the mouse click event, provided the control is in a state when it can do that.
        /// </summary>
        public override bool DoMouseClick(Point p, MouseButtons button)
        {
            if (visible) FireClick();
            return visible;
        }

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
                Rectangle imgRect = new Rectangle(padding + border, padding + border,
                    Height - 2 * (padding + border), Height - 2 * (padding + border));
                g.DrawImage(image, imgRect);
            }
        }
    }
}
