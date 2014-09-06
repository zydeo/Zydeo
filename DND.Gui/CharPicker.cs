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
            if (items == null) items = new char[0];
            List<char> filteredItems = new List<char>();
            foreach (char c in items)
            {
                if (char.IsLetter(c)) filteredItems.Add(c);
                if (filteredItems.Count == 10) break;
            }
            this.items = filteredItems.ToArray();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            calibrateFont();
        }

        /// <summary>
        /// <para>Finds the right font size to fit characters (2x5 with default size, but it varies).</para>
        /// <para>Finds right vertical area based on font's actual display properties.</para>
        /// </summary>
        private void calibrateFont()
        {
            float width = Width;
            float fontSize = 10.0F;

            // Measuring artefacts
            using (Bitmap bmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                StringFormat sf = StringFormat.GenericTypographic;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Keep growing font until we reach a comfortable width
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
            }

            // Width of rectangle: using my space equally
            float rectWidth = (width - 2.0F) / 5.0F;
            // Height of rectange: depends on font's actual drawing behavior!
            var si = HanziMeasure.Instance.GetMeasures(fontFace, fontSize);
            float rectHeight = si.RealRect.Bottom + si.RealRect.Top;
            // Horizontal padding is rectangle width minus measured char width, over two
            float hPad = (rectWidth - si.AllegedSize.Width) / 2.0F;
            // Add twice horizontal padding to rectangle height; offset chars from top by padding
            rectHeight += 3.0F * hPad;

            for (int i = 0; i != 5; ++i)
            {
                float x = ((float)i) * rectWidth + 1.0F;
                RectangleF rtop = new RectangleF(x, 1.0F, rectWidth, rectHeight);
                RectangleF rbot = new RectangleF(x, rectHeight + 1.0F, rectWidth, rectHeight);
                charRects[i] = rtop;
                charRects[i + 5] = rbot;
            }
            charOfsX = (rectWidth - charSize.Width) / 2.0F;
            charOfsY = 1.5F * hPad;
            Height = (int)Math.Round((rectHeight) * 2.0F + 0.5F);
            MakeMePaint(true, RenderMode.Invalidate);
        }

        private int getCharRectIx(Point p)
        {
            int ix = -1;
            for (int i = 0; i != charRects.Count; ++i)
            {
                if (charRects[i].Contains(p)) { ix = i; break; }
            }
            return ix;
        }

        public override bool DoMouseMove(Point p, System.Windows.Forms.MouseButtons button)
        {
            int ix = getCharRectIx(p);
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
                    if (hoverRectIx == i && i < items.Length) col = Color.FromArgb(240, 240, 240);
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
            using (Pen p = new Pen(ZenParams.BorderColor))
            {
                g.DrawLine(p, 0, 0, Width, 0);
                g.DrawLine(p, Width - 1, 0, Width - 1, Height);
                g.DrawLine(p, Width - 1, Height - 1, 0, Height - 1);
                g.DrawLine(p, 0, Height - 1, 0, 0);
            }
        }

        public override bool DoMouseClick(Point p, System.Windows.Forms.MouseButtons button)
        {
            int ix = getCharRectIx(p);
            if (ix == -1) return true;
            if (ix >= items.Length) return true;
            if (CharPicked != null) CharPicked(items[ix]);
            return true;
        }
    }
}
