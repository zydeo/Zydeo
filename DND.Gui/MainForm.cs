using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DND.Gui.Zen;

namespace DND.Controls
{
    public class MainForm : ZenTabbedForm
    {
        private LookupControl lc;
        private SettingsControl stgs;

        public MainForm()
        {
            LogicalSize = new Size(800, 500);
            Header = "Zydeo Chinese-English dictionary";
            lc = new LookupControl(Scale, this);
            stgs = new SettingsControl(Scale, this);
            MainTab = new ZenTab(stgs, "Zydeo video");
            //MainTabHeader = "Zydeo";
            Tabs.Add(new ZenTab(lc, "Lookup"));
        }
    }
}
