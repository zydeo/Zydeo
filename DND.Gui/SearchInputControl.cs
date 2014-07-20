using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DND.Common;

namespace DND.Gui
{
    public partial class SearchInputControl : UserControl
    {
        public delegate void StartSearchDelegate(string text, SearchScript script, SearchLang lang);
        public event StartSearchDelegate StartSearch;

        private readonly float scale;
        private readonly int padding;
        private bool blockSizeChanged = false;

        public SearchInputControl(float scale)
        {
            this.scale = scale;
            padding = (int)Math.Round(4.0F * scale);
            InitializeComponent();
            txtInput.AutoSize = false;
            txtInput.Height = (int)(((float)txtInput.PreferredHeight) * 1.1F);
            blockSizeChanged = true;
            Height = 2 + txtInput.Height + 2 * padding;
            blockSizeChanged = false;
            txtInput.KeyPress += txtInput_KeyPress;
        }

        public void InsertCharacter(char c)
        {
            string str = ""; str += c;
            txtInput.SelectedText = str;
        }

        private void txtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                if (StartSearch != null)
                    StartSearch(txtInput.Text, SearchScript.Both, SearchLang.Chinese);
                e.Handled = true;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (blockSizeChanged) return;
            base.OnSizeChanged(e);
            txtInput.Location = new Point(padding, padding);
            txtInput.Size = new Size(ClientRectangle.Width - 2 * padding, ClientRectangle.Height - 2 * padding);
        }
    }
}
