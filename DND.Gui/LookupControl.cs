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
    internal class LookupControl : ZenControl
    {
        private readonly ICedictEngineFactory dictFact;
        private readonly ITextProvider tprov;
        private readonly int padding;

        private SearchScript searchScript = SearchScript.Simplified;

        private const double looseness = 0.25;  // the "looseness" of lookup, 0-1, higher == looser, looser more computationally intensive
        private const int numResults = 15;      // maximum number of results to return with each lookup
        private FileStream fsStrokes;
        private BinaryReader brStrokes;
        private StrokesDataSource strokesData;

        private ICedictEngine dict;

        private WritingPad writingPad;
        private ZenGradientButton btnClearWritingPad;
        private ZenGradientButton btnUndoStroke;
        private ResultsControl resCtrl;
        private CharPicker cpCtrl;
        private SearchInputControl siCtrl;
        private ZenGradientButton btnSimpTrad;

        private readonly HashSet<StrokesMatcher> runningMatchers = new HashSet<StrokesMatcher>();

        public LookupControl(ZenControlBase owner, ICedictEngineFactory dictFact, ITextProvider tprov)
            : base(owner)
        {
            this.dictFact = dictFact;
            this.tprov = tprov;
            padding = (int)Math.Round(5.0F * Scale);

            fsStrokes = new FileStream("strokes-zydeo.dat", FileMode.Open, FileAccess.Read);
            brStrokes = new BinaryReader(fsStrokes);
            strokesData = new StrokesDataSource(brStrokes);

            writingPad = new WritingPad(this);
            writingPad.RelLocation = new Point(padding, padding);
            writingPad.LogicalSize = new Size(200, 200);
            writingPad.StrokesChanged += writingPad_StrokesChanged;

            Assembly a = Assembly.GetExecutingAssembly();
            var imgStrokesClear = Image.FromStream(a.GetManifestResourceStream("DND.Gui.Resources.strokes-clear.png"));
            var imgStrokesUndo = Image.FromStream(a.GetManifestResourceStream("DND.Gui.Resources.strokes-undo.png"));

            float leftBtnWidth = writingPad.Width / 2 + 1;
            float btnHeight = 22.0F * Scale;

            btnClearWritingPad = new ZenGradientButton(this);
            btnClearWritingPad.RelLocation = new Point(writingPad.RelLeft, writingPad.RelBottom - 1);
            btnClearWritingPad.Size = new Size((int)leftBtnWidth, (int)btnHeight);
            btnClearWritingPad.Text = tprov.GetString("WritingPadClear");
            btnClearWritingPad.SetFont(ZenParams.GenericFontFamily, 9.0F);
            btnClearWritingPad.Padding = (int)(3.0F * Scale);
            btnClearWritingPad.Image = imgStrokesClear;
            btnClearWritingPad.Enabled = false;
            btnClearWritingPad.MouseClick += btnClearWritingPad_MouseClick;
           
            btnUndoStroke = new ZenGradientButton(this);
            btnUndoStroke.RelLocation = new Point(btnClearWritingPad.RelRight - 1, writingPad.RelBottom - 1);
            btnUndoStroke.Size = new Size(writingPad.RelRight - btnUndoStroke.RelLeft, (int)btnHeight);
            btnUndoStroke.Text = tprov.GetString("WritingPadUndo");
            btnUndoStroke.SetFont(ZenParams.GenericFontFamily, 9.0F);
            btnUndoStroke.Padding = (int)(3.0F * Scale);
            btnUndoStroke.Image = imgStrokesUndo;
            btnUndoStroke.Enabled = false;
            btnUndoStroke.MouseClick += btnUndoStroke_MouseClick;

            cpCtrl = new CharPicker(this);
            cpCtrl.FontFace = "Noto Sans S Chinese Regular";
            //cpCtrl.FontFace = "䡡湄楮札䍓ⵆ潮瑳";
            //cpCtrl.FontFace = "SimSun";
            cpCtrl.RelLocation = new Point(padding, btnClearWritingPad.RelBottom + padding);
            cpCtrl.LogicalSize = new Size(200, 80);
            cpCtrl.CharPicked += cpCtrl_CharPicked;

            siCtrl = new SearchInputControl(this);
            siCtrl.RelLocation = new Point(writingPad.RelRight + padding, padding);
            siCtrl.StartSearch += onStartSearch;

            btnSimpTrad = new ZenGradientButton(this);
            btnSimpTrad.RelTop = padding;
            btnSimpTrad.Height = siCtrl.Height;
            btnSimpTrad.SetFont(ZenParams.ZhoFontFamily, ZenParams.ZhoButtonFontSize);
            btnSimpTrad.Width = getSimpTradWidth();
            btnSimpTrad.ForcedCharHeight = HanziMeasure.Instance.GetMeasures(ZenParams.ZhoFontFamily, ZenParams.ZhoButtonFontSize).RealRect.Height;
            btnSimpTrad.MouseClick += simpTradCtrl_MouseClick;
            setSimpTradText();

            resCtrl = new ResultsControl(this, tprov, lookupThroughLink);
            resCtrl.RelLocation = new Point(writingPad.RelRight + padding, siCtrl.RelBottom + padding);
        }

        public override void Dispose()
        {
            if (brStrokes != null) brStrokes.Dispose();
            if (fsStrokes != null) fsStrokes.Dispose();
            base.Dispose();
        }

        protected override void OnFormLoaded()
        {
            base.OnFormLoaded();
            ThreadPool.QueueUserWorkItem(loadDictionary);
        }

        private void loadDictionary(object ctxt)
        {
            Thread.Sleep(100);
            dict = dictFact.Create("cedict-zydeo.bin");
        }

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
                cpCtrl.SetItems(null);
                return;
            }

            ThreadPool.QueueUserWorkItem(recognize, wc);
        }

        private void writingPad_StrokesChanged(IEnumerable<WritingPad.Stroke> strokes)
        {
            startNewCharRecog(strokes);
            int cnt = 0;
            foreach (var x in strokes) ++cnt;
            btnClearWritingPad.Enabled = btnUndoStroke.Enabled = cnt > 0;
        }

        private void recognize(object ctxt)
        {
            WrittenCharacter wc = ctxt as WrittenCharacter;
            CharacterDescriptor id = wc.BuildCharacterDescriptor();
            strokesData.Reset();
            StrokesMatcher matcher = new StrokesMatcher(id,
                searchScript != SearchScript.Simplified,
                searchScript != SearchScript.Traditional,
                looseness,
                numResults,
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
                cpCtrl.SetItems(matches);
            });
        }

        void btnUndoStroke_MouseClick(ZenControlBase sender)
        {
            writingPad.UndoLast();
            // Button states will be updated in "strokes changed" event handler.
        }

        void btnClearWritingPad_MouseClick(ZenControlBase sender)
        {
            writingPad.Clear();
            // Button states will be updated in "strokes changed" event handler.
        }

        private void onStartSearch(object sender, string text)
        {
            if (dict == null) return;
            CedictLookupResult res = dict.Lookup(text, searchScript, SearchLang.Chinese);
            if (sender == siCtrl) siCtrl.SelectAll();
            resCtrl.SetResults(res.Results, searchScript);
            // DBG
            GC.Collect();
            GC.Collect();
        }

        private int getSimpTradWidth()
        {
            int w = btnSimpTrad.GetPreferredWidth(false, "m" + Texts.SearchSimp);
            w = Math.Max(w, btnSimpTrad.GetPreferredWidth(false, "m" + Texts.SearchTrad));
            w = Math.Max(w, btnSimpTrad.GetPreferredWidth(false, "m" + Texts.SearchBoth));
            return w;
        }

        private void setSimpTradText()
        {
            string text;
            if (searchScript == SearchScript.Simplified)
                text = Texts.SearchSimp;
            else if (searchScript == SearchScript.Traditional)
                text = Texts.SearchTrad;
            else text = Texts.SearchBoth;
            btnSimpTrad.Text = text;
            btnSimpTrad.Invalidate();
        }

        private void simpTradCtrl_MouseClick(ZenControlBase sender)
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
            // Re-render results list with desired script(s) shown
            resCtrl.ChangeScript(searchScript);
        }

        private void cpCtrl_CharPicked(char c)
        {
            writingPad.Clear();
            cpCtrl.SetItems(null);
            siCtrl.InsertCharacter(c);
            onStartSearch(this, siCtrl.Text);
        }

        private void lookupThroughLink(string queryString)
        {
            siCtrl.Text = queryString;
            siCtrl.SelectAll();
            onStartSearch(this, siCtrl.Text);
        }

        public override void DoPaint(Graphics g)
        {
            using (Brush b = new SolidBrush(ZenParams.PaddingBackColor))
            {
                g.FillRectangle(b, 0, 0, Width, Height);
            }

            DoPaintChildren(g);
        }

        protected override void OnSizeChanged()
        {
            siCtrl.RelLocation = new Point(writingPad.RelRight + padding, padding);
            siCtrl.Width = Width - resCtrl.RelLeft - btnSimpTrad.Width - 2 * padding;
            btnSimpTrad.RelLeft = Width - padding - btnSimpTrad.Width;
            btnSimpTrad.Height = siCtrl.Height;
            resCtrl.Size = new Size(Width - resCtrl.RelLeft - padding, Height - resCtrl.RelTop - padding);
        }
    }
}
