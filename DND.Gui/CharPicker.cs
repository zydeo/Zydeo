using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using DND.Common;
using DND.Gui.Zen;

namespace DND.Gui
{
    public class CharPicker : ZenControl
    {
        public delegate void CharPickedDelegate(char c);
        public event CharPickedDelegate CharPicked;

        private char[] items = new char[0];

        private string fontFace = "SimSun";
        private readonly List<RectangleF> charRects;
        private SizeF charSize;
        float charOfsX;
        float charOfsY;
        private Font font;
        private int hoverRectIx = -1;

        public CharPicker(ZenControl owner)
            : base(owner)
        {
            charRects = new List<RectangleF>();
            for (int i = 0; i != 10; ++i) charRects.Add(new RectangleF(0, 0, 0, 0));
        }

        public string FontFace
        {
            get { return fontFace; }
            set { fontFace = value; calibrateFont(); }
        }

        public override void Dispose()
        {
            if (font != null) font.Dispose();
            base.Dispose();
        }

        public void SetItems(char[] items)
        {
            if (items == null) throw new ArgumentNullException("items");
            this.items = items;
            MakeMePaint(false, RenderMode.Invalidate);
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            calibrateFont();
        }

        private void calibrateFont()
        {
            float width = Width;
            float fontSize = 10.0F;
            float descentPixels;

            using (Bitmap bmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                StringFormat sf = StringFormat.GenericTypographic;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                while (true)
                {
                    using (Font fnt = new Font(fontFace, fontSize))
                    {
                        charSize = g.MeasureString("中", fnt, 65535, sf);
                    }
                    if (charSize.Width * 5.0F >= width * 0.8F) break;
                    fontSize += 0.5F;
                }

                if (font != null) font.Dispose(); font = null;
                font = new Font(fontFace, fontSize);
                int descent = font.FontFamily.GetCellDescent(FontStyle.Regular);
                descentPixels = font.Size * ((float)descent) /
                    ((float)font.FontFamily.GetEmHeight(FontStyle.Regular));
                descentPixels *= Scale;
            }

            float rectWidth = (width - 2.0F) / 5.0F;
            float rectHeight = charSize.Height;
            for (int i = 0; i != 5; ++i)
            {
                float x = ((float)i) * rectWidth + 1.0F;
                RectangleF rtop = new RectangleF(x, 1.0F, rectWidth, rectHeight);
                RectangleF rbot = new RectangleF(x, rectHeight + 1.0F, rectWidth, rectHeight);
                charRects[i] = rtop;
                charRects[i + 5] = rbot;
            }
            charOfsX = (rectWidth - charSize.Width) / 2.0F;
            charOfsY = descentPixels / 1.5F;
            Height = (int)Math.Round((rectHeight) * 2.0F + 0.5F);
            MakeMePaint(true, RenderMode.Invalidate);
        }

        public override bool DoMouseMove(Point p, System.Windows.Forms.MouseButtons button)
        {
            int ix = -1;
            for (int i = 0; i != charRects.Count; ++i)
            {
                if (charRects[i].Contains(p)) { ix = i; break; }
            }
            if (ix != hoverRectIx)
            {
                hoverRectIx = ix;
                MakeMePaint(false, RenderMode.Invalidate);
            }
            return true;
        }

        public override void DoMouseLeave()
        {
            if (hoverRectIx != -1)
            {
                hoverRectIx = -1;
                MakeMePaint(false, RenderMode.Invalidate);
            }
        }

        public override void DoPaint(System.Drawing.Graphics g)
        {
            // Background
            using (Brush b = new SolidBrush(Color.White))
            {
                g.FillRectangle(b, 0, 0, Width, Height);
            }
            // Characters
            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            using (Brush b = new SolidBrush(Color.Black))
            {
                for (int i = 0; i != charRects.Count; ++i)
                {
                    RectangleF rect = charRects[i];
                    // Background
                    Color col = Color.White;
                    if (hoverRectIx == i) col = Color.FromArgb(240, 240, 240);
                    using (Brush bgb = new SolidBrush(col))
                    {
                        g.FillRectangle(bgb, rect);
                    }
                    // Draw character, if any
                    if (i >= items.Length) continue;
                    string str = ""; str += items[i];
                    g.DrawString(str, font, b, rect.X + charOfsX, rect.Y + charOfsY, sf);
                }
            }
            // Border
            using (Pen p = new Pen(SystemColors.ControlDarkDark))
            {
                g.DrawLine(p, 0, 0, Width, 0);
                g.DrawLine(p, Width - 1, 0, Width - 1, Height);
                g.DrawLine(p, Width - 1, Height - 1, 0, Height - 1);
                g.DrawLine(p, 0, Height - 1, 0, 0);
            }
        }

        public override bool DoMouseClick(Point p, System.Windows.Forms.MouseButtons button)
        {
            if (CharPicked != null) CharPicked('你');
            return true;
        }
    }
}
