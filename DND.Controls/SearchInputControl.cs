using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DND.Controls
{
    public partial class SearchInputControl : UserControl
    {
        public delegate void StartSearchDelegate();
        public event StartSearchDelegate StartSearch;

        public SearchInputControl()
        {
            InitializeComponent();
            txtInput.KeyPress += txtInput_KeyPress;
        } 

        void txtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                if (StartSearch != null) StartSearch();
            }
        }

    }
}
