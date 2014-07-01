using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace DND.Controls
{
    internal class ZenTabControl : ZenControl
    {
        private readonly bool isMain;
        private bool isHover = false;
        private string text;
        private int textWidth;
        private int textHeight;
        private Font font;
        private bool isSelected = false;

        public ZenTabControl(float scale, IZenControlOwner owner, bool isMain)
            : base(scale, owner)
        {
            this.isMain = isMain;
            font = new Font(new FontFamily(ZenParams.HeaderTabFontFamily),
                ZenParams.HeaderTabFontSize,
                FontStyle.Regular);
        }

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                SizeF size = MeasureText(text, font, StringFormat.GenericDefault);
                textWidth = (int)size.Width;
                textHeight = (int)size.Height;
                Invalidate();
            }
        }

        public bool Selected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    Invalidate();
                }
            }
        }

        public bool IsMain
        {
            get { return isMain; }
        }

        public int PreferredWidth
        {
            get
            {
                return textWidth + (int)(2.0F * scale * ZenParams.HeaderTabPadding);
            }
        }

        public override void DoMouseEnter()
        {
            base.DoMouseEnter();
            isHover = true;
            Invalidate();
        }

        public override void DoMouseLeave()
        {
            base.DoMouseLeave();
            isHover = false;
            Invalidate();
        }

        public override void DoPaint(Graphics g)
        {
            Color fillColor;
            Color borderColor;
            Color textColor;
            if (isMain)
            {
                if (isSelected || isHover)
                {
                    fillColor = Color.White;
                    borderColor = Color.Black;
                    textColor = Color.Black;
                }
                else
                {
                    fillColor = Color.Black;
                    borderColor = Color.Black;
                    textColor = Color.White;
                }
            }
            else
            {
                if (isSelected || isHover)
                {
                    fillColor = ZenParams.PaddingBackColor;
                    borderColor = Color.LightGray;
                    textColor = Color.Black;
                }
                else
                {
                    fillColor = ZenParams.HeaderBackColor;
                    borderColor = Color.LightGray;
                    textColor = Color.Black;
                }
            }
            using (Brush b = new SolidBrush(fillColor))
            {
                g.FillRectangle(b, Location.X, Location.Y, Size.Width, Size.Height);
            }
            using (Pen p = new Pen(borderColor))
            {
                g.DrawLine(p, Location.X, Location.Y, Location.X + Width, Location.Y);
                g.DrawLine(p, Location.X + Width - 1, Location.Y, Location.X + Width - 1, Location.Y + Height - 1);
            }
            using (Brush b = new SolidBrush(textColor))
            {
                float x = Location.X + ZenParams.HeaderTabPadding* scale;
                float y = Location.Y + (((float)Height) - textHeight) / 2.0F;
                g.DrawString(text, font, b, new PointF(x, y));
            }
        }

    }
}
