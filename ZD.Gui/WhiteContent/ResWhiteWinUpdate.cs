using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace ZD.Gui.WhiteContent
{
    internal partial class ResWhiteWinUpdate : UserControl
    {
        public ResWhiteWinUpdate(int vmaj, int vmin, DateTime rdate, string rnotes,
            UpdateNowDelegate updateNowDelegate)
        {
            InitializeComponent();
            if (Process.GetCurrentProcess().ProcessName == "devenv") return;
        }
    }
}
