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

        private void onPanelSizeChanged(object sender, EventArgs e)
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
            txtEntry.SelectionChanged += onTextSelectionChanged;
            txtEntry.HandleSpecialKey = onTextSpecialKey;
        }

        private void onTextChanged(object sender, EventArgs e)
        {
            //updateHints();
        }

        private void onTextSelectionChanged(object sender, EventArgs e)
        {
            updateHints();
        }

        public string StrSenses
        {
            get
            {
                string txt = txtEntry.Text.Replace("\r\n", "\n");
                return txtEntry.Text.Replace("\n", "/");
            }
            set
            {
                suppressHinting = true;
                txtEntry.Text = value.Replace("/", "\n");
                suppressHinting = false;
                lastText = txtEntry.Text;
                hints = new string[0];
                UpdateErrorBg();
            }
        }

        public void Clear()
        {
            StrSenses = "";
            clearHints();
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

        private Label makeLabel(string txt)
        {
            Label lbl = new Label();
            lbl.Margin = new Padding(0);
            lbl.Padding = new Padding(4, 0, 0, 0);
            lbl.TextAlign = ContentAlignment.TopLeft;
            lbl.Font = new Font("Segoe UI", 12F);
            lbl.AutoSize = false;
            lbl.Height = flowHints.Height;
            lbl.Text = txt;
            lbl.BackColor = Color.LightCyan;
            return lbl;
        }

        private string[] hints = new string[0];

        private string lastText = string.Empty;
        private int lastCaretPos = -1;
        private readonly List<Label> hintLabels = new List<Label>();
        private int activeHintIx = -1;
        private bool suppressHinting = false;

        private void updateHints()
        {
            if (lastCaretPos == -1 || suppressHinting || txtEntry.SelectionLength != 0)
            {
                clearHints();
                lastText = string.Empty;
                if (txtEntry.SelectionLength == 0) lastCaretPos = txtEntry.SelectionStart;
                else lastCaretPos = -1;
                return;
            }
            // If caret didn't move exactly one position, kill hinting
            int caretPos = txtEntry.SelectionStart;
            int caretPosDiff = caretPos - lastCaretPos;
            if (caretPosDiff != -1 && caretPosDiff != 1)
            {
                clearHints();
                lastText = txtEntry.Text.Substring(0, caretPos);
                lastCaretPos = caretPos;
                return;
            }

            // Typed/pasted exactly one character?
            // Backspaced exactly one character?
            string currTextStart = txtEntry.Text.Substring(0, txtEntry.SelectionStart);
            bool typedOne = lastText.Length == currTextStart.Length - 1 && currTextStart.StartsWith(lastText);
            bool deletedOne = lastText.Length == currTextStart.Length + 1 && lastText.StartsWith(currTextStart);
            bool rightOk = typedOne || deletedOne;
            if (rightOk)
            {
                char chr = ' ';
                if (currTextStart.Length < txtEntry.TextLength) chr = txtEntry.Text[currTextStart.Length];
                rightOk = chr == ' ' || chr == '\n' || char.IsPunctuation(chr);
            }
            if ((typedOne || deletedOne) && rightOk)
            {
                string prefix = getPrefix(currTextStart);
                List<string> hintsToShow = getHints(prefix);
                showHints(hintsToShow);
            }
            else clearHints();
            lastText = currTextStart;
            lastCaretPos = caretPos;
        }

        private string getPrefix(string txt)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = txt.Length - 1; i >= 0; --i)
            {
                char c = txt[i];
                if (char.IsLetterOrDigit(c)) sb.Insert(0, c);
                else break;
            }
            return sb.ToString();
        }

        private void clearHints()
        {
            foreach (Label lbl in hintLabels)
            {
                flowHints.Controls.Remove(lbl);
                lbl.MouseEnter -= onHintLabelMouseEnter;
                lbl.MouseLeave -= onHintLabelMouseLeave;
                lbl.MouseClick -= onHintLabelClick;
                lbl.Dispose();
            }
            hintLabels.Clear();
            activeHintIx = -1;
        }

        private void showHints(List<string> hintsToShow)
        {
            flowHints.SuspendLayout();
            List<Label> newHintLabels = new List<Label>(hintsToShow.Count);
            foreach (string str in hintsToShow)
            {
                Label hintLabel = null;
                foreach (Label lbl in hintLabels)
                {
                    if (lbl.Text == str)
                    {
                        hintLabel = lbl;
                        flowHints.Controls.Remove(hintLabel);
                        hintLabels.Remove(lbl);
                        break;
                    }
                }
                if (hintLabel == null)
                {
                    hintLabel = makeLabel(str);
                    hintLabel.MouseEnter += onHintLabelMouseEnter;
                    hintLabel.MouseLeave += onHintLabelMouseLeave;
                    hintLabel.Click += onHintLabelClick;
                }
                newHintLabels.Add(hintLabel);
            }
            clearHints();
            hintLabels.Clear();
            foreach (Label lbl in newHintLabels)
            {
                hintLabels.Add(lbl);
                flowHints.Controls.Add(lbl);
                lbl.Width = lbl.PreferredWidth;
            }
            setActiveHint(newHintLabels.Count > 0 ? 0 : -1);
            flowHints.ResumeLayout();
        }

        private void setActiveHint(int ix)
        {
            activeHintIx = ix;
            for (int i = 0; i != hintLabels.Count; ++i)
            {
                Label lbl = hintLabels[i];
                if (i == ix)
                {
                    lbl.BackColor = Color.DimGray;
                    lbl.ForeColor = SystemColors.Window;
                }
                else
                {
                    if (lbl.BackColor != Color.Gainsboro)
                    {
                        lbl.BackColor = SystemColors.Window;
                        lbl.ForeColor = SystemColors.WindowText;
                    }
                }
            }
        }

        private bool isActiveHint(Label lbl)
        {
            if (activeHintIx == -1) return false;
            else return hintLabels[activeHintIx] == lbl;
        }

        private void onHintLabelClick(object sender, EventArgs e)
        {
            Label lbl = sender as Label;
            pasteHint(lbl.Text);
        }

        private void onHintLabelMouseLeave(object sender, EventArgs e)
        {
            Label lbl = sender as Label;
            if (!isActiveHint(lbl))
            {
                lbl.BackColor = SystemColors.Window;
                lbl.ForeColor = SystemColors.WindowText;
            }
        }

        private void onHintLabelMouseEnter(object sender, EventArgs e)
        {
            Label lbl = sender as Label;
            if (!isActiveHint(lbl))
            {
                lbl.BackColor = Color.Gainsboro;
                lbl.ForeColor = SystemColors.WindowText;
            }
        }

        private void pasteHint(string txt)
        {
            string currTextStart = txtEntry.Text.Substring(0, txtEntry.SelectionStart);
            string prefix = getPrefix(currTextStart);
            txt = txt.Substring(prefix.Length);
            txtEntry.SelectedText = txt;
            clearHints();
        }

        private bool onTextSpecialKey(HintingTextBox.SpecialKeys sk)
        {
            if (hintLabels.Count == 0) return false;
            if (sk == HintingTextBox.SpecialKeys.Esc)
                clearHints();
            else if (sk == HintingTextBox.SpecialKeys.Left)
            {
                int newIx = activeHintIx - 1;
                if (newIx == -1) newIx = hintLabels.Count - 1;
                setActiveHint(newIx);
            }
            else if (sk == HintingTextBox.SpecialKeys.Right)
            {
                int newIx = activeHintIx + 1;
                if (newIx == hintLabels.Count) newIx = 0;
                setActiveHint(newIx);
            }
            else if (sk == HintingTextBox.SpecialKeys.Enter)
            {
                pasteHint(hintLabels[activeHintIx].Text);
            }
            else
                return false;
            return true;
        }
    }
}
