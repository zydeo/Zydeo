using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using DND.Common;

namespace DND.Controls
{
    public partial class ResultsControl : Control, IMessageFilter
    {
        // TEMP
        private VScrollBar sb;

        private bool handleCreated = false;
        private ReadOnlyCollection<CedictResult> results;
        private int pageSize;

        public ResultsControl()
        {
            Application.AddMessageFilter(this);

            this.DoubleBuffered = false;
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            sb = new VScrollBar();
            Controls.Add(sb);
            sb.Height = Height - 2;
            sb.Top = 1;
            sb.Left = Width - 1 - sb.Width;
            sb.Enabled = false;
            sb.Scroll += sb_Scroll;
            sb.ValueChanged += sb_ValueChanged;

            contentRectSize = new Size(Width - 2 - sb.Width, Height - 2);

            renderThread = new Thread(renderThreadFun);
            renderThread.Start();

            HandleCreated += onHandleCreated;
            HandleDestroyed += onHandleDestroyed;
        }

        void sb_ValueChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        void sb_Scroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }

        public static ushort HIWORD(IntPtr l) { return (ushort)((l.ToInt64() >> 16) & 0xFFFF); }
        public static ushort LOWORD(IntPtr l) { return (ushort)((l.ToInt64()) & 0xFFFF); }

        public bool PreFilterMessage(ref Message m)
        {
            bool r = false;
            if (m.Msg == 0x020A) //WM_MOUSEWHEEL
            {
                Point p = new Point((int)m.LParam);
                int delta = (Int16)HIWORD(m.WParam);
                MouseEventArgs e = new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, delta);
                m.Result = IntPtr.Zero; //don't pass it to the parent window
                onMouseWheel(e);
            }
            return r;
        }

        private void onMouseWheel(MouseEventArgs e)
        {
            float diff = ((float)sb.SmallChange) * ((float)e.Delta) / 120.0F;
            int idiff = -(int)diff;
            int newval = sb.Value + idiff;
            if (newval < 0) newval = 0;
            else if (newval > sb.Maximum - sb.LargeChange) newval = sb.Maximum - sb.LargeChange;
            sb.Value = newval;
        }

        private void onHandleDestroyed(object sender, EventArgs e)
        {
            renderKill = true;
            renderEvent.Set();
        }

        void onHandleCreated(object sender, EventArgs e)
        {
            handleCreated = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ...
            }
            base.Dispose(disposing);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            sb.Height = Height - 2;
            sb.Top = 1;
            sb.Left = Width - 1 - sb.Width;
            contentRectSize = new Size(Width - 2 - sb.Width, Height - 2);
            if (results != null)
                renderInBackground();
        }

        public void SetResults(ReadOnlyCollection<CedictResult> results, int pageSize)
        {
            this.results = results;
            this.pageSize = pageSize;
            renderInBackground();
        }

        private Thread renderThread;
        private AutoResetEvent renderEvent = new AutoResetEvent(false);
        private bool renderStop = false;
        private bool renderKill = false;
        private bool isRendering = false;
        private int renderRequests = 0;
        private Size contentRectSize;
        private Bitmap contentBmp;
        private List<CedictResult> resultsToRender;
        private int pageSizeToRender;

        private void renderInBackground()
        {
            lock (renderThread)
            {
                sb.Enabled = false;
                ++renderRequests;
                renderStop = true;
                if (results != null)
                    resultsToRender = new List<CedictResult>(results);
                else
                    resultsToRender = new List<CedictResult>();
                pageSizeToRender = pageSize;
                renderEvent.Set();
            }
        }

        private void renderThreadFun()
        {
            while (true)
            {
                renderEvent.WaitOne();
                if (renderKill) break;
                Size canvasSize;
                List<CedictResult> copiedResults;
                int copiedPageSize;
                lock (renderThread)
                {
                    --renderRequests;
                    if (renderRequests > 0) continue;
                    canvasSize = contentRectSize;
                    renderStop = false;
                    isRendering = true;
                    copiedResults = resultsToRender;
                    copiedPageSize = pageSizeToRender;
                }
                bool completed = doRender(canvasSize, copiedResults, copiedPageSize);
                if (renderKill) break;
                if (completed)
                {
                    lock (renderThread)
                    {
                        isRendering = false;
                    }

                    if (handleCreated)
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            sb.Maximum = contentBmp.Height;
                            sb.LargeChange = Height - 2;
                            sb.SmallChange = sb.Maximum / (copiedResults.Count + 1);
                            sb.Value = 0;
                            sb.Enabled = true;
                            Invalidate();
                        });
                    }
                }
            }
        }

        private void doRenderOne(Graphics g, int w, int h, CedictResult cr)
        {
            for (int i = 0; i != h; ++i)
            {
                using (Pen p = new Pen(Color.FromArgb(i,i,i)))
                {
                    g.DrawLine(p, 0, i, w, i);
                }
                using (Brush b = new SolidBrush(Color.White))
                using (Font f = new Font("SimSun", 12.0F))
                {
                    g.DrawString(cr.Entry.ChSimpl, f, b, 5.0F, 5.0F);
                }
            }
        }

        private bool doRender(Size canvasSize, List<CedictResult> cRes, int cPageSize)
        {
            Bitmap bmpTest = new Bitmap(1, 1);
            List<Bitmap> entryBmps = new List<Bitmap>();

            int top = 0;
            foreach (CedictResult cr in cRes)
            {
                Bitmap resBmp = new Bitmap(canvasSize.Width, 47);
                using (Graphics g = Graphics.FromImage(resBmp))
                {
                    doRenderOne(g, canvasSize.Width, resBmp.Height, cr);
                }
                entryBmps.Add(resBmp);
                top += resBmp.Height;
                if (renderStop || renderKill) break;
            }

            if (entryBmps.Count != cRes.Count)
            {
                foreach (var x in entryBmps) if (x != null) x.Dispose();
                bmpTest.Dispose();
                return false;
            }

            int h = 0;
            foreach (Bitmap b in entryBmps) h += b.Height;
            Bitmap bmp = null;
            if (h > 0)
            {
                bmp = new Bitmap(canvasSize.Width, h);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    int y = 0;
                    for (int i = 0; i != entryBmps.Count; ++i)
                    {
                        Bitmap thisBmp = entryBmps[i];
                        g.DrawImageUnscaled(thisBmp, 0, y);
                        y += thisBmp.Height;
                        thisBmp.Dispose();
                        entryBmps[i] = null;
                    }
                }
            }
            lock (renderThread)
            {
                if (contentBmp != null)
                {
                    contentBmp.Dispose();
                    contentBmp = null;
                }
                contentBmp = bmp;
            }
            bmpTest.Dispose();
            return true;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // NOP
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;
            // Background
            using (Brush b = new SolidBrush(Color.White))
            {
                g.FillRectangle(b, ClientRectangle);
            }
            // Border
            using (Pen p = new Pen(SystemColors.ControlDarkDark))
            {
                g.DrawLine(p, 0, 0, Width, 0);
                g.DrawLine(p, Width - 1, 0, Width - 1, Height);
                g.DrawLine(p, Width - 1, Height - 1, 0, Height - 1);
                g.DrawLine(p, 0, Height - 1, 0, 0);
            }
            // Show canvas
            lock (renderThread)
            {
                if (isRendering || contentBmp == null) return;
                if (contentBmp.Width != contentRectSize.Width) return;
                // Render part of bitmap
                g.DrawImage(contentBmp, new Rectangle(1, 1, contentRectSize.Width, contentRectSize.Height),
                    new Rectangle(0, sb.Value, contentRectSize.Width, contentRectSize.Height),
                    GraphicsUnit.Pixel);
            }
        }
    }
}
