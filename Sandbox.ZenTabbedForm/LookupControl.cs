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
            List<CedictResult> rs = new List<CedictResult>();
            for (int i = 0; i != 99; ++i)
            {
                string[] xs = new string[0];
                CedictMeaning[] xm = new CedictMeaning[0];
                CedictEntry ce = new CedictEntry("爱情" + (i + 1).ToString(),
                    "爱情" + (i + 1).ToString(), new ReadOnlyCollection<string>(xs), new ReadOnlyCollection<CedictMeaning>(xm));
                CedictResult cr = new CedictResult(ce);
                rs.Add(cr);
            }
            resultsCtrl.SetResults(new ReadOnlyCollection<CedictResult>(rs), 99);
        }
    }
}
