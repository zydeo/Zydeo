using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

using DND.Common;

namespace DND.Controls
{
    internal class OneResultControl : ZenControl
    {
        public int LastTop = int.MinValue;
        public readonly CedictResult Res;

        private int analyzedWidth = int.MinValue;
        private static Size zhoCharSize = new Size(0, 0);

        public OneResultControl(float scale, IZenControlOwner owner, CedictResult cr)
            : base(scale, owner)
        {
            this.Res = cr;
        }

        // Graphics resource: static, singleton, never disposed
        private static Font fntSimp;
        private static Font fntTrad;
        private static Font fntPinyin;
        private static Font fntLemma;

        static OneResultControl()
        {
            fntSimp = new Font(ZenParams.SimpFontFamily, ZenParams.ZhoFontSize);
            fntTrad = new Font(ZenParams.TradFontFamily, ZenParams.ZhoFontSize);
            fntPinyin = new Font(ZenParams.LatnFontFamily, ZenParams.PinyinFontSize, FontStyle.Bold);
            fntLemma = new Font(ZenParams.LatnFontFamily, ZenParams.LemmaFontSize);
        }

        public override void DoPaint(System.Drawing.Graphics g)
        {
            if (analyzedWidth != Width) Analyze(g, Width);
            for (int i = 0; i != Height; ++i)
            {
                using (Pen p = new Pen(Color.FromArgb(240 - i, 240 - i, 240 - i)))
                {
                    g.DrawLine(p, Left, Top + i, Left + Width, Top + i);
                }
            }
            using (Brush b = new SolidBrush(Color.Black))
            {
                g.DrawString(Res.Entry.ChSimpl, fntSimp, b, (float)Left + 30.0F, (float)Top + 5.0F);
            }
        }

        // Analyze layout with provided width; assume corresponding height; does not invalidate
        public void Analyze(Graphics g, int width)
        {
            analyzedWidth = Width;
            if (zhoCharSize.Width == 0)
            {
                StringFormat sf = StringFormat.GenericTypographic;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                SizeF sz = g.MeasureString("中", fntSimp, 1000, sf);
                zhoCharSize = new Size((int)sz.Width, (int)sz.Height);
            }
            int height = 72;
            SetSizeNoInvalidate(new Size(width, height));
        }
    }
}
