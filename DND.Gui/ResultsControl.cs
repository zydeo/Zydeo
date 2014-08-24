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
        /// <summary>
        /// UI text provider.
        /// </summary>
        private readonly ITextProvider tprov;

        // TEMP standard scroll bar
        private VScrollBar sb;

        /// <summary>
        /// One result control for each result I'm showin.
        /// </summary>
        private List<OneResultControl> resCtrls = new List<OneResultControl>();

        /// <summary>
        /// Text shown in bottom right: N results found. Or empty, if no search yet.
        /// </summary>
        private string txtResCount = string.Empty;

        /// <summary>
        /// Currently shown scripts (simp/trad/both).
        /// </summary>
        private SearchScript currScript;

        /// <summary>
        /// <para>Suppresses scroll changed event handler.</para>
        /// <para>Needed to avoid recursion when we set to scroll thumb upon re-layout on resize.</para>
        /// </summary>
        private bool suppressScrollChanged = false;

        /// <summary>
        /// Ctor.
        /// </summary>
        public ResultsControl(ZenControl owner, ITextProvider tprov)
            : base(owner)
        {
            this.tprov = tprov;
            Application.AddMessageFilter(this);

            sb = new VScrollBar();
            RegisterWinFormsControl(sb);
            sb.Height = Height - 2;
            sb.Top = 1;
            sb.Left = Width - 1 - sb.Width;
            sb.ValueChanged += sb_ValueChanged;
            sb.Visible = false;

            SubscribeToTimer();
        }

        /// <summary>
        /// Called when scroll thumb moves: repositions results list and trigger paint.
        /// </summary>
        void sb_ValueChanged(object sender, EventArgs e)
        {
            if (suppressScrollChanged) return;

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

        /// <summary>
        /// Pre-filters Windows messages to fish out mouse wheel events.
        /// </summary>
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

        /// <summary>
        /// Handles the mouse wheel event: adds momentum to animated scrolling.
        /// </summary>
        private void onMouseWheel(MouseEventArgs e)
        {
            // Don't handle mouse wheel even if we have no scroll bar.
            if (!sb.Visible || !sb.Enabled) return;

            // Input from the mouse wheel adds momentum to animated scrolling.
            float extraMomentum = -((float)e.Delta) * ((float)sb.LargeChange) / 1500.0F;
            lock (scrollTimerLO)
            {
                scrollSpeed += extraMomentum;
            }
        }

        /// <summary>
        /// Current speed of animated scrolling. Lock with <see cref="scrollTimerLO"/>.
        /// </summary>
        private float scrollSpeed;
        /// <summary>
        /// Lock object for accessing <see cref="scrollSpeed"/>.
        /// </summary>
        private object scrollTimerLO = new object();

        /// <summary>
        /// Executes animations (e.g., scrolling with momentum)
        /// </summary>
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
                // Height of content: my height minus 2 for top and bottom border
                int ch = Height - 2;
                int scrollVal = sb.Value;
                scrollVal += (int)speed;
                if (scrollVal < 0)
                {
                    scrollVal = 0;
                }
                else if (scrollVal > sb.Maximum - ch)
                {
                    scrollVal = sb.Maximum - ch;
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

        /// <summary>
        /// Gets width and height of content area (occupied by individual results controls)
        /// </summary>
        private void getContentSize(out int cw, out int ch)
        {
            ch = Height - 2; // Top and bottom border
            cw = Width - 2; // Left and right borders
            if (sb.Visible) cw -= sb.Width; // Scrollbar, if visible
        }

        /// <summary>
        /// <para>Shows or hides scroll bar as needed; sets maximum and large value.</para>
        /// <para>Changes content area width if scrollbar appears or disappears.</para>
        /// </summary>
        /// <returns>Content area width afterwards.</returns>
        private int showOrHideScrollbar()
        {
            int cw, ch;
            getContentSize(out cw, out ch);
            if (sb.Visible && resCtrls[resCtrls.Count - 1].AbsBottom < Height - 1 ||
                !sb.Visible && resCtrls[resCtrls.Count - 1].AbsBottom >= Height - 1)
            {
                sb.Visible = !sb.Visible;
                sb.Enabled = sb.Visible;
                // Get content size again - scrollbar visibility affects it
                getContentSize(out cw, out ch);
                // Reposition results controls, laying them out from top
                int y = 0;
                using (Bitmap bmp = new Bitmap(1, 1))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    bool odd = true;
                    foreach (OneResultControl orc in resCtrls)
                    {
                        orc.Analyze(g, cw, currScript);
                        orc.AbsLocation = new Point(1, y + 1);
                        y += orc.Height;
                        odd = !odd;
                    }
                }
            }
            // If scroll bar is now visible, set its large value and position
            if (sb.Visible)
            {
                suppressScrollChanged = true;
                sb.Maximum = resCtrls[resCtrls.Count - 1].AbsBottom;
                sb.LargeChange = ch;
                sb.Value = 1 - resCtrls[0].AbsTop;
                suppressScrollChanged = false;
            }
            return cw;
        }

        private void reAnalyzeResultsDisplay()
        {
            // Content rectangle height and width
            int cw, ch;
            getContentSize(out cw, out ch);

            // Put Windows scroll bar in its place, update its large change (which is always one full screen)
            sb.Height = Height - 2;
            sb.Top = AbsTop + 1;
            sb.Left = AbsLeft + Width - 1 - sb.Width;

            // No results being shown: done here
            if (resCtrls.Count == 0) return;

            // Pivot control: first one that's at least partially visible
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
            using (Bitmap bmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                foreach (OneResultControl orc in resCtrls)
                {
                    orc.Analyze(g, cw, currScript);
                }
            }
            // Move pivot control back in place so bottom stays where it was
            // But: if pivot was first shown control at top, keep top in place
            int diff = pivotY - pivotCtrl.AbsBottom;
            if (pivotIX == 0 && pivotCtrl.AbsTop == 1) diff = 0;
            pivotCtrl.AbsTop += diff;
            // Lay out remaining controls up and down
            for (int i = pivotIX + 1; i < resCtrls.Count; ++i)
                resCtrls[i].AbsTop = resCtrls[i - 1].AbsBottom;
            for (int i = pivotIX - 1; i >= 0; --i)
                resCtrls[i].AbsTop = resCtrls[i + 1].AbsTop - resCtrls[i].Height;
            // Edge case: very first control's top must not be greater than 1
            if (resCtrls[0].AbsTop > 1)
            {
                diff = resCtrls[0].AbsTop - 1;
                foreach (OneResultControl orc in resCtrls) orc.AbsTop -= diff;
            }
            // If there is space below very last control's bottom, but first control is above window edge
            // > Move down, but without detaching creating empty space at top
            int emptyAtBottom = Height - 1 - resCtrls[resCtrls.Count - 1].AbsBottom;
            if (emptyAtBottom > 0)
            {
                int outsideAtTop = 1 - resCtrls[0].AbsTop;
                diff = Math.Min(outsideAtTop, emptyAtBottom);
                if (diff > 0)
                {
                    foreach (OneResultControl orc in resCtrls)
                        orc.AbsTop += diff;
                }
            }
            // Change our mind about scrollbar control?
            cw = showOrHideScrollbar();
        }

        protected override void OnSizeChanged()
        {
            reAnalyzeResultsDisplay();
            // No need to invalidate here. Form will redraw evertyhing from top down.
            // Done.
        }

        /// <summary>
        /// Changes the display script, keeping existing results on screen.
        /// </summary>
        /// <param name="script">The new script(s) to show.</param>
        public void ChangeScript(SearchScript script)
        {
            currScript = script;
            reAnalyzeResultsDisplay();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Displays the received results, discarding existing data.
        /// </summary>
        /// <param name="results">Cedict lookup results to show.</param>
        /// <param name="script">Defines which script(s) to show.</param>
        public void SetResults(ReadOnlyCollection<CedictResult> results, SearchScript script)
        {
            // Decide if we first try with scrollbar visible or not
            // This is a very rough heuristics (10 results or more), but doesn't matter
            // Recalc costs much if there are many results, and the number covers that safely
            sb.Visible = results.Count > 10;
            sb.Enabled = results.Count > 10;

            // Content rectangle height and width
            int cw, ch;
            getContentSize(out cw, out ch);
            
            // Dispose old results controls
            foreach (OneResultControl orc in resCtrls)
            {
                RemoveChild(orc);
                orc.Dispose();
            }
            resCtrls.Clear();
            currScript = script;

            // No results
            if (results.Count == 0)
            {
                txtResCount = tprov.GetString("ResultsCountNone");
                sb.Visible = false;
                sb.Enabled = false;
                MakeMePaint(false, RenderMode.Invalidate);
                return;
            }

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
                    orc.Analyze(g, cw, script);
                    orc.AbsLocation = new Point(1, y + 1);
                    y += orc.Height;
                    resCtrls.Add(orc);
                    odd = !odd;
                }
            }
            // Change our mind about scrollbar?
            cw = showOrHideScrollbar();

            // Results count text
            if (resCtrls.Count == 1) txtResCount = tprov.GetString("ResultsCountOne");
            else
            {
                txtResCount = tprov.GetString("ResultsCountN");
                txtResCount = string.Format(txtResCount, resCtrls.Count);
            }

            // Render
            MakeMePaint(false, RenderMode.Invalidate);
        }

        private void doPaintBottomOverlay(Graphics g)
        {
            // If results count text is empty, nothing to draw.
            // (This will change as overlay receives zoom/settings functionality)
            if (txtResCount == string.Empty) return;

            // First, measure size of "N results" text
            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            using (Font fnt = new Font(ZenParams.GenericFontFamily, ZenParams.ResultsCountFontSize))
            {
                // Measure label size: this is solid BG
                SizeF txtSizeF = g.MeasureString(txtResCount, fnt);
                int olHeight = (int)(txtSizeF.Height * 1.5F);
                int lblPad = (int)(txtSizeF.Height * 0.25F);
                int olRight = Width - 1;
                if (sb.Visible) olRight -= sb.Width;
                int lblWidth = ((int)txtSizeF.Width) + lblPad;

                // If we smoothing is on, gradient and normal opaque rectangles will never match up.
                g.SmoothingMode = SmoothingMode.None;

                // Gradient fade out on left
                int gradWidth = olHeight;
                Color colR = Color.FromArgb(196, 0, 0, 0);
                Color colL = Color.FromArgb(0, 0, 0, 0);
                Rectangle grRect = new Rectangle(olRight - lblWidth - gradWidth, Height - olHeight, gradWidth, olHeight);
                using (LinearGradientBrush gb = new LinearGradientBrush(grRect, colL, colR, LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(gb, grRect);
                }

                // Draw solid BG for label
                //using (Brush b = new SolidBrush(Color.FromArgb(255, 59, 59, 59)))
                using (Brush b = new SolidBrush(Color.FromArgb(196, 0, 0, 0)))
                {
                    g.FillRectangle(b, olRight - lblWidth, Height - olHeight, lblWidth, olHeight);
                }
                // Write text
                using (Brush b = new SolidBrush(Color.FromArgb(240, 240, 240)))
                {
                    g.DrawString(txtResCount, fnt, b, olRight - lblWidth, Height - olHeight + lblPad);
                }
            }
        }

        public override void DoPaint(Graphics g)
        {
            // Content rectangle height and width
            int cw, ch;
            getContentSize(out cw, out ch);

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
            g.Clip = new Region(new Rectangle(1, 1, cw, ch));
            foreach (OneResultControl orc in resCtrls)
            {
                if ((orc.AbsBottom < ch && orc.AbsBottom >= 0) ||
                    (orc.AbsTop < ch && orc.AbsTop >= 0))
                {
                    orc.DoPaint(g);
                }
            }
            // Bottom overlay (results count, zoom, settings)
            doPaintBottomOverlay(g);
        }

    }
}
