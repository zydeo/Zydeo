using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

using DND.Common;

namespace DND.Gui
{
    public partial class SearchInputControl : UserControl
    {
        public delegate void StartSearchDelegate(string text, SearchScript script, SearchLang lang);
        public event StartSearchDelegate StartSearch;

        private readonly float scale;
        private readonly int padding;
        private Image imgSearch;
        private Image imgCancel;
        private bool blockSizeChanged = false;
        private bool isHover = false;

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

            getResources();
            pnlBg.Paint += onPnlPaint;
            pnlBg.Click += onPnlClick;
            pnlBg.MouseEnter += onMouseEnter;
            txtInput.MouseEnter += onMouseEnter;
            pnlBg.MouseLeave += onMouseLeave;
            txtInput.MouseLeave += onMouseLeave;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();
                if (imgSearch != null) imgSearch.Dispose();
                if (imgCancel != null) imgCancel.Dispose();
            }
            base.Dispose(disposing);
        }

        public void InsertCharacter(char c)
        {
            string str = ""; str += c;
            txtInput.SelectedText = str;
        }

        public void SelectAll()
        {
            txtInput.SelectAll();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (blockSizeChanged) return;
            base.OnSizeChanged(e);

            // The height of the text box and icons
            int ctrlHeight = ClientRectangle.Height - 2 * padding;
            // Text field: search icon on left, X icon on right
            txtInput.Location = new Point(padding + ctrlHeight + padding, padding);
            txtInput.Size = new Size(ClientRectangle.Width - 4 * padding - 2 * ctrlHeight, ctrlHeight);
        }

        private void doStartSearch()
        {
            if (StartSearch != null)
                StartSearch(txtInput.Text, SearchScript.Both, SearchLang.Chinese);
        }

        private void getResources()
        {
            Assembly a = Assembly.GetExecutingAssembly();
            imgSearch = Image.FromStream(a.GetManifestResourceStream("DND.Gui.Resources.search.png"));
            imgCancel = Image.FromStream(a.GetManifestResourceStream("DND.Gui.Resources.cancel.png"));
        }

        private void onMouseLeave(object sender, EventArgs e)
        {
            if (sender == txtInput)
            {
                Point p = MousePosition;
                p = PointToClient(p);
                if (pnlBg.ClientRectangle.Contains(p)) return;
            }
            if (sender == pnlBg)
            {
                Point p = MousePosition;
                p = PointToClient(p);
                if (txtInput.ClientRectangle.Contains(p)) return;
            }
            isHover = false;
            pnlBg.Invalidate();
        }

        private void onMouseEnter(object sender, EventArgs e)
        {
            if (isHover) return;
            isHover = true;
            pnlBg.Invalidate();
        }

        private void onPnlClick(object sender, EventArgs e)
        {
            Point p = MousePosition;
            p = PointToClient(p);
            int ctrlHeight = ClientRectangle.Height - 2 * padding;
            Rectangle rectImgSearch = new Rectangle(padding, padding, ctrlHeight, ctrlHeight);
            if (rectImgSearch.Contains(p)) doStartSearch();
            Rectangle rectImgCancel = new Rectangle(Width - padding - ctrlHeight, padding, ctrlHeight, ctrlHeight);
            if (rectImgCancel.Contains(p)) txtInput.Text = "";
        }

        private void onPnlPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            // Paint my icons
            int ctrlHeight = ClientRectangle.Height - 2 * padding;
            Rectangle rectImgSearch = new Rectangle(padding, padding, ctrlHeight, ctrlHeight);
            g.DrawImage(imgSearch, rectImgSearch);
            if (isHover)
            {
                Rectangle rectImgCancel = new Rectangle(Width - padding - ctrlHeight, padding, ctrlHeight, ctrlHeight);
                g.DrawImage(imgCancel, rectImgCancel);
                using (Brush b = new SolidBrush(Color.FromArgb(192, Color.White)))
                {
                    g.FillRectangle(b, rectImgCancel);
                }
            }
        }

        private void txtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                doStartSearch();
                e.Handled = true;
            }
        }
    }
}
