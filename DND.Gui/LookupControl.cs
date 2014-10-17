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

using DND.Common;
using DND.HanziLookup;
using DND.Gui.Zen;

namespace DND.Gui
{
    /// <summary>
    /// Tab control that includes writing pad, search text entry field, results etc.
    /// </summary>
    internal class LookupControl : ZenControl
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

            // Init HanziLookup
            fsStrokes = new FileStream("strokes-zydeo.dat", FileMode.Open, FileAccess.Read);
            brStrokes = new BinaryReader(fsStrokes);
            strokesData = new StrokesDataSource(brStrokes);

            // Writing pad
            writingPad = new WritingPad(this);
            writingPad.RelLocation = new Point(padding, padding);
            writingPad.LogicalSize = new Size(200, 200);
            writingPad.StrokesChanged += writingPad_StrokesChanged;

            // Images for buttons under writing pad; will get owned by buttons, not that it matters.
            Assembly a = Assembly.GetExecutingAssembly();
            var imgStrokesClear = Image.FromStream(a.GetManifestResourceStream("DND.Gui.Resources.strokes-clear.png"));
            var imgStrokesUndo = Image.FromStream(a.GetManifestResourceStream("DND.Gui.Resources.strokes-undo.png"));

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

            // Character picker control under writing pad.
            ctrlCharPicker = new CharPicker(this);
            ctrlCharPicker.FontFace = ZenParams.ZhoContentFontFamily;
            ctrlCharPicker.RelLocation = new Point(padding, btnClearWritingPad.RelBottom + padding);
            ctrlCharPicker.LogicalSize = new Size(200, 80);
            ctrlCharPicker.CharPicked += onCharPicked;

            // Search input control at top
            ctrlSearchInput = new SearchInputControl(this);
            ctrlSearchInput.RelLocation = new Point(writingPad.RelRight + padding, padding);
            ctrlSearchInput.StartSearch += onStartSearch;

            // Tweaks for Chinese text on UI buttons
            var siZho = HanziMeasure.Instance.GetMeasures(ZenParams.ZhoButtonFontFamily, ZenParams.ZhoButtonFontSize);
            float ofsZho = -siZho.RealRect.Top;

            // Script selector button to the right of search input control
            btnSimpTrad = new ZenGradientButton(this);
            btnSimpTrad.RelTop = padding;
            btnSimpTrad.Height = ctrlSearchInput.Height;
            btnSimpTrad.SetFont(ZenParams.ZhoButtonFontFamily, ZenParams.ZhoButtonFontSize);
            btnSimpTrad.Width = getSimpTradWidth();
            btnSimpTrad.ForcedCharHeight = siZho.RealRect.Height;
            btnSimpTrad.ForcedCharVertOfs = ofsZho;
            btnSimpTrad.MouseClick += onSimpTrad;
            setSimpTradText();

            // Search language selector to the right of search input control
            btnSearchLang = new ZenGradientButton(this);
            btnSearchLang.RelTop = padding;
            btnSearchLang.Height = ctrlSearchInput.Height;
            btnSearchLang.SetFont(ZenParams.ZhoButtonFontFamily, ZenParams.ZhoButtonFontSize);
            btnSearchLang.Width = getSearchLangWidth();
            btnSearchLang.ForcedCharHeight = siZho.RealRect.Height;
            btnSearchLang.ForcedCharVertOfs = ofsZho;
            btnSearchLang.MouseClick += onSearchLang;
            setSearchLangText();

            // Lookup results control.
            ctrlResults = new ResultsControl(this, tprov, onLookupThroughLink);
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
            WrittenCharacter wc = ctxt as WrittenCharacter;
            CharacterDescriptor id = wc.BuildCharacterDescriptor();
            strokesData.Reset();
            StrokesMatcher matcher = new StrokesMatcher(id,
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
                lock (runningMatchers) { runningMatchers.Remove(matcher); }
                return;
            }
            char[] matches = matcher.DoMatching();
            lock (runningMatchers) { runningMatchers.Remove(matcher); }
            if (matches == null) return;
            InvokeOnForm((MethodInvoker)delegate
            {
                ctrlCharPicker.SetItems(matches);
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
            CedictLookupResult res = dict.Lookup(text, searchScript, searchLang);
            // If search request comes from input control: select all text. Easier to overwrite.
            // Otherwise, user inserted a recognized character. Then we want to allow her to keep writing > no select.
            if (sender == ctrlSearchInput) ctrlSearchInput.SelectAll();
            // Did lookup language change?
            if (res.ActualSearchLang != searchLang)
            {
                searchLang = res.ActualSearchLang;
                setSearchLangText();
                btnSearchLang.Flash();
            }
            // Call below transfers ownership of entry provider to results control.
            ctrlResults.SetResults(res.EntryProvider, res.Results, searchScript);
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
        /// Updates text of script selector button based on current search script.
        /// </summary>
        private void setSimpTradText()
        {
            string text;
            if (searchScript == SearchScript.Simplified)
                text = Magic.SearchSimp;
            else if (searchScript == SearchScript.Traditional)
                text = Magic.SearchTrad;
            else text = Magic.SearchBoth;
            btnSimpTrad.Text = text;
            btnSimpTrad.Invalidate();
        }

        /// <summary>
        /// Updates text of search language selector based on current setting.
        /// </summary>
        private void setSearchLangText()
        {
            if (searchLang == SearchLang.Chinese) btnSearchLang.Text = Magic.SearchLangZho;
            else btnSearchLang.Text = Magic.SearchLangEng;
            btnSearchLang.Invalidate();
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
            setSimpTradText();
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
            setSearchLangText();
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
        /// Event handler: looked started by clicking a link in a result control.
        /// </summary>
        private void onLookupThroughLink(string queryString)
        {
            ctrlSearchInput.Text = queryString;
            ctrlSearchInput.SelectAll();
            onStartSearch(this, ctrlSearchInput.Text);
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
