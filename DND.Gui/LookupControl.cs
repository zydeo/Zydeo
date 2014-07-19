using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using DND.Common;
using DND.Gui.Zen;

namespace DND.Controls
{
    internal class LookupControl : ZenControl
    {
        private WritingPad writingPad;
        private ResultsControl resCtrl;
        private ZenControl blahCtrl;

        public LookupControl(float scale, ZenControlBase owner)
            : base(scale, owner)
        {
            writingPad = new WritingPad(scale, this);
            writingPad.RelLogicalLocation = new Point(5, 5);
            writingPad.LogicalSize = new Size(200, 200);
            
            resCtrl = new ResultsControl(scale, this);
            resCtrl.RelLocation = new Point(writingPad.RelRect.Right + writingPad.RelRect.Left, writingPad.RelRect.Top);

            blahCtrl = new ZenControl(scale, this);
            blahCtrl.RelLogicalLocation = new Point(5, 210);
            blahCtrl.LogicalSize = new Size(200, 20);
            blahCtrl.MouseClick += blahCtrl_MouseClick;
        }

        void blahCtrl_MouseClick(ZenControlBase sender)
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

        public override void DoPaint(Graphics g)
        {
            using (Brush b = new SolidBrush(ZenParams.PaddingBackColor))
            {
                g.FillRectangle(b, AbsLocation.X, AbsLocation.Y, Size.Width, Size.Height);
            }
            DoPaintChildren(g);
        }

        protected override void OnSizeChanged()
        {
            resCtrl.Size = new Size(Width - resCtrl.RelRect.Left - 5, Height - resCtrl.RelRect.Top - 5);
        }
    }
}
