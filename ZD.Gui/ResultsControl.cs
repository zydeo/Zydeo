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

using ZD.Common;
using ZD.Gui.Zen;

namespace ZD.Gui
{
    public class ResultsControl : ZenControl
    {
        /// <summary>
        /// Delegate for handling lookup requests through clicking on target link.
        /// </summary>
        /// <param name="queryString"></param>
        public delegate void LookupThroughLinkDelegate(string queryString);

        /// <summary>
        /// UI text provider.
        /// </summary>
        private readonly ITextProvider tprov;

        /// <summary>
        /// Called when user clicks a link (Chinese text) in an entry target;
        /// </summary>
        private LookupThroughLinkDelegate lookupThroughLink;

        /// <summary>
        /// Scroll bar.
        /// </summary>
        private ZenScrollBar sbar;

        /// <summary>
        /// One result control for each result I'm showin.
        /// </summary>
        private List<OneResultControl> resCtrls = new List<OneResultControl>();

        /// <summary>
        /// Index of the first results control that is at least partially visible.
        /// </summary>
        private int firstVisibleIdx = -1;

        /// <summary>
        /// Text shown in bottom right: N results found. Or empty, if no search yet.
        /// </summary>
        private string txtResCount = string.Empty;

        /// <summary>
        /// Ctor.
        /// </summary>
        public ResultsControl(ZenControl owner, ITextProvider tprov, LookupThroughLinkDelegate lookupThroughLink)
            : base(owner)
        {
            this.tprov = tprov;
            this.lookupThroughLink = lookupThroughLink;

            sbar = new ZenScrollBar(this, onTimerForScrollBar);
            RemoveChild(sbar); // Instead of setting visible to false
        }

        /// <summary>
        /// Handles scroll bar's request for a timer. It's bossy, this inert scrollbar.
        /// </summary>
        private void onTimerForScrollBar()
        {
            SubscribeToTimer();
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
        public override void DoTimer(out bool? needBackground, out RenderMode? renderMode)
        {
            bool needTimer, valueChanged;
            sbar.DoScrollTimer(out needTimer, out valueChanged);
            if (!needTimer) UnsubscribeFromTimer();
            if (valueChanged)
            {
                int y = 1 - sbar.Position;
                foreach (OneResultControl orc in resCtrls)
                {
                    orc.RelTop = y;
                    y += orc.Height;
                }
                updateFirstVisibleIdx();
                needBackground = false;
                renderMode = RenderMode.Invalidate;
            }
            else
            {
                needBackground = null;
                renderMode = null;
            }
        }

        public override void Dispose()
        {
            foreach (OneResultControl orc in resCtrls) orc.Dispose();
            resCtrls = null;
            firstVisibleIdx = -1;
            base.Dispose();
        }

        /// <summary>
        /// Gets width and height of content area (occupied by individual results controls)
        /// </summary>
        private void getContentSize(out int cw, out int ch)
        {
            ch = Height - 2; // Top and bottom border
            cw = Width - 2; // Left and right borders
            if (sbar.Parent == this) cw -= sbar.Width; // Scrollbar, if visible
        }

        /// <summary>
        /// <para>Effectively shows or hides scrollbar.</para>
        /// <para>To hide, removes from children.</para>
        /// <para>To show, adds to children and updates position.</para>
        /// </summary>
        private void setScrollbarVisibility(bool visible)
        {
            if (visible)
            {
                if (sbar.Parent != this) AddChild(sbar);
                sbar.Height = Height - 2;
                sbar.RelTop = 1;
                sbar.RelLeft = Width - 1 - sbar.Width;
            }
            else
            {
                if (sbar.Parent == this) RemoveChild(sbar);
            }
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
            bool sbarVisible = sbar.Parent == this;
            if (sbarVisible && resCtrls[resCtrls.Count - 1].RelBottom < Height - 1 ||
                !sbarVisible && resCtrls[resCtrls.Count - 1].RelBottom >= Height - 1)
            {
                sbarVisible = !sbarVisible;
                setScrollbarVisibility(sbarVisible);
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
                        orc.Analyze(g, cw);
                        orc.RelLocation = new Point(1, y + 1);
                        y += orc.Height;
                        odd = !odd;
                    }
                }
                // Probably not needed, but doesn't hurt
                updateFirstVisibleIdx();
            }
            // If scroll bar is now visible, set its large value and position
            if (sbarVisible)
            {
                //suppressScrollChanged = true;
                sbar.Maximum = resCtrls[resCtrls.Count - 1].RelBottom - resCtrls[0].RelTop;
                sbar.PageSize = ch;
                int smallChange = (int)(Magic.ZhoResultFontSize * Scale);
                if (smallChange > ch) smallChange = ch;
                sbar.SmallChange = smallChange;
                sbar.Position = 1 - resCtrls[0].RelTop;
                //suppressScrollChanged = false;
                // Probably not needed, but doesn't hurt
                updateFirstVisibleIdx();
            }
            return cw;
        }

        private void reAnalyzeResultsDisplay()
        {
            // Content rectangle height and width
            int cw, ch;
            getContentSize(out cw, out ch);

            // Put scroll bar in its place, update its large change (which is always one full screen)
            // Call below will not change visibility, but update sbar position.
            setScrollbarVisibility(sbar.Parent == this);

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
                if (orc.RelBottom > pivotY)
                {
                    pivotY = orc.RelBottom;
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
                    orc.Analyze(g, cw);
                }
            }
            // Move pivot control back in place so bottom stays where it was
            // But: if pivot was first shown control at top, keep top in place
            int diff = pivotY - pivotCtrl.RelBottom;
            if (pivotIX == 0 && pivotCtrl.RelTop == 1) diff = 0;
            pivotCtrl.RelTop += diff;
            // Lay out remaining controls up and down
            for (int i = pivotIX + 1; i < resCtrls.Count; ++i)
                resCtrls[i].RelTop = resCtrls[i - 1].RelBottom;
            for (int i = pivotIX - 1; i >= 0; --i)
                resCtrls[i].RelTop = resCtrls[i + 1].RelTop - resCtrls[i].Height;
            // Edge case: very first control's top must not be greater than 1
            if (resCtrls[0].RelTop > 1)
            {
                diff = resCtrls[0].RelTop - 1;
                foreach (OneResultControl orc in resCtrls) orc.RelTop -= diff;
            }
            // If there is space below very last control's bottom, but first control is above window edge
            // > Move down, but without detaching creating empty space at top
            int emptyAtBottom = Height - 1 - resCtrls[resCtrls.Count - 1].RelBottom;
            if (emptyAtBottom > 0)
            {
                int outsideAtTop = 1 - resCtrls[0].RelTop;
                diff = Math.Min(outsideAtTop, emptyAtBottom);
                if (diff > 0)
                {
                    foreach (OneResultControl orc in resCtrls)
                        orc.RelTop += diff;
                }
            }
            // Change our mind about scrollbar control?
            cw = showOrHideScrollbar();
            // Update first visible control's index
            updateFirstVisibleIdx();
        }

        /// <summary>
        /// Updates the cached index of the first visible control.
        /// </summary>
        private void updateFirstVisibleIdx()
        {
            // This should never happen, just being defensive.
            if (resCtrls == null || resCtrls.Count == 0)
            {
                firstVisibleIdx = -1;
                return;
            }
            // OK, check if what we used to call the first visible control is now scrolled out at top
            // Then: search down
            // Or, if it's been scolled down, search up
            // No previous first visible: start from 0
            if (firstVisibleIdx == -1) firstVisibleIdx = 0;
            if (resCtrls[firstVisibleIdx].RelBottom <= 1)
            {
                if (firstVisibleIdx < resCtrls.Count) ++firstVisibleIdx;
                while (resCtrls[firstVisibleIdx].RelBottom <= 1 && firstVisibleIdx < resCtrls.Count)
                    ++firstVisibleIdx;
            }
            else if (resCtrls[firstVisibleIdx].RelTop > 1)
            {
                if (firstVisibleIdx > 0) --firstVisibleIdx;
                while (resCtrls[firstVisibleIdx].RelTop > 1 && firstVisibleIdx > 0)
                    --firstVisibleIdx;
            }
        }

        protected override void OnSizeChanged()
        {
            reAnalyzeResultsDisplay();
            // No need to invalidate here. Form will redraw evertyhing from top down.
            // Done.
        }

        /// <summary>
        /// Displays the received results, discarding existing data.
        /// </summary>
        /// <param name="entryProvider">The entry provider; ownership passed by caller to me.</param>
        /// <param name="results">Cedict lookup results to show.</param>
        /// <param name="script">Defines which script(s) to show.</param>
        public void SetResults(ICedictEntryProvider entryProvider,
            ReadOnlyCollection<CedictResult> results,
            SearchScript script)
        {
            try
            {
                doSetResults(entryProvider, results, script);
            }
            finally
            {
                entryProvider.Dispose();
            }
        }

        /// <summary>
        /// See <see cref="SetResults"/>.
        /// </summary>
        private void doSetResults(ICedictEntryProvider entryProvider,
            ReadOnlyCollection<CedictResult> results,
            SearchScript script)
        {
            // Decide if we first try with scrollbar visible or not
            // This is a very rough heuristics (10 results or more), but doesn't matter
            // Recalc costs much if there are many results, and the number covers that safely
            bool sbarVisible = results.Count > 10;
            setScrollbarVisibility(sbarVisible);

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
            firstVisibleIdx = -1;

            // No results
            if (results.Count == 0)
            {
                txtResCount = tprov.GetString("ResultsCountNone");
                setScrollbarVisibility(false);
                MakeMePaint(false, RenderMode.Invalidate);
                return;
            }

            // Create new result controls
            int y = 0;
            using (Bitmap bmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                bool odd = true;
                foreach (CedictResult cr in results)
                {
                    OneResultControl orc = new OneResultControl(this, tprov, lookupFromCtrl, entryProvider, cr, script, odd);
                    orc.Analyze(g, cw);
                    orc.RelLocation = new Point(1, y + 1);
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

            // Update first visible control's index
            updateFirstVisibleIdx();

            // Render
            MakeMePaint(false, RenderMode.Invalidate);
        }

        private void lookupFromCtrl(string queryString)
        {
            lookupThroughLink(queryString);
        }

        private void doPaintBottomOverlay(Graphics g)
        {
            // If results count text is empty, nothing to draw.
            // (This will change as overlay receives zoom/settings functionality)
            if (txtResCount == string.Empty) return;

            // First, measure size of "N results" text
            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            using (Font fnt = new Font(ZenParams.GenericFontFamily, Magic.ResultsCountFontSize))
            {
                // Measure label size: this is solid BG
                SizeF txtSizeF = g.MeasureString(txtResCount, fnt);
                int olHeight = (int)(txtSizeF.Height * 1.5F);
                int lblPad = (int)(txtSizeF.Height * 0.25F);
                int olRight = Width - 1;
                if (sbar.Parent == this) olRight -= sbar.Width;
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

        /// <summary>
        /// Paints control.
        /// </summary>
        /// <remarks>
        /// This paint function is non-standard in that it does not invoke base.DoPaint.
        /// Base paint would take care of children, BUT: we don't want that: we only paint visible results controls.
        /// In exchange, we must manually paint or one real child, the scroll bar.
        /// </remarks>
        public override void DoPaint(Graphics g)
        {
            // Content rectangle height and width
            int cw, ch;
            getContentSize(out cw, out ch);

            // Background
            using (Brush b = new SolidBrush(ZenParams.WindowColor))
            {
                g.FillRectangle(b, new Rectangle(0, 0, Width, Height));
            }
            // Border
            using (Pen p = new Pen(ZenParams.BorderColor))
            {
                g.DrawLine(p, 0, 0, Width, 0);
                g.DrawLine(p, Width - 1, 0, Width - 1, Height);
                g.DrawLine(p, Width - 1, Height - 1, 0, Height - 1);
                g.DrawLine(p, 0, Height - 1, 0, 0);
            }
            // Results
            g.ResetTransform();
            g.TranslateTransform(AbsLeft, AbsTop);
            g.Clip = new Region(new Rectangle(1, 1, cw, ch));
            if (firstVisibleIdx != -1)
            {
                int ix = firstVisibleIdx;
                while (ix < resCtrls.Count)
                {
                    OneResultControl orc = resCtrls[ix];
                    if (orc.RelTop >= ch + 1) break;
                    g.ResetTransform();
                    g.TranslateTransform(orc.AbsLeft, orc.AbsTop);
                    orc.DoPaint(g);
                    ++ix;
                }
            }
            // Bottom overlay (results count, zoom, settings)
            g.ResetTransform();
            g.TranslateTransform(AbsLeft, AbsTop);
            g.Clip = new Region(new Rectangle(1, 1, cw, ch));
            doPaintBottomOverlay(g);
            // Scroll bar, if visible
            if (sbar.Parent == this)
            {
                g.ResetTransform();
                g.TranslateTransform(sbar.AbsLeft, sbar.AbsTop);
                g.Clip = new Region(new Rectangle(0, 0, sbar.Width, sbar.Height));
                sbar.DoPaint(g);
            }
        }

    }
}
