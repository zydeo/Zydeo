using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DND.Controls;

namespace Sandbox
{
    public partial class MainForm : ZenTabbedForm
    {
        private LookupControl lc;
        private SettingsControl stgs;

        public MainForm()
        {
            LogicalSize = new Size(800, 500);
            Header = "Zydeo Chinese-English dictionary";
            lc = new LookupControl();
            stgs = new SettingsControl();
            MainTab = stgs;
            MainTabHeader = "Zydeo";
            Tabs.Add(new ZenTab(lc, "Lookup"));
        }
    }
}
