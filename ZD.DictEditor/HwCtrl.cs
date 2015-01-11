using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ZD.DictEditor
{
    public partial class HwCtrl : UserControl
    {
        public HwCtrl()
        {
            InitializeComponent();
        }

        public DictData.HwData Data
        {
            set
            {
                lblHeadword.Text = value.Simp;
            }
        }

        public bool Selected
        {
            set
            {
                BackColor = value ? Color.LightBlue : Color.Magenta;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            lblHeadword.Width = Width - lblHeadword.Left;
            lblExtract.Width = Width - lblExtract.Left;
        }
    }
}
