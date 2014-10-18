using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;

using DND.Common;
using DND.Gui.Zen;

namespace DND.Gui
{
    internal class SearchInputControl : ZenControl
    {
        public delegate void StartSearchDelegate(object sender, string text);
        public event StartSearchDelegate StartSearch;

        private readonly TextBox txtInput;
        private readonly int padding;
        private readonly ZenImageButton btnSearch;
        private readonly ZenImageButton btnCancel;
        private bool blockSizeChanged = false;

        public SearchInputControl(ZenControl owner)
            : base(owner)
        {
            padding = (int)Math.Round(4.0F * Scale);

            txtInput = new TextBox();
            txtInput.Name = "txtInput";
            txtInput.BorderStyle = BorderStyle.None;
            txtInput.TabIndex = 0;
            RegisterWinFormsControl(txtInput);
            string fface = Magic.ZhoButtonFontFamily;
            txtInput.Font = new System.Drawing.Font(fface, 16F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            txtInput.AutoSize = false;
            txtInput.Height = txtInput.PreferredHeight + padding;
            //txtInput.BackColor = Color.Magenta;

            blockSizeChanged = true;
            Height = 2 + txtInput.Height;
            blockSizeChanged = false;
            txtInput.KeyPress += txtInput_KeyPress;

            Assembly a = Assembly.GetExecutingAssembly();
            var imgSearch = Image.FromStream(a.GetManifestResourceStream("DND.Gui.Resources.search.png"));
            btnSearch = new ZenImageButton(this);
            btnSearch.RelLocation = new Point(padding, padding);
            btnSearch.Size = new Size(Height - 2 * padding, Height - 2 * padding);
            btnSearch.Image = imgSearch;
            btnSearch.MouseClick += onClickSearch;

            var imgCancel = Image.FromStream(a.GetManifestResourceStream("DND.Gui.Resources.cancel.png"));
            btnCancel = new ZenImageButton(this);
            btnCancel.Size = new Size(Height - 2 * padding, Height - 2 * padding);
            btnCancel.RelLocation = new Point(Width - padding - btnCancel.Width, padding);
            btnCancel.Image = imgCancel;
            btnCancel.Visible = false;
            btnCancel.MouseClick += onClickCancel;

            txtInput.MouseEnter += onTxtMouseEnter;
            txtInput.MouseLeave += onTxtMouseLeave;
        }

        protected override void OnSizeChanged()
        {
            if (blockSizeChanged) return;

            // The height of the text box and icons
            int ctrlHeight = Height - 2 * padding;
            // Position and height below is suitable for Noto, but not for Segoe.
            //int textBoxHeight = Height - 2 * padding;
            //int textBoxTop = AbsTop + padding;
            int textBoxHeight = Height - padding;
            int textBoxTop = AbsTop + 1;
            // Text field: search icon on left, X icon on right
            // Position must be in absolute (canvas) position, winforms controls' onwer is borderless form.
            // Position below is suitable for Noto, but not for Segoe.
            //txtInput.Location = new Point(AbsLeft + padding + ctrlHeight + padding, AbsTop + padding);
            txtInput.Location = new Point(AbsLeft + padding + ctrlHeight + padding, AbsTop + 1);
            txtInput.Size = new Size(Width - 4 * padding - 2 * ctrlHeight, textBoxHeight);

            // Cancel button: right-aligned
            btnCancel.RelLocation = new Point(Width - padding - btnCancel.Width, 2 * padding / 3);
        }

        public override void Dispose()
        {
            base.Dispose();
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

        public string Text
        {
            get { return txtInput.Text; }
            set { txtInput.Text = value; }
        }

        private void doStartSearch()
        {
            if (StartSearch != null)
                StartSearch(this, txtInput.Text);
        }

        private bool isCancelVisible()
        {
            Point p = MousePosition;
            Rectangle rect = new Rectangle(txtInput.Left, 1, Width - txtInput.Left - 1, Height - 2);
            return rect.Contains(p);
        }

        public override bool DoMouseMove(Point p, MouseButtons button)
        {
            base.DoMouseMove(p, button);
            btnCancel.Visible = isCancelVisible();
            return true;
        }

        public override void DoMouseEnter()
        {
            base.DoMouseEnter();
            btnCancel.Visible = isCancelVisible();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        public override void DoMouseLeave()
        {
            base.DoMouseLeave();
            btnCancel.Visible = false;
            MakeMePaint(false, RenderMode.Invalidate);
        }

        private void onTxtMouseLeave(object sender, EventArgs e)
        {
            // Pointer may lease text box but still be inside me
            if (sender == txtInput)
            {
                Point p = MousePositionAbs;
                if (AbsRect.Contains(p)) return;
            }
            DoMouseLeave();
        }

        private void onTxtMouseEnter(object sender, EventArgs e)
        {
            if (btnCancel.Visible) return;
            btnCancel.Visible = true;
            MakeMePaint(false, RenderMode.Invalidate);
        }

        private void onClickSearch(ZenControlBase sender)
        {
            doStartSearch();
        }

        private void onClickCancel(ZenControlBase sender)
        {
            txtInput.Text = "";
        }

        public override void DoPaint(Graphics g)
        {
            // Paint my BG
            using (SolidBrush b = new SolidBrush(Color.White))
            {
                g.FillRectangle(b, new Rectangle(0, 0, Width, Height));
            }
            // Draw my border
            using (Pen p = new Pen(ZenParams.BorderColor))
            {
                g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
            }
            // Children! My buttons.
            DoPaintChildren(g);
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
