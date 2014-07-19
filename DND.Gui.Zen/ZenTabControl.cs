using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace DND.Gui.Zen
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

        public ZenTabControl(float scale, ZenTabbedForm owner, bool isMain)
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
                MakeMePaint(false, RenderMode.Invalidate);
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
                    MakeMePaint(false, RenderMode.Invalidate);
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
                return textWidth + (int)(2.0F * Scale * ZenParams.HeaderTabPadding);
            }
        }

        public override bool DoMouseEnter()
        {
            isHover = true;
            MakeMePaint(false, RenderMode.Invalidate);
            return true;
        }

        public override bool DoMouseLeave()
        {
            isHover = false;
            MakeMePaint(false, RenderMode.Invalidate);
            return true;
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
                g.FillRectangle(b, AbsLocation.X, AbsLocation.Y, Size.Width, Size.Height);
            }
            using (Pen p = new Pen(borderColor))
            {
                g.DrawLine(p, AbsLocation.X, AbsLocation.Y, AbsLocation.X + Width, AbsLocation.Y);
                g.DrawLine(p, AbsLocation.X + Width - 1, AbsLocation.Y, AbsLocation.X + Width - 1, AbsLocation.Y + Height - 1);
            }
            using (Brush b = new SolidBrush(textColor))
            {
                float x = AbsLocation.X + ZenParams.HeaderTabPadding* Scale;
                float y = AbsLocation.Y + (((float)Height) - textHeight) / 2.0F;
                g.DrawString(text, font, b, new PointF(x, y));
            }
        }

    }
}
