using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Text;

namespace ZD.FontTest
{
    public partial class FontTestCtrl : Control
    {
        private static PrivateFontCollection fonts = new PrivateFontCollection();

        private float scale;
        private const float xpad = 20F;
        private Font fnt = new Font("Segoe UI", 8F);
        private string txt = "我英语给您";
        private const string txtLatn = "gAfy";
        private FontMetrics fm = null;

        public FontTestCtrl()
        {
            InitializeComponent();
        }

        public float DpiScale
        {
            set { scale = value; }
        }

        public override string Text
        {
            get { return txt; }
            set { txt = value; Invalidate(); }
        }

        public FontMetrics FontMetrics
        {
            get { return fm; }
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

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (fnt == null || txt == null) return;
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
            SizeF sz = g.MeasureString(txt, fnt, 65535, sf);
            float yOfs = 0;
            if (fm != null)
                yOfs = (float)(-(fm.Height - 1.0) * sz.Height * 0.85);

            using (Pen p = new Pen(Color.Red))
            {
                g.DrawRectangle(p, 0, yOfs, sz.Width, sz.Height);
            }
            using (Brush b = new SolidBrush(Color.DarkGray))
            {
                g.DrawString(txt, fnt, b, new PointF(0, yOfs), sf);
            }

            SizeF szLatn = g.MeasureString(txtLatn, fnt, 65535, sf);
            using (Pen p = new Pen(Color.Red))
            {
                g.DrawRectangle(p, sz.Width, yOfs, szLatn.Width, szLatn.Height);

            }
            using (Brush b = new SolidBrush(Color.DarkGray))
            {
                g.DrawString(txtLatn, fnt, b, new PointF(sz.Width, yOfs), sf);
            }
        }
    }
}
