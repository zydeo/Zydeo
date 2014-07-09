using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DND.Common;

namespace Sandbox
{
    public partial class LookupControl : UserControl
    {
        public LookupControl()
        {
            InitializeComponent();
            resultsCtrl.SetScale(AutoScaleDimensions.Height / 13.0F);
            siCtrl.StartSearch += siCtrl_StartSearch;
        }

        void siCtrl_StartSearch()
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
            resultsCtrl.SetResults(new ReadOnlyCollection<CedictResult>(rs), 99);
        }
    }
}
