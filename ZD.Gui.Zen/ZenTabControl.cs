using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;

namespace ZD.Gui.Zen
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

        public ZenTabControl(ZenTabbedForm owner, bool isMain)
            : base(owner)
        {
            this.isMain = isMain;
            font = SystemFontProvider.Instance.GetSystemFont(FontStyle.Regular, ZenParams.HeaderTabFontSize);
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

        public override void DoMouseEnter()
        {
            isHover = true;
            MakeMePaint(false, RenderMode.Invalidate);
        }

        public override void DoMouseLeave()
        {
            isHover = false;
            MakeMePaint(false, RenderMode.Invalidate);
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
                    fillColor = ZenParams.TabSysBgActive;
                    borderColor = ZenParams.TabSysBgActive;
                    textColor = ZenParams.TabSysTxtActive;
                }
                else
                {
                    fillColor = ZenParams.TabSysBgBase;
                    borderColor = ZenParams.TabSysBgBase;
                    textColor = ZenParams.TabSysTxtBase;
                }
            }
            else
            {
                if (isSelected || isHover)
                {
                    fillColor = ZenParams.TabOtherBgActive;
                    borderColor = ZenParams.TabOtherBgActive;
                    textColor = ZenParams.TabOtherTxtActive;
                }
                else
                {
                    fillColor = ZenParams.TabOtherBgBase;
                    borderColor = ZenParams.TabOtherBgActive;
                    textColor = ZenParams.TabOtherTxtBase;
                }
            }
            using (Brush b = new SolidBrush(fillColor))
            {
                g.FillRectangle(b, 0, 0, Width, Height);
            }
            using (Pen p = new Pen(borderColor))
            {
                g.DrawLine(p, 0, 0, Width, 0);
                g.DrawLine(p, Width - 1, 0, Width - 1, Height - 1);
            }
            using (Brush b = new SolidBrush(textColor))
            {
                float x = ZenParams.HeaderTabPadding* Scale;
                float y = (((float)Height) - textHeight) / 2.0F;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.DrawString(text, font, b, new PointF(x, y));
            }
        }

    }
}
