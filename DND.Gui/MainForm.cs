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

        public MainForm(ICedictEngineFactory dictFact, ITextProvider tprov)
        {
            LogicalSize = new Size(800, 500);
            Header = Texts.WinHeader;
            lc = new LookupControl(this, dictFact, tprov);
            stgs = new SettingsControl(this);
            MainTab = new ZenTab(stgs, Texts.TabMain);
            //MainTabHeader = "Zydeo";
            Tabs.Add(new ZenTab(lc, Texts.TabLookup));
        }
    }
}
