using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DND.Controls
{
    public class ZenTabbedForm : Form
    {
        private readonly int headerHeight;
        private readonly int innerPadding;
        private readonly int outerShadow;
        private readonly float scale;
        private Bitmap dbuffer = null;
        private Panel contentPanel;

        public ZenTabbedForm()
        {
            SuspendLayout();

            DoubleBuffered = false;
            FormBorderStyle = FormBorderStyle.None;
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            Size = new Size(600, 240);
            contentPanel = new Panel();
            contentPanel.BackColor = ZenParams.PaddingBackColor;
            contentPanel.BorderStyle = BorderStyle.FixedSingle;
            contentPanel.Location = new Point(
                (int)(ZenParams.OuterShadow + ZenParams.InnerPadding),
                (int)(ZenParams.HeaderHeight + ZenParams.OuterShadow));
            contentPanel.Size = new Size(
                Width - 2 * (int)(ZenParams.OuterShadow + ZenParams.InnerPadding),
                Height - 2 * (int)ZenParams.OuterShadow - (int)ZenParams.InnerPadding - (int)ZenParams.HeaderHeight);
            contentPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;
            Controls.Add(contentPanel);
            AutoScaleDimensions = new SizeF(6.0F, 13.0F);
            AutoScaleMode = AutoScaleMode.Font;
            scale = CurrentAutoScaleDimensions.Height / 13.0F;
            ResumeLayout();

            headerHeight = (int)(ZenParams.HeaderHeight * scale);
            innerPadding = (int)(ZenParams.InnerPadding * scale);
            outerShadow = (int)(ZenParams.OuterShadow * scale);

            contentPanel.Location = new Point(
                outerShadow + innerPadding,
                headerHeight + outerShadow);
            contentPanel.Size = new Size(
                Width - 2 * (outerShadow + innerPadding),
                Height - 2 * outerShadow - innerPadding - headerHeight);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (dbuffer != null) { dbuffer.Dispose(); dbuffer = null; }
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (dbuffer != null)
            {
                dbuffer.Dispose();
                dbuffer = null;
            }
            base.OnSizeChanged(e);
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics gg = e.Graphics;
            // Draw border outside: cannot double-buffer that
            // (Must draw directly onto graphics, which shows whatever is on screen behind)
            for (int i = 0; i != outerShadow; ++i)
            {
                float alpha = ZenParams.ShadowAlphaStart;
                float alphaStep = alpha / (float)outerShadow;
                alpha = alpha - (float)i * alphaStep;
                using (Pen p = new Pen(Color.FromArgb((int)alpha, Color.Black)))
                {
                    p.Width = 1;
                    // North
                    gg.DrawLine(p, outerShadow, outerShadow - i - 1, Width - outerShadow - 1, outerShadow - i - 1);
                    // South
                    gg.DrawLine(p, outerShadow, Height - outerShadow - 1 + i, Width - outerShadow - 1, Height - outerShadow - 1 + i);
                    // East
                    gg.DrawLine(p, outerShadow - 1 - i, outerShadow, outerShadow - 1 - i, Height - outerShadow - 1);
                    // West
                    gg.DrawLine(p, Width - outerShadow - 1 + i, outerShadow, Width - outerShadow - 1 + i, Height - outerShadow - 1);
                }
            }
            // Do all the remaining drawing through my own hand-made double-buffering for speed
            if (dbuffer == null)
                dbuffer = new Bitmap(Width - 2 * outerShadow, Height - 2 * outerShadow);
            using (Graphics g = Graphics.FromImage(dbuffer))
            {
                int width = Width - 2 * outerShadow;
                int height = Height - 2 * outerShadow;
                using (Brush b = new SolidBrush(ZenParams.HeaderBackColor))
                {
                    g.FillRectangle(b, 0, 0, width, headerHeight);
                }
                using (Brush b = new SolidBrush(ZenParams.PaddingBackColor))
                {
                    g.FillRectangle(b, 0, headerHeight, innerPadding, height - headerHeight);
                    g.FillRectangle(b, width - innerPadding, headerHeight, innerPadding, height - headerHeight);
                    g.FillRectangle(b, innerPadding, height - innerPadding, width - 2 * innerPadding, innerPadding);
                }
            }
            e.Graphics.DrawImageUnscaled(dbuffer, outerShadow, outerShadow);
        }
    }
}
