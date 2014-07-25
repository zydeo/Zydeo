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
        public delegate void StartSearchDelegate(string text, SearchScript script, SearchLang lang);
        public event StartSearchDelegate StartSearch;

        private readonly TextBox txtInput;
        private readonly int padding;
        private Image imgSearch;
        private Image imgCancel;
        private bool blockSizeChanged = false;
        private bool isHover = false;

        public SearchInputControl(ZenControl owner)
            : base(owner)
        {
            txtInput = new TextBox();
            txtInput.Name = "txtInput";
            txtInput.BorderStyle = BorderStyle.None;
            txtInput.TabIndex = 0;
            RegisterWinFormsControl(txtInput);
            txtInput.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            txtInput.AutoSize = false;
            txtInput.Height = (int)(((float)txtInput.PreferredHeight) * 1.1F);

            padding = (int)Math.Round(4.0F * Scale);
            blockSizeChanged = true;
            Height = 2 + txtInput.Height + 2 * padding;
            blockSizeChanged = false;
            txtInput.KeyPress += txtInput_KeyPress;

            getResources();
            txtInput.MouseEnter += onTxtMouseEnter;
            txtInput.MouseLeave += onTxtMouseLeave;
        }

        public override void Dispose()
        {
            if (imgSearch != null) imgSearch.Dispose();
            if (imgCancel != null) imgCancel.Dispose();
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

        protected override void OnSizeChanged()
        {
            if (blockSizeChanged) return;

            // The height of the text box and icons
            int ctrlHeight = Height - 2 * padding;
            // Text field: search icon on left, X icon on right
            // Position must be in absolute (canvas) position, winforms controls' onwer is borderless form.
            txtInput.Location = new Point(AbsLeft + padding + ctrlHeight + padding, AbsTop + padding);
            txtInput.Size = new Size(Width - 4 * padding - 2 * ctrlHeight, ctrlHeight);
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

        public override void DoMouseEnter()
        {
            isHover = true;
            MakeMePaint(false, RenderMode.Invalidate);
        }

        public override void DoMouseLeave()
        {
            isHover = false;
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
            if (isHover) return;
            isHover = true;
            MakeMePaint(false, RenderMode.Invalidate);
        }

        public override bool DoMouseClick(Point p, MouseButtons button)
        {
            int ctrlHeight = Height - 2 * padding;
            Rectangle rectImgSearch = new Rectangle(padding, padding, ctrlHeight, ctrlHeight);
            if (rectImgSearch.Contains(p)) doStartSearch();
            Rectangle rectImgCancel = new Rectangle(Width - padding - ctrlHeight, padding, ctrlHeight, ctrlHeight);
            if (rectImgCancel.Contains(p)) txtInput.Text = "";
            return true;
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
            // Paint my icons
            int ctrlHeight = Height - 2 * padding;
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
