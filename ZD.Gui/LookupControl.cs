using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Reflection;

using ZD.Common;
using ZD.HanziLookup;
using ZD.Gui.Zen;

namespace ZD.Gui
{
    /// <summary>
    /// Tab control that includes writing pad, search text entry field, results etc.
    /// </summary>
    internal partial class LookupControl : ZenControl
    {
        /// <summary>
        /// Dictionary factory: we use this to create dictionary in worker thread, after startup, when window is already shown.
        /// </summary>
        private readonly ICedictEngineFactory dictFact;
        /// <summary>
        /// Localized UI texts.
        /// </summary>
        private readonly ITextProvider tprov;
        /// <summary>
        /// Padding at our current scaling factor.
        /// </summary>
        private readonly int padding;

        /// <summary>
        /// Script: for results display, and filter for character recognition.
        /// </summary>
        private SearchScript searchScript = SearchScript.Simplified;

        /// <summary>
        /// Search language preference.
        /// </summary>
        private SearchLang searchLang = SearchLang.Chinese;

        // --- To remove after refactoring HanziLookup
        private FileStream fsStrokes;
        private BinaryReader brStrokes;
        private StrokesDataSource strokesData;
        private readonly HashSet<StrokesMatcher> runningMatchers = new HashSet<StrokesMatcher>();
        // --- END To remove after refactoring HanziLookup

        /// <summary>
        /// My dictionary engine, used for lookup.
        /// </summary>
        private ICedictEngine dict;

        /// <summary>
        /// Writing pad control.
        /// </summary>
        private WritingPad writingPad;
        /// <summary>
        /// Clear writing pad button.
        /// </summary>
        private ZenGradientButton btnClearWritingPad;
        /// <summary>
        /// Undo last stroke button.
        /// </summary>
        private ZenGradientButton btnUndoStroke;
        /// <summary>
        /// Lookup results.
        /// </summary>
        private ResultsControl ctrlResults;
        /// <summary>
        /// Character picker under writing pad.
        /// </summary>
        private CharPicker ctrlCharPicker;
        /// <summary>
        /// Search input at top (includes WinForms text box).
        /// </summary>
        private SearchInputControl ctrlSearchInput;
        /// <summary>
        /// Simplified/traditional script selector next to search input.
        /// </summary>
        private ZenGradientButton btnSimpTrad;
        /// <summary>
        /// English/Chinese search language selector next to search input.
        /// </summary>
        private ZenGradientButton btnSearchLang;

        /// <summary>
        /// Ctor.
        /// </summary>
        public LookupControl(ZenControlBase owner, ICedictEngineFactory dictFact, ITextProvider tprov)
            : base(owner)
        {
            this.dictFact = dictFact;
            this.tprov = tprov;
            padding = (int)Math.Round(5.0F * Scale);

            // Init search language and script from user settings
            searchLang = AppSettings.SearchLang;
            searchScript = AppSettings.SearchScript;

            // Init HanziLookup
            fsStrokes = new FileStream(Magic.StrokesFileName, FileMode.Open, FileAccess.Read);
            brStrokes = new BinaryReader(fsStrokes);
            strokesData = new StrokesDataSource(brStrokes);

            // Writing pad
            writingPad = new WritingPad(this, tprov);
            writingPad.RelLocation = new Point(padding, padding);
            writingPad.LogicalSize = new Size(200, 200);
            writingPad.StrokesChanged += writingPad_StrokesChanged;

            // Images for buttons under writing pad; will get owned by buttons, not that it matters.
            Assembly a = Assembly.GetExecutingAssembly();
            var imgStrokesClear = Image.FromStream(a.GetManifestResourceStream("ZD.Gui.Resources.strokes-clear.png"));
            var imgStrokesUndo = Image.FromStream(a.GetManifestResourceStream("ZD.Gui.Resources.strokes-undo.png"));

            // Clear and undo buttons under writing pad.
            float leftBtnWidth = writingPad.Width / 2 + 1;
            float btnHeight = 22.0F * Scale;
            // --
            btnClearWritingPad = new ZenGradientButton(this);
            btnClearWritingPad.RelLocation = new Point(writingPad.RelLeft, writingPad.RelBottom - 1);
            btnClearWritingPad.Size = new Size((int)leftBtnWidth, (int)btnHeight);
            btnClearWritingPad.Text = tprov.GetString("WritingPadClear");
            btnClearWritingPad.SetFont(ZenParams.GenericFontFamily, 9.0F);
            btnClearWritingPad.Padding = (int)(3.0F * Scale);
            btnClearWritingPad.Image = imgStrokesClear;
            btnClearWritingPad.Enabled = false;
            btnClearWritingPad.MouseClick += onClearWritingPad;
            // --
            btnUndoStroke = new ZenGradientButton(this);
            btnUndoStroke.RelLocation = new Point(btnClearWritingPad.RelRight - 1, writingPad.RelBottom - 1);
            btnUndoStroke.Size = new Size(writingPad.RelRight - btnUndoStroke.RelLeft, (int)btnHeight);
            btnUndoStroke.Text = tprov.GetString("WritingPadUndo");
            btnUndoStroke.SetFont(ZenParams.GenericFontFamily, 9.0F);
            btnUndoStroke.Padding = (int)(3.0F * Scale);
            btnUndoStroke.Image = imgStrokesUndo;
            btnUndoStroke.Enabled = false;
            btnUndoStroke.MouseClick += onUndoStroke;
            // --
            btnClearWritingPad.Tooltip = new ClearUndoTooltips(btnClearWritingPad, true, tprov, padding);
            btnUndoStroke.Tooltip = new ClearUndoTooltips(btnUndoStroke, false, tprov, padding);
            // --

            // Character picker control under writing pad.
            ctrlCharPicker = new CharPicker(this, tprov);
            ctrlCharPicker.FontFam = Magic.ZhoContentFontFamily;
            ctrlCharPicker.FontScript = searchScript == SearchScript.Traditional ? IdeoScript.Trad : IdeoScript.Simp;
            ctrlCharPicker.RelLocation = new Point(padding, btnClearWritingPad.RelBottom + padding);
            ctrlCharPicker.LogicalSize = new Size(200, 80);
            ctrlCharPicker.CharPicked += onCharPicked;

            // Search input control at top
            ctrlSearchInput = new SearchInputControl(this, tprov);
            ctrlSearchInput.RelLocation = new Point(writingPad.RelRight + padding, padding);
            ctrlSearchInput.StartSearch += onStartSearch;

            // Tweaks for Chinese text on UI buttons
            //var siZho = HanziMeasure.Instance.GetMeasures(Magic.ZhoButtonFontFamily, Magic.ZhoButtonFontSize);
            //float ofsZho = -siZho.RealRect.Top;
            float ofsZho = 0;

            // Script selector button to the right of search input control
            btnSimpTrad = new ZenGradientButton(this);
            btnSimpTrad.RelTop = padding;
            btnSimpTrad.Height = ctrlSearchInput.Height;
            btnSimpTrad.SetFont(Magic.ZhoButtonFontFamily, Magic.ZhoButtonFontSize);
            btnSimpTrad.Width = getSimpTradWidth();
            //btnSimpTrad.ForcedCharHeight = siZho.RealRect.Height;
            btnSimpTrad.ForcedCharVertOfs = ofsZho;
            btnSimpTrad.RelLeft = Width - padding - btnSimpTrad.Width;
            btnSimpTrad.Height = ctrlSearchInput.Height;
            btnSimpTrad.MouseClick += onSimpTrad;

            // Search language selector to the right of search input control
            btnSearchLang = new ZenGradientButton(this);
            btnSearchLang.RelTop = padding;
            btnSearchLang.Height = ctrlSearchInput.Height;
            btnSearchLang.SetFont(Magic.ZhoButtonFontFamily, Magic.ZhoButtonFontSize);
            btnSearchLang.Width = getSearchLangWidth();
            //btnSearchLang.ForcedCharHeight = siZho.RealRect.Height;
            btnSearchLang.ForcedCharVertOfs = ofsZho;
            btnSearchLang.RelLeft = btnSimpTrad.RelLeft - padding - btnSearchLang.Width;
            btnSearchLang.Height = ctrlSearchInput.Height;
            btnSearchLang.MouseClick += onSearchLang;

            // Update button texts; do it here so tooltip locations will be correct.
            simpTradChanged();
            searchLangChanged();

            // Lookup results control.
            ctrlResults = new ResultsControl(this, tprov, onLookupThroughLink, onGetEntry);
            ctrlResults.RelLocation = new Point(writingPad.RelRight + padding, ctrlSearchInput.RelBottom + padding);
        }

        /// <summary>
        /// Size changed event handler: rearrange all constituents.
        /// </summary>
        protected override void OnSizeChanged()
        {
            ctrlSearchInput.RelLocation = new Point(writingPad.RelRight + padding, padding);
            ctrlSearchInput.Width = Width - ctrlResults.RelLeft - btnSimpTrad.Width - btnSearchLang.Width - 3 * padding;
            btnSimpTrad.RelLeft = Width - padding - btnSimpTrad.Width;
            btnSimpTrad.Height = ctrlSearchInput.Height;
            btnSearchLang.RelLeft = btnSimpTrad.RelLeft - padding - btnSearchLang.Width;
            btnSearchLang.Height = ctrlSearchInput.Height;
            ctrlResults.Size = new Size(Width - ctrlResults.RelLeft - padding, Height - ctrlResults.RelTop - padding);
        }

        /// <summary>
        /// Dispose: free acquired resources.
        /// </summary>
        public override void Dispose()
        {
            if (brStrokes != null) brStrokes.Dispose();
            if (fsStrokes != null) fsStrokes.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// After form is loaded - delayed initi, e.g., dictionary and hanzilookup.
        /// </summary>
        protected override void OnFormLoaded()
        {
            base.OnFormLoaded();
            ThreadPool.QueueUserWorkItem(loadDictionary);
        }

        /// <summary>
        /// Loads dictionary in worker thread.
        /// </summary>
        /// <param name="ctxt"></param>
        private void loadDictionary(object ctxt)
        {
            Thread.Sleep(100);
            dict = dictFact.Create(Magic.DictFileName);
        }

        /// <summary>
        /// Start recognizing a character in a BG thread after strokes have changed.
        /// </summary>
        private void startNewCharRecog(IEnumerable<WritingPad.Stroke> strokes)
        {
            // If there are other matchers running, stop them now
            lock (runningMatchers)
            {
                foreach (StrokesMatcher sm in runningMatchers) sm.Stop();
            }
            // Convert stroke data to HanziLookup's format
            WrittenCharacter wc = new WrittenCharacter();
            foreach (WritingPad.Stroke stroke in strokes)
            {
                WrittenStroke ws = new WrittenStroke();
                foreach (PointF p in stroke.Points)
                {
                    WrittenPoint wp = new WrittenPoint((int)(p.X), (int)(p.Y));
                    ws.AddPoint(wp, ref wc.LeftX, ref wc.RightX, ref wc.TopY, ref wc.BottomY);
                }
                wc.AddStroke(ws);
            }
            if (wc.StrokeList.Count == 0)
            {
                // Don't bother doing anything if nothing has been input yet (number of strokes == 0).
                ctrlCharPicker.SetItems(null);
                return;
            }

            ThreadPool.QueueUserWorkItem(recognize, wc);
        }

        /// <summary>
        /// Event handler, called by writing pad when strokes have changed.
        /// </summary>
        private void writingPad_StrokesChanged(IEnumerable<WritingPad.Stroke> strokes)
        {
            startNewCharRecog(strokes);
            int cnt = 0;
            foreach (var x in strokes) ++cnt;
            btnClearWritingPad.Enabled = btnUndoStroke.Enabled = cnt > 0;
        }

        /// <summary>
        /// Character recognition worker function (run in worker thread).
        /// </summary>
        private void recognize(object ctxt)
        {
            bool error = false;
            char[] matches = null;
            StrokesMatcher matcher = null;
            try
            {
                WrittenCharacter wc = ctxt as WrittenCharacter;
                CharacterDescriptor id = wc.BuildCharacterDescriptor();
                strokesData.Reset();
                matcher = new StrokesMatcher(id,
                    searchScript != SearchScript.Simplified,
                    searchScript != SearchScript.Traditional,
                    Magic.HanziLookupLooseness,
                    Magic.HanziLookupNumResults,
                    strokesData);
                int matcherCount;
                lock (runningMatchers)
                {
                    runningMatchers.Add(matcher);
                    matcherCount = runningMatchers.Count;
                }
                while (matcher.IsRunning && matcherCount > 1)
                {
                    Thread.Sleep(50);
                    lock (runningMatchers) { matcherCount = runningMatchers.Count; }
                }
                if (!matcher.IsRunning)
                {
                    lock (runningMatchers) { runningMatchers.Remove(matcher); matcher = null; }
                    return;
                }
                matches = matcher.DoMatching();
                if (matches == null) return;
            }
            catch (DiagnosticException dex)
            {
                // Errors not handled locally are only for diagnostics
                // We actually handle every real-life exception locally here.
                if (!dex.HandleLocally) throw;
                error = true;
                AppErrorLogger.Instance.LogException(dex, false);
            }
            catch (Exception ex)
            {
                error = true;
                AppErrorLogger.Instance.LogException(ex, false);
            }
            finally
            {
                if (matcher != null)
                {
                    lock (runningMatchers) { runningMatchers.Remove(matcher); }
                }
            }
            InvokeOnForm((MethodInvoker)delegate
            {
                if (error) ctrlCharPicker.SetError();
                else ctrlCharPicker.SetItems(matches);
            });
        }

        /// <summary>
        /// Event handler: "undo last stroke" button clicked.
        /// </summary>
        void onUndoStroke(ZenControlBase sender)
        {
            writingPad.UndoLast();
            // Button states will be updated in "strokes changed" event handler.
        }

        /// <summary>
        /// Event handler: "clear strokes" button clicked.
        /// </summary>
        void onClearWritingPad(ZenControlBase sender)
        {
            writingPad.Clear();
            // Button states will be updated in "strokes changed" event handler.
        }

        /// <summary>
        /// Event handler: search initiated.
        /// </summary>
        private void onStartSearch(object sender, string text)
        {
            // TO-DO: Move to worker thread!!
            // If dictionary is not yet initialized, we just don't search.
            if (dict == null) return;
            // If search request comes from input control: select all text. Easier to overwrite.
            // Otherwise, user inserted a recognized character. Then we want to allow her to keep writing > no select.
            if (sender == ctrlSearchInput) ctrlSearchInput.SelectAll();
            // Fade out whatever is currently shown
            ctrlResults.FadeOut();
            // Launch lookup and rendering in worker thread
            lock (lookupItems)
            {
                ++lookupId;
                lookupItems.Add(new LookupItem(lookupId, text, searchScript, searchLang));
            }
            ThreadPool.QueueUserWorkItem(search);
        }

        /// <summary>
        /// On query to look up, queued for processing in worker thread.
        /// </summary>
        private class LookupItem
        {
            public readonly int ID;
            public readonly string Text;
            public readonly SearchScript Script;
            public readonly SearchLang Lang;
            public LookupItem(int id, string text, SearchScript script, SearchLang lang)
            { ID = id; Text = text; Script = script; Lang = lang; }
        }

        /// <summary>
        /// Items to look up - new item is added every time user triggers lookup.
        /// </summary>
        private readonly List<LookupItem> lookupItems = new List<LookupItem>();

        /// <summary>
        /// ID of latest lookup in progress.
        /// </summary>
        private int lookupId = -1;

        /// <summary>
        /// Dictionary lookup and results rendering in worker thread.
        /// </summary>
        /// <param name="ctxt"></param>
        private void search(object ctxt)
        {
            LookupItem li;
            // Pick very last item in queue to look up; clear rest of queue
            lock (lookupItems)
            {
                if (lookupItems.Count == 0) return;
                li = lookupItems[lookupItems.Count - 1];
                lookupItems.Clear();
                // If this is not very last request, don't even bother
                if (li.ID != lookupId) return;
            }
            // Look up in dictionary
            CedictLookupResult res = dict.Lookup(li.Text, li.Script, li.Lang);
            // Call below transfers ownership of entry provider to results control.
            bool shown = ctrlResults.SetResults(li.ID, res.EntryProvider, res.Results, searchScript);
            // If these results came too late (a long query completing too late, when a later fast query already completed)
            // Then we're done, no flashing.
            if (!shown) return;
            // Did lookup language change?
            if (res.ActualSearchLang != li.Lang)
            {
                searchLang = res.ActualSearchLang;
                searchLangChanged();
                btnSearchLang.Flash();
            }
        }

        /// <summary>
        /// Gets ideal width of script selector button.
        /// </summary>
        private int getSimpTradWidth()
        {
            int w = btnSimpTrad.GetPreferredWidth(false, "m" + Magic.SearchSimp);
            w = Math.Max(w, btnSimpTrad.GetPreferredWidth(false, "m" + Magic.SearchTrad));
            w = Math.Max(w, btnSimpTrad.GetPreferredWidth(false, "m" + Magic.SearchBoth));
            return w;
        }

        /// <summary>
        /// Gets ideal width of search language selector button.
        /// </summary>
        private int getSearchLangWidth()
        {
            int w = btnSearchLang.GetPreferredWidth(false, "m" + Magic.SearchLangEng);
            return w;
        }

        /// <summary>
        /// Updates text of script selector button based on current search script; updates user settings.
        /// </summary>
        private void simpTradChanged()
        {
            // Set button "text"
            string text;
            if (searchScript == SearchScript.Simplified)
                text = Magic.SearchSimp;
            else if (searchScript == SearchScript.Traditional)
                text = Magic.SearchTrad;
            else text = Magic.SearchBoth;
            btnSimpTrad.Text = text;
            btnSimpTrad.Invalidate();
            // Set button tooltip
            btnSimpTrad.Tooltip = new SearchOptionsTooltip(btnSimpTrad, false, tprov, searchScript, searchLang,
                padding, btnSimpTrad.Width);
            // Save in user settings
            AppSettings.SearchScript = searchScript;
        }

        /// <summary>
        /// Updates text of search language selector based on current setting; updates user settings.
        /// </summary>
        private void searchLangChanged()
        {
            if (searchLang == SearchLang.Chinese) btnSearchLang.Text = Magic.SearchLangZho;
            else btnSearchLang.Text = Magic.SearchLangEng;
            btnSearchLang.Invalidate();
            // Set button tooltip
            btnSearchLang.Tooltip = new SearchOptionsTooltip(btnSearchLang, true, tprov, searchScript, searchLang,
                padding, btnSimpTrad.RelRight - btnSearchLang.RelLeft);
            // Save in user settings
            AppSettings.SearchLang = searchLang;
        }

        /// <summary>
        /// Event handler: script selector button clicked.
        /// </summary>
        private void onSimpTrad(ZenControlBase sender)
        {
            // Next in row
            int scri = (int)searchScript;
            ++scri;
            if (scri > 2) scri = 0;
            searchScript = (SearchScript)scri;
            // Update button
            simpTradChanged();
            // Change character picker's font
            // Only if it really changes - triggers calibration
            ctrlCharPicker.FontScript = searchScript == SearchScript.Traditional ? IdeoScript.Trad : IdeoScript.Simp;
            // Re-recognize strokes, if there are any
            startNewCharRecog(writingPad.Strokes);
        }

        /// <summary>
        /// Event handler: search language button clicked.
        /// </summary>
        private void onSearchLang(ZenControlBase sender)
        {
            // Toggle between English and Chinese
            if (searchLang == SearchLang.Chinese) searchLang = SearchLang.Target;
            else searchLang = SearchLang.Chinese;
            // Update button
            searchLangChanged();
        }

        /// <summary>
        /// Event handler: recognized character picked.
        /// </summary>
        private void onCharPicked(char c)
        {
            writingPad.Clear();
            ctrlCharPicker.SetItems(null);
            ctrlSearchInput.InsertCharacter(c);
            onStartSearch(this, ctrlSearchInput.Text);
        }

        /// <summary>
        /// Event handler: lookup started by clicking a link in a result control.
        /// </summary>
        private void onLookupThroughLink(string queryString)
        {
            ctrlSearchInput.Text = queryString;
            ctrlSearchInput.SelectAll();
            onStartSearch(this, ctrlSearchInput.Text);
        }

        /// <summary>
        /// Event handler: retrieves a specific entry from the dictionary.
        /// </summary>
        private CedictEntry onGetEntry(int entryId)
        {
            return dict.GetEntry(entryId);
        }

        /// <summary>
        /// Render.
        /// </summary>
        public override void DoPaint(Graphics g)
        {
            using (Brush b = new SolidBrush(ZenParams.PaddingBackColor))
            {
                g.FillRectangle(b, 0, 0, Width, Height);
            }

            DoPaintChildren(g);
        }
    }
}
