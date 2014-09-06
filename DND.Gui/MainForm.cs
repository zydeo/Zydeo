using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DND.Common;
using DND.Gui.Zen;

namespace DND.Gui
{
    public class MainForm : ZenTabbedForm
    {
        private LookupControl lc;
        private SettingsControl stgs;
        private ITextProvider tprov;

        public MainForm(ICedictEngineFactory dictFact, ITextProvider tprov)
        {
            this.tprov = tprov;

            LogicalSize = new Size(800, 500);
            Header = tprov.GetString("WinHeader");
            lc = new LookupControl(this, dictFact, tprov);
            stgs = new SettingsControl(this);
            MainTab = new ZenTab(stgs, tprov.GetString("TabMain"));
            Tabs.Add(new ZenTab(lc, tprov.GetString("TabLookup")));
        }
    }
}
