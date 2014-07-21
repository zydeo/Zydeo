using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using DND.Common;
using DND.Gui.Zen;

namespace DND.Gui
{
    public class ResultsControl : ZenControl, IMessageFilter
    {
        // TEMP standard scroll bar
        private VScrollBar sb;
        private Size contentRectSize;
        private List<OneResultControl> resCtrls = new List<OneResultControl>();
        private SearchScript currScript;

        public ResultsControl(ZenControl owner)
            : base(owner)
        {
            Application.AddMessageFilter(this);

            sb = new VScrollBar();
            RegisterWinFormsControl(sb);
            sb.Height = Height - 2;
            sb.Top = 1;
            sb.Left = Width - 1 - sb.Width;
            sb.Enabled = false;
            sb.ValueChanged += sb_ValueChanged;

            contentRectSize = new Size(Width - 2 - sb.Width, Height - 2);

            SubscribeToTimer();
        }

        void sb_ValueChanged(object sender, EventArgs e)
        {
            int y = 1 - sb.Value;
            foreach (OneResultControl orc in resCtrls)
            {
                orc.AbsTop = y;
                y += orc.Height;
            }
            MakeMePaint(false, RenderMode.Invalidate);
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
            if (!sb.Enabled) return;
            addMomentum(-((float)e.Delta) * ((float)sb.LargeChange) / 1500.0F);
        }

        private float scrollSpeed;
        private object scrollTimerLO = new object();

        private void addMomentum(float diffMomentum)
        {
            lock (scrollTimerLO)
            {
                scrollSpeed += diffMomentum;
            }
        }

        public override void DoTimer()
        {
            float speed = 0;
            lock (scrollTimerLO)
            {
                speed = scrollSpeed;
                scrollSpeed *= 0.9F;
                if (scrollSpeed > 3.0F) scrollSpeed -= 3.0F;
                else if (scrollSpeed < -3.0F) scrollSpeed += 3.0F;
                if (Math.Abs(scrollSpeed) < 1.0F) scrollSpeed = 0;
            }
            if (speed == 0) return;
            InvokeOnForm((MethodInvoker)delegate
            {
                int scrollVal = sb.Value;
                scrollVal += (int)speed;
                if (scrollVal < 0)
                {
                    scrollVal = 0;
                }
                else if (scrollVal > sb.Maximum - contentRectSize.Height)
                {
                    scrollVal = sb.Maximum - contentRectSize.Height;
                    if (scrollVal < 0) scrollVal = 0;
                }
                sb.Value = scrollVal;
            });
        }

        public override void Dispose()
        {
            foreach (OneResultControl orc in resCtrls) orc.Dispose();
            base.Dispose();
        }

        protected override void OnSizeChanged()
        {
            // Update content rectangle - just a cache for other calculations
            contentRectSize = new Size(Width - 2 - sb.Width, Height - 2);
            // Put Windows scroll bar in its place, update its large change (which is always one full screen)
            sb.Height = Height - 2;
            sb.Top = AbsTop + 1;
            sb.Left = AbsLeft + Width - 1 - sb.Width;
            sb.LargeChange = contentRectSize.Height;

            // No results being shown: done 'ere
            if (resCtrls.Count == 0) return;
            // Find bottom of first visible control
            int pivotY = 1;
            int pivotIX = -1;
            OneResultControl pivotCtrl = null;
            for (int i = 0; i != resCtrls.Count; ++i)
            {
                OneResultControl orc = resCtrls[i];
                // Results controls' absolute locations are within my full canvas
                // First visible one is the guy whose bottom is greater than 1
                if (orc.AbsBottom > pivotY)
                {
                    pivotY = orc.AbsBottom;
                    pivotCtrl = orc;
                    pivotIX = i;
                    break;
                }
            }
            // Recalculate each result control's layout, and height
            using (Bitmap bmp = new Bitmap(1,1))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                foreach (OneResultControl orc in resCtrls)
                {
                    orc.Analyze(g, contentRectSize.Width, currScript);
                    //orc.Width = contentRectSize.Width;
                }
            }
            // Move pivot control back in place
            int diff = pivotY - pivotCtrl.AbsBottom;
            pivotCtrl.AbsTop += diff;
            // Lay out remaining controls up and down
            for (int i = pivotIX + 1; i < resCtrls.Count; ++i)
                resCtrls[i].AbsTop = resCtrls[i - 1].AbsBottom;
            for (int i = pivotIX - 1; i >= 0; --i)
                resCtrls[i].AbsTop = resCtrls[i + 1].AbsTop - resCtrls[i].Height;
            // Very first control's top must be 1
            // TO-DO from here: edge cases
            // Reset scroll bar's max value and position
            // TO-DO from here
            // Invalidate
            MakeMePaint(false, RenderMode.Invalidate);
        }

        public void SetResults(ReadOnlyCollection<CedictResult> results, SearchScript script)
        {
            if (Scale == 0) throw new InvalidOperationException("Scale must be set before setting results to show.");
            // Dispose old results controls
            foreach (OneResultControl orc in resCtrls) orc.Dispose();
            resCtrls.Clear();
            currScript = script;
            // Find longest character count in headwords
            int maxHeadLength = 0;
            if (results.Count > 0) maxHeadLength = results.Max(r => r.Entry.ChSimpl.Length);
            // Create new result controls
            int y = 0;
            using (Bitmap bmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                bool odd = true;
                foreach (CedictResult cr in results)
                {
                    OneResultControl orc = new OneResultControl(this, cr, maxHeadLength, script, odd);
                    orc.Analyze(g, contentRectSize.Width, script);
                    orc.AbsLocation = new Point(1, y + 1);
                    y += orc.Height;
                    resCtrls.Add(orc);
                    odd = !odd;
                }
            }
            sb.Maximum = y;
            sb.LargeChange = contentRectSize.Height;
            sb.Value = 0;
            sb.Enabled = true;
            MakeMePaint(false, RenderMode.Invalidate);
        }

        public override void DoPaint(Graphics g)
        {
            // Background
            using (Brush b = new SolidBrush(Color.White))
            {
                g.FillRectangle(b, new Rectangle(0, 0, Width, Height));
            }
            // Border
            using (Pen p = new Pen(SystemColors.ControlDarkDark))
            {
                g.DrawLine(p, 0, 0, Width, 0);
                g.DrawLine(p, Width - 1, 0, Width - 1, Height);
                g.DrawLine(p, Width - 1, Height - 1, 0, Height - 1);
                g.DrawLine(p, 0, Height - 1, 0, 0);
            }
            // Results
            g.Clip = new Region(new Rectangle(1, 1, contentRectSize.Width, contentRectSize.Height));
            foreach (OneResultControl orc in resCtrls)
            {
                if ((orc.AbsBottom < contentRectSize.Height && orc.AbsBottom >= 0) ||
                    (orc.AbsTop < contentRectSize.Height && orc.AbsTop >= 0))
                {
                    orc.DoPaint(g);
                }
            }
        }

    }
}
