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
    public partial class EntryEditor : UserControl
    {
        public EntryEditor()
        {
            InitializeComponent();
            pnlFrame.SizeChanged += onPanelSizeChanged;
        }

        void onPanelSizeChanged(object sender, EventArgs e)
        {
            arrange();
        }

        private void arrange()
        {
            Size sz = pnlFrame.ClientSize;
            flowHints.Location = new Point(0, sz.Height - flowHints.Height);
            flowHints.Width = sz.Width;
            txtEntry.Size = new Size(sz.Width, sz.Height - flowHints.Height - 1);
            txtEntry.Location = new Point(0, 0);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            arrange();
        }
    }
}
