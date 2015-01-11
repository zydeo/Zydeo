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
            flowHints.Location = new Point(txtEntry.Left, sz.Height - flowHints.Height);
            flowHints.Width = sz.Width - 2 * txtEntry.Left;
            pnlSeparator.Location = new Point(0, flowHints.Top - 1);
            pnlSeparator.Size = new Size(sz.Width, 1);
            pnlEditorBg.Location = new Point(0, 0);
            pnlEditorBg.Size = new Size(sz.Width, sz.Height - flowHints.Height - 1);
            txtEntry.Size = new Size(sz.Width - 2 * txtEntry.Left, pnlEditorBg.Height - txtEntry.Top);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            arrange();
            txtEntry.TextChanged += onTextChanged;
        }

        private void onTextChanged(object sender, EventArgs e)
        {
        }

        public string StrSenses
        {
            get { return txtEntry.Text.Replace("\r\n", "/"); }
            set
            {
                txtEntry.Text = value.Replace("/", "\r\n");
                UpdateErrorBg();
            }
        }

        public bool HasErrors
        {
            get { return !isTextOk(txtEntry.Text); }
        }

        public void UpdateErrorBg()
        {
            bool textOk = isTextOk(txtEntry.Text);
            txtEntry.BackColor = textOk ? SystemColors.Window : Color.FromArgb(0xff, 0xb6, 0xc1);
            pnlEditorBg.BackColor = txtEntry.BackColor;
        }

        private bool isTextOk(string txt)
        {
            bool ok = true;
            if (txt.Contains("\r\n\r\n")) ok = false;
            if (txt.Contains("  ")) ok = false;
            if (txt.Contains("/")) ok = false;
            if (txt.StartsWith(" ") || txt.EndsWith(" ")) ok = false;
            return ok;
        }
    }
}
