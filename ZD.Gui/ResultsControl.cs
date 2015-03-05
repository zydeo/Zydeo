using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
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
        /// Delegate for handling a control's request for retrieving a dictionary entry.
        /// </summary>
        public delegate CedictEntry GetEntryDelegate(int entryId);

        /// <summary>
        /// UI text provider.
        /// </summary>
        private readonly ITextProvider tprov;

        /// <summary>
        /// Called when user clicks a link (Chinese text) in an entry target;
        /// </summary>
        private LookupThroughLinkDelegate lookupThroughLink;

        /// <summary>
        /// Called when a result control requests a dictionary entry.
        /// </summary>
        private readonly GetEntryDelegate getEntry;

        /// <summary>
        /// Scroll bar.
        /// </summary>
        private ZenScrollBar sbar;

        /// <summary>
        /// One result control for each result I'm showing.
        /// </summary>
        private List<OneResultControl> resCtrls = new List<OneResultControl>();

        /// <summary>
        /// Lock object around results controls collection - to avoid conflicts in pain while recreating.
        /// </summary>
        private readonly object resCtrlsLO = new object();

        /// <summary>
        /// Lock object around <see cref="displayId"/> value.
        /// </summary>
        private readonly object displayIdLO = new object();

        /// <summary>
        /// Incremental lookup ID of results current being shown.
        /// </summary>
        private int displayId = int.MinValue;

        /// <summary>
        /// Index of the first results control that is at least partially visible.
        /// </summary>
        private int firstVisibleIdx = -1;

        /// <summary>
        /// Text shown in bottom right: N results found. Or empty, if no search yet.
        /// </summary>
        private string txtResCount = string.Empty;

        /// <summary>
        /// If true, we throw an exception from BG thread when scroll reaches bottom. to test error handling,.
        /// </summary>
        private bool crashForTest = false;

        /// <summary>
        /// Ctor.
        /// </summary>
        public ResultsControl(ZenControl owner, ITextProvider tprov,
            LookupThroughLinkDelegate lookupThroughLink, GetEntryDelegate getEntry)
            : base(owner)
        {
            this.tprov = tprov;
            this.lookupThroughLink = lookupThroughLink;
            this.getEntry = getEntry;

            sbar = new ZenScrollBar(this, onTimerForScrollBar);
            RemoveChild(sbar); // Instead of setting visible to false
        }

        /// <summary>
        /// Disposes existing result controls and clears list.
        /// </summary>
        private void doDisposeResultControls()
        {
            // Dispose old results controls
            foreach (OneResultControl orc in resCtrls)
            {
                RemoveChild(orc);
                orc.Dispose();
            }
            resCtrls.Clear();
            firstVisibleIdx = -1;
        }

        public override void Dispose()
        {
            doDisposeResultControls();
            base.Dispose();
        }

        /// <summary>
        /// Handles scroll bar's request for a timer. It's bossy, this inert scrollbar.
        /// </summary>
        private void onTimerForScrollBar()
        {
            SubscribeToTimer();
        }

        /// <summary>
        /// Gets width and height of content area (occupied by individual results controls)
        /// </summary>
        private void getContentSize(bool sbarVisible, out int cw, out int ch)
        {
            ch = Height - 2; // Top and bottom border
            cw = Width - 2; // Left and right borders
            if (sbarVisible) cw -= sbar.Width; // Scrollbar, if visible
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
            getContentSize(sbar.Parent == this, out cw, out ch);
            bool sbarVisible = sbar.Parent == this;
            if (sbarVisible && resCtrls[resCtrls.Count - 1].RelBottom < Height - 1 ||
                !sbarVisible && resCtrls[resCtrls.Count - 1].RelBottom >= Height - 1)
            {
                sbarVisible = !sbarVisible;
                setScrollbarVisibility(sbarVisible);
                // Get content size again - scrollbar visibility affects it
                getContentSize(sbar.Parent == this, out cw, out ch);
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
            getContentSize(sbar.Parent == this, out cw, out ch);

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
            // Throwing for test
            if (crashForTest)
                throw new ZD.Common.DiagnosticException(false);

            reAnalyzeResultsDisplay();
            // No need to invalidate here. Form will redraw evertyhing from top down.
            // Done.
        }

        /// <summary>
        /// <para>Fades out whatever is currently shown to indicate lookup in progress.</para>
        /// <para>Next call to set content (e.g., <see cref="SetResults"/> will remove shade.</para>
        /// </summary>
        public void FadeOut()
        {
            doFade(true);
        }

        /// <summary>
        /// Displays the received results, discarding existing data.
        /// </summary>
        /// <param name="lookupId">ID of lookup whose results are shown. If ID is smaller than last seen value, we don't show results.</param>
        /// <param name="entryProvider">The entry provider; ownership passed by caller to me.</param>
        /// <param name="results">Cedict lookup results to show.</param>
        /// <param name="script">Defines which script(s) to show.</param>
        /// <returns>True if results got shown; false if they're discarded because newer results are already on display.</returns>
        public bool SetResults(int lookupId,
            ICedictEntryProvider entryProvider,
            ReadOnlyCollection<CedictResult> results,
            SearchScript script)
        {
#if DEBUG
            // Make us crash at bottom if first result "柏林" (comes up for "bolin")
            if (results.Count > 0)
            {
                CedictEntry entry = entryProvider.GetEntry(results[0].EntryId);
                if (entry.ChSimpl == "柏林") crashForTest = true;
                else crashForTest = false;
            }
#endif
            try
            {
                return doSetResults(lookupId, entryProvider, results, script);
            }
            finally
            {
                if (entryProvider != null) entryProvider.Dispose();
            }
        }

        /// <summary>
        /// See <see cref="SetResults"/>.
        /// </summary>
        private bool doSetResults(int lookupId,
            ICedictEntryProvider entryProvider,
            ReadOnlyCollection<CedictResult> results,
            SearchScript script)
        {
            lock (displayIdLO)
            {
                // If we're already too late, don't bother changing display.
                if (displayId > lookupId) return false;
                displayId = lookupId;
                // Empty result set - special handling
                if (results.Count == 0)
                {
                    lock (resCtrlsLO)
                    {
                        doDisposeResultControls();
                        txtResCount = tprov.GetString("ResultsCountNone");
                        setScrollbarVisibility(false);
                    }
                    // Render
                    doFade(false);
                    MakeMePaint(false, RenderMode.Invalidate);
                    return true;
                }
            }

            // Decide if we first try with scrollbar visible or not
            // This is a very rough heuristics (10 results or more), but doesn't matter
            // Recalc costs much if there are many results, and the number covers that safely
            bool sbarVisible = results.Count > 10;

            // Content rectangle height and width
            int cw, ch;
            getContentSize(sbarVisible, out cw, out ch);

            // Create new result controls. At this point, not overwriting old ones!
            // This is the cycle that takes *long*.
            List<OneResultControl> newCtrls = new List<OneResultControl>(results.Count);
            int y = 0;
            using (Bitmap bmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                bool canceled = false;
                for (int rix = 0; rix != results.Count; ++rix)
                {
                    CedictResult cr = results[rix];
                    OneResultControl orc = new OneResultControl(null, Scale, tprov,
                        onLookupFromCtrl, onPaintFromCtrl, onGetEntry,
                        entryProvider, cr, script, rix == results.Count - 1);
                    orc.Analyze(g, cw);
                    // Cannot use RelLocation b/c control has no parent yet
                    orc.AbsLocation = new Point(AbsLeft + 1, AbsTop + y + 1);
                    y += orc.Height;
                    newCtrls.Add(orc);
                    // At any point, if we realize lookup ID has changed, we stop
                    // This can happen if a later, quick lookup completes and shows results before us
                    // Checking integers is atomic, no locking
                    if (displayId > lookupId) { canceled = true; break; }
                }
                if (canceled)
                {
                    foreach (OneResultControl orc in newCtrls) orc.Dispose();
                    return false;
                }
            }
            // OK, last chance to change our mind about showing results.
            // The rest is synchronized - but it's also fast
            lock (displayIdLO)
            {
                if (displayId > lookupId) return false;
                displayId = lookupId;
                // Rest must be invoked on GUI. Otherwise, as we're adding children,
                // Collections are modified that are also accessed by paint in a resize event handler etc.
                InvokeOnForm((MethodInvoker)delegate
                {
                    // Stop any scrolling that may be going on. Cannot scroll what's being replaced.
                    if (sbar.Parent == this) sbar.StopAnyScrolling();
                    // Prevent any painting from worker threads - also accesses collection we're changing
                    lock (resCtrlsLO)
                    {
                        // Get rid of old result controls, remember/own new ones
                        doDisposeResultControls();
                        resCtrls = newCtrls;
                        foreach (OneResultControl orc in resCtrls) AddChild(orc);
                        // Actually show or hide scrollbar as per original decision
                        setScrollbarVisibility(sbarVisible);
                        // Now, by the time we're here, size may have changed
                        // That is unlikely, but then we got to re-layout stuff
                        int cwNew, chNew;
                        getContentSize(sbarVisible, out cwNew, out chNew);
                        if (cwNew != cw || chNew != ch) reAnalyzeResultsDisplay();
                        else
                        {
                            // Everything as big as it used to be...
                            // Change our mind about scrollbar?
                            cw = showOrHideScrollbar();
                        }
                    }
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
                    doFade(false);
                    MakeMePaint(false, RenderMode.Invalidate);
                });
                // Done.
                return true;
            }
        }

        /// <summary>
        /// Repaints results control in one when a single result control needs UI update.
        /// </summary>
        private void onPaintFromCtrl()
        {
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Handles event sent by control when user clicks on link (Chinese in target).
        /// </summary>
        private void onLookupFromCtrl(string queryString)
        {
            lookupThroughLink(queryString);
        }

        /// <summary>
        /// Handles request from a control to retrieve a dictionary entry.
        /// </summary>
        private CedictEntry onGetEntry(int entryId)
        {
            return getEntry(entryId);
        }

        /// <summary>
        /// Lock object around animation values.
        /// </summary>
        private readonly object animLO = new object();

        /// <summary>
        /// <para>Fade degree. 0 is no fade, 1 is greatest fade.</para>
        /// <para>float.MinValue means no fade, and also no animbation in progress.</para>
        /// </summary>
        private float animFade = float.MinValue;

        /// <summary>
        /// Handles timer event for fading animation.
        /// </summary>
        private void doTimerFade(out bool needTimer, out bool paintNeeded)
        {
            needTimer = paintNeeded = false;
            lock (animLO)
            {
                if (animFade == float.MinValue) return;
                if (animFade > 1) { animFade = 1; return; }
                animFade += 0.05F;
                needTimer = paintNeeded = true;
            }
        }

        /// <summary>
        /// Gets color (with alpha) of fade overlay.
        /// </summary>
        private Color getFadeColor()
        {
            lock (animLO)
            {
                if (animFade == float.MinValue) return Color.FromArgb(0, 255, 255, 255);
                float fadeAlpha = 196F;
                float fadeVal = Math.Min(animFade, 1);
                fadeAlpha *= fadeVal;
                return Color.FromArgb((int)fadeAlpha, 255, 255, 255);
            }
        }

        /// <summary>
        /// Starts fading in, or removes fade effect.
        /// </summary>
        private void doFade(bool showShadow)
        {
            lock (animLO)
            {
                if (showShadow)
                {
                    if (animFade == float.MinValue)
                    {
                        animFade = 0;
                        SubscribeToTimer();
                    }
                }
                else animFade = float.MinValue;
            }
        }

        /// <summary>
        /// Executes animations (e.g., scrolling with momentum)
        /// </summary>
        public override void DoTimer(out bool? needBackground, out RenderMode? renderMode)
        {
            bool ntScrool, valueChangedScroll;
            bool ntFade, pnFade;
            sbar.DoScrollTimer(out ntScrool, out valueChangedScroll);
            doTimerFade(out ntFade, out pnFade);

            if (!ntScrool && !ntFade) UnsubscribeFromTimer();

            if (valueChangedScroll || pnFade)
            {
                if (valueChangedScroll)
                {
                    int y = 1 - sbar.Position;
                    foreach (OneResultControl orc in resCtrls)
                    {
                        orc.RelTop = y;
                        y += orc.Height;
                    }
                    updateFirstVisibleIdx();
                }
                needBackground = false;
                renderMode = RenderMode.Invalidate;
            }
            else
            {
                needBackground = null;
                renderMode = null;
            }

            // Throwing for test
            if (crashForTest && sbar.Position + sbar.PageSize >= sbar.Maximum)
                throw new ZD.Common.DiagnosticException(false);
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
                Color colR = Color.FromArgb(Magic.ResultCountOverlayOpacity, Magic.ResultCountOverlayBackColor);
                Color colL = Color.FromArgb(0, Magic.ResultCountOverlayBackColor);
                Rectangle grRect = new Rectangle(olRight - lblWidth - gradWidth, Height - olHeight, gradWidth, olHeight);
                using (LinearGradientBrush gb = new LinearGradientBrush(grRect, colL, colR, LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(gb, grRect);
                }

                // Draw solid BG for label
                using (Brush b = new SolidBrush(colR))
                {
                    g.FillRectangle(b, olRight - lblWidth, Height - olHeight, lblWidth, olHeight);
                }
                // Write text
                using (Brush b = new SolidBrush(Magic.ResultCountOverlayTextColor))
                {
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
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
            getContentSize(sbar.Parent == this, out cw, out ch);

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
            lock (resCtrlsLO)
            {
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
            // Fade overaly above everything but scollbar
            Color colFade = getFadeColor();
            if (colFade.A != 0)
            {
                using (Brush b = new SolidBrush(colFade))
                {
                    g.ResetTransform();
                    g.TranslateTransform(AbsLeft, AbsTop);
                    Rectangle rect = new Rectangle(1, 1, cw, ch);
                    g.Clip = new Region(rect);
                    g.FillRectangle(b, rect);
                }
            }
        }
    }
}
