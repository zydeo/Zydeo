using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;

namespace DND.Controls
{
    internal class ZenWinForm : Form
    {
        public class BitmapRenderer : IDisposable
        {
            private readonly Mutex dbufferMutex;
            private readonly Bitmap dbuffer;
            private readonly Graphics graphics;

            internal BitmapRenderer(Mutex dbufferMutex, Bitmap dbuffer)
            {
                this.dbufferMutex = dbufferMutex;
                this.dbuffer = dbuffer;
                dbufferMutex.WaitOne();
                graphics = Graphics.FromImage(dbuffer);
            }

            public void Dispose()
            {
                graphics.Dispose();
                dbufferMutex.ReleaseMutex();
            }

            public Graphics Graphics
            {
                get { return graphics; }
            }
        }

        public delegate void RenderDelegate(Graphics g);

        private readonly RenderDelegate renderDelegate;
        private readonly float scale;
        private readonly Mutex dbufferMutex = new Mutex();
        private Bitmap dbuffer = null;

        public ZenWinForm(RenderDelegate renderDelegate)
        {
            this.renderDelegate = renderDelegate;

            SuspendLayout();

            DoubleBuffered = false;
            FormBorderStyle = FormBorderStyle.None;

            Size = new Size(800, 300);
            AutoScaleDimensions = new SizeF(6.0F, 13.0F);
            AutoScaleMode = AutoScaleMode.Font;
            scale = CurrentAutoScaleDimensions.Height / 13.0F;

            ResumeLayout();
        }

        internal new float Scale
        {
            get { return scale; }
        }

        internal new Point MousePosition
        {
            get { return Form.MousePosition; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            try
            {
                dbufferMutex.WaitOne();
                if (dbuffer != null) { dbuffer.Dispose(); dbuffer = null; }
            }
            finally { dbufferMutex.ReleaseMutex(); }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            try
            {
                dbufferMutex.WaitOne();
                if (dbuffer != null)
                {
                    dbuffer.Dispose();
                    dbuffer = null;
                }
            }
            finally { dbufferMutex.ReleaseMutex(); }
            base.OnSizeChanged(e);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        public BitmapRenderer GetBitmapRenderer()
        {
            if (dbuffer == null) return null;
            return new BitmapRenderer(dbufferMutex, dbuffer);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // NOP!
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                dbufferMutex.WaitOne();
                // Do all the remaining drawing through my own hand-made double-buffering for speed
                if (dbuffer == null)
                {
                    dbuffer = new Bitmap(Width, Height);
                    using (Graphics g = Graphics.FromImage(dbuffer))
                    {
                        renderDelegate(g);
                    }
                }
                e.Graphics.DrawImageUnscaled(dbuffer, 0, 0);
            }
            finally { dbufferMutex.ReleaseMutex(); }
        }

    }
}
