using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ZD.Gui.Zen;

namespace ZD.Gui
{
    public partial class ResultsCtxtControl : UserControl, ICtxtMenuControl
    {
        public ResultsCtxtControl()
        {
            InitializeComponent();
            // http://stackoverflow.com/questions/2032381/row-column-coloring-for-tablelayoutpanel-vs2008-winform
        }

        public void DoNavKey(CtxtMenuNavKey key)
        {

        }
    }
}
