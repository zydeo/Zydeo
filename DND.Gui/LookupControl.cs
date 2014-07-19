using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

using DND.Common;
using DND.HanziLookup;
using DND.Gui.Zen;

namespace DND.Gui
{
    internal class LookupControl : ZenControl
    {
        bool searchTraditional = true;
        bool searchSimplified = true;
        private const double looseness = 0.25;  // the "looseness" of lookup, 0-1, higher == looser, looser more computationally intensive
        private const int numResults = 15;      // maximum number of results to return with each lookup

        private FileStream fsStrokes;
        private BinaryReader brStrokes;
        private StrokesDataSource strokesData;

        private WritingPad writingPad;
        private ResultsControl resCtrl;
        private CharPicker cpCtrl;
        private SearchInputControl siCtrl;

        private readonly HashSet<StrokesMatcher> runningMatchers = new HashSet<StrokesMatcher>();

        public LookupControl(ZenControlBase owner)
            : base(owner)
        {
            fsStrokes = new FileStream("strokes-extended.dat", FileMode.Open, FileAccess.Read);
            brStrokes = new BinaryReader(fsStrokes);
            strokesData = new StrokesDataSource(brStrokes);

            writingPad = new WritingPad(this);
            writingPad.RelLogicalLocation = new Point(5, 5);
            writingPad.LogicalSize = new Size(200, 200);
            writingPad.StrokesChanged += writingPad_StrokesChanged;

            cpCtrl = new CharPicker(this);
            //cpCtrl.FontFace = "Noto Sans S Chinese Regular";
            cpCtrl.FontFace = "䡡湄楮札䍓ⵆ潮瑳";
            //cpCtrl.FontFace = "SimSun";
            cpCtrl.RelLogicalLocation = new Point(5, 210);
            cpCtrl.LogicalSize = new Size(200, 80);
            cpCtrl.CharPicked += cpCtrl_CharPicked;

            siCtrl = new SearchInputControl(Scale);
            RegisterWinFormsControl(siCtrl);
            siCtrl.Location = new Point(writingPad.AbsRect.Right + writingPad.RelRect.Left, writingPad.AbsRect.Top);
            siCtrl.StartSearch += siCtrl_StartSearch;

            resCtrl = new ResultsControl(this);
            resCtrl.RelLocation = new Point(writingPad.RelRect.Right + writingPad.RelRect.Left, siCtrl.Bottom + writingPad.RelRect.Top);
        }

        public override void Dispose()
        {
            if (brStrokes != null) brStrokes.Dispose();
            if (fsStrokes != null) fsStrokes.Dispose();
            base.Dispose();
        }

        private void writingPad_StrokesChanged(object sender, IEnumerable<WritingPad.Stroke> strokes)
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

        private void recognize(object ctxt)
        {
            WrittenCharacter wc = ctxt as WrittenCharacter;
            CharacterDescriptor id = wc.BuildCharacterDescriptor();
            strokesData.Reset();
            StrokesMatcher matcher = new StrokesMatcher(id,
                                                     searchTraditional,
                                                     searchSimplified,
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

        private void siCtrl_StartSearch()
        {
            populateResults();
        }

        private void populateResults()
        {
            CedictMeaning[] xmAll = new CedictMeaning[9];
            xmAll[0] = new CedictMeaning(null, "pot-scrubbing brush made of bamboo strips", null);
            xmAll[1] = new CedictMeaning(null, "basket (container) for chopsticks", null);
            xmAll[2] = new CedictMeaning(null, "variant of 筲[shao1]", null);
            xmAll[3] = new CedictMeaning(null, "mask of a god used in ceremonies to exorcise demons and drive away pestilence", null);
            xmAll[4] = new CedictMeaning("(archaic)", "ugly", null);
            xmAll[5] = new CedictMeaning(null, "pith", "(soft interior of plant stem)");
            xmAll[6] = new CedictMeaning(null, "spoonbill", null);
            xmAll[7] = new CedictMeaning(null, "ibis", null);
            xmAll[8] = new CedictMeaning(null, "family Threskiornidae", null);
            List<CedictResult> rs = new List<CedictResult>();
            for (int i = 0; i != 99; ++i)
            {
                int meaningCount = (i % 9);
                CedictMeaning[] xm = new CedictMeaning[meaningCount + 2];
                xm[0] = new CedictMeaning(null, "ITEM-" + (i + 1).ToString("D3"), null);
                for (int j = 0; j <= meaningCount; ++j)
                    xm[j + 1] = xmAll[j];
                string[] xs = new string[2];
                xs[0] = "ài​";
                xs[1] = "qíng";
                CedictEntry ce = new CedictEntry("爱情", "爱情",
                    new ReadOnlyCollection<string>(xs),
                    new ReadOnlyCollection<CedictMeaning>(xm));
                CedictResult cr = new CedictResult(ce);
                rs.Add(cr);
            }
            resCtrl.SetResults(new ReadOnlyCollection<CedictResult>(rs), 99);
        }

        private void cpCtrl_CharPicked(char c)
        {
            writingPad.Clear();
            cpCtrl.SetItems(null);
            siCtrl.InsertCharacter(c);
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
            siCtrl.Location = new Point(writingPad.AbsRect.Right + writingPad.RelRect.Left, writingPad.AbsRect.Top);
            siCtrl.Width = Width - resCtrl.RelRect.Left - 5;
            resCtrl.Size = new Size(Width - resCtrl.RelRect.Left - 5, Height - resCtrl.RelRect.Top - 5);
        }
    }
}
