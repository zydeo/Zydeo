using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ZD.FontTest
{
    public partial class PrettyTestCtrl : Control
    {
        private float scale;
        private const float xpad = 20F;
        private FontTray fntSimpA;
        private FontTray fntTradA;
        private FontTray fntSimpN;
        private FontTray fntTradN;
        private Font fntLatn;
        private string txtSimp = "我英语给您";
        private string txtTrad = "我英語給您";
        private const string txtLatn = "Gyfitex";

        public PrettyTestCtrl()
        {
            InitializeComponent();
        }

        public float DpiScale
        {
            set { scale = value; }
        }

        public void SetFonts(string familyLatn, float sz)
        {
            fntSimpA = FontPool.GetFont(IdeoFont.ArphicKai, SimpTradFont.Simp, sz, FontStyle.Regular);
            fntTradA = FontPool.GetFont(IdeoFont.ArphicKai, SimpTradFont.Trad, sz, FontStyle.Regular);
            //fntSimpN = FontPool.GetFont(IdeoFont.Noto, SimpTradFont.Simp, sz, FontStyle.Regular);
            //fntTradN = FontPool.GetFont(IdeoFont.Noto, SimpTradFont.Trad, sz, FontStyle.Regular);
            fntSimpN = FontPool.GetFont(IdeoFont.WinKai, SimpTradFont.Simp, sz, FontStyle.Regular);
            fntTradN = FontPool.GetFont(IdeoFont.WinKai, SimpTradFont.Trad, sz, FontStyle.Regular);
            fntLatn = new Font(familyLatn, sz / 1.3F, FontStyle.Regular);
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            using (Brush b = new SolidBrush(Color.White))
            {
                g.FillRectangle(b, 0, 0, Width, Height);
            }
        }

        private void drawHanzi(Graphics g, string txt, FontTray ft, PointF pt, Brush b, StringFormat sf)
        {
            float x = pt.X;
            float y = pt.Y + ft.VertOfs;
            for (int i = 0; i != txt.Length; ++i)
            {
                string chr = txt.Substring(i, 1);
                g.DrawString(chr, ft.Font, b, new PointF(x + ft.HorizOfs, y), sf);
                x += ft.DisplayWidth;
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (fntSimpA == null) return;
            Graphics g = pe.Graphics;

            int pad = (int)(xpad * scale);
            Rectangle rbounded = new Rectangle(pad, pad, Width - 2 * pad, Height - 2 * pad);
            using (Pen p = new Pen(Color.Black))
            {
                p.DashStyle = DashStyle.Dot;
                g.SmoothingMode = SmoothingMode.None;
                g.DrawRectangle(p, rbounded);
            }
            g.TranslateTransform(rbounded.Left, rbounded.Top);

            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            using (Brush b = new SolidBrush(Color.DimGray))
            {
                float x = 0;
                drawHanzi(g, txtSimp, fntSimpA, new PointF(x, 0), b, sf);
                x += ((float)txtSimp.Length) * fntSimpA.DisplayWidth;
                g.DrawString(txtLatn, fntLatn, b, new PointF(x, 0), sf);
                x += g.MeasureString(txtLatn, fntLatn, 65535, sf).Width;
                drawHanzi(g, txtTrad, fntTradA, new PointF(x, 0), b, sf);
                x += ((float)txtTrad.Length) * fntTradA.DisplayWidth;
                g.DrawString(txtLatn, fntLatn, b, new PointF(x, 0), sf);
                x += g.MeasureString(txtLatn, fntLatn, 65535, sf).Width;

                drawHanzi(g, txtSimp, fntSimpN, new PointF(x, 0), b, sf);
                x += ((float)txtSimp.Length) * fntSimpN.DisplayWidth;
                g.DrawString(txtLatn, fntLatn, b, new PointF(x, 0), sf);
                x += g.MeasureString(txtLatn, fntLatn, 65535, sf).Width;
                drawHanzi(g, txtTrad, fntTradN, new PointF(x, 0), b, sf);
            }

            //SizeF szLatn = g.MeasureString(txtLatn, fnt, 65535, sf);
            //using (Pen p = new Pen(Color.Red))
            //{
            //    g.DrawRectangle(p, sz.Width, yOfs, szLatn.Width, szLatn.Height);

            //}
            //using (Brush b = new SolidBrush(Color.DarkGray))
            //{
            //    g.DrawString(txtLatn, fnt, b, new PointF(sz.Width, yOfs), sf);
            //}
        }
    }
}
