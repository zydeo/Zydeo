using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

using ZD.ChDict.Common;

namespace ZD.DictEditor
{
    public partial class MainForm : Form
    {
        private DictData dd;
        private DateTime dtEditStart;
        private DictData.HwData activeHwd = null;
        private string activeStrSenses = null;

        public MainForm()
        {
            InitializeComponent();

            dgvHeads.AutoGenerateColumns = false;
            CustomColumn cc = new CustomColumn();
            cc.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvHeads.Columns.Add(cc);
            dgvHeads.RowTemplate.Height = CustomCell.CellHeight;

            txtHeadSimp.BackColor = SystemColors.Window;
            txtHeadTrad.BackColor = SystemColors.Window;
            txtHeadPinyin.BackColor = SystemColors.Window;
            pnlCommands.SizeChanged += onCommandsSizeChanged;

            btnConfirm.Click += onButtonClick;
            btnMark.Click += onButtonClick;
            btnDrop.Click += onButtonClick;
            btnGoogleTrans.Click += onButtonClick;
            btnGoogleImg.Click += onButtonClick;
        }

        private void onCommandsSizeChanged(object sender, EventArgs e)
        {
            btnConfirm.Width = pnlCommands.Width / 3;
            btnMark.Left = btnConfirm.Right;
            btnMark.Width = pnlCommands.Width / 3;
            btnDrop.Left = btnMark.Right;
            btnDrop.Width = pnlCommands.Width - btnDrop.Left;

            btnGoogleTrans.Width = pnlCommands.Width / 2;
            btnGoogleImg.Left = btnGoogleTrans.Right;
            btnGoogleImg.Width = pnlCommands.Width - btnGoogleImg.Left;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            MinimumSize = new Size(3 * dgvHeads.Width, 2 * editor.Height);
            onCommandsSizeChanged(null, null);

            // Initialize from DB, or parse full backbone XML and create DB
            if (File.Exists("chdict.sqlite"))
                dd = DictData.InitFromDB("chdict.sqlite");
            else
                dd = DictData.InitFromXml("backbone.xml", "chdict.sqlite");

            // Time logging
            dtEditStart = DateTime.Now;

            dgvHeads.DataSource = new BindingList<DictData.HwData>(dd.Headwords);
            dgvHeads.SelectionChanged += onHwSelectionChanged;
            onHwSelectionChanged(null, null);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            doSaveChanges(false, false);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Enter))
                doConfirm();
            else if (keyData == (Keys.Control | Keys.D))
                doDrop();
            else if (keyData == (Keys.Control | Keys.M))
                doMark();
            else if (keyData == (Keys.Control | Keys.T))
                doLookup(LookupSource.GoogleMT);
            else if (keyData == (Keys.Control | Keys.I))
                doLookup(LookupSource.GoogleImg);
            else if (keyData == (Keys.Control | Keys.Up))
                doMove(-1);
            else if (keyData == (Keys.Control | Keys.Down))
                doMove(1);
            else
                return base.ProcessCmdKey(ref msg, keyData);
            return true;
        }

        private void onButtonClick(object sender, EventArgs e)
        {
            if (sender == btnConfirm) doConfirm();
            else if (sender == btnMark) doMark();
            else if (sender == btnDrop) doDrop();
            else if (sender == btnGoogleTrans) doLookup(LookupSource.GoogleMT);
            else if (sender == btnGoogleImg) doLookup(LookupSource.GoogleImg);
        }

        private void doEnterNoEntry()
        {
            activeHwd = null;
            activeStrSenses = null;
        }

        private void doScrollDGV()
        {
            int halfWay = (dgvHeads.DisplayedRowCount(false) / 2);
            if (dgvHeads.FirstDisplayedScrollingRowIndex + halfWay > dgvHeads.SelectedRows[0].Index ||
                (dgvHeads.FirstDisplayedScrollingRowIndex + dgvHeads.DisplayedRowCount(false) - halfWay) <= dgvHeads.SelectedRows[0].Index)
            {
                int targetRow = dgvHeads.SelectedRows[0].Index;
                targetRow = Math.Max(targetRow - halfWay, 0);
                dgvHeads.FirstDisplayedScrollingRowIndex = targetRow;
            }
        }

        private void doMove(int diff)
        {
            int currRoxIx = -1;
            if (dgvHeads.SelectedRows.Count == 1) currRoxIx = dgvHeads.SelectedRows[0].Index;
            if (currRoxIx == -1) return;
            int newRoxIx = currRoxIx + diff;
            if (newRoxIx >= 0 && newRoxIx < dgvHeads.Rows.Count)
            {
                blockSelectionChanged = true;
                dgvHeads.Rows[currRoxIx].Selected = false;
                blockSelectionChanged = false;
                dgvHeads.Rows[newRoxIx].Selected = true;
            }
            doScrollDGV();
        }

        private void doMoveToNext()
        {
            int currRoxIx = -1;
            if (dgvHeads.SelectedRows.Count == 1) currRoxIx = dgvHeads.SelectedRows[0].Index;
            if (currRoxIx != -1 && currRoxIx < dgvHeads.RowCount - 1)
            {
                blockSelectionChanged = true;
                dgvHeads.Rows[currRoxIx].Selected = false;
                ++currRoxIx;
                dgvHeads.Rows[currRoxIx].Selected = true;
                blockSelectionChanged = false;
                DictData.HwData data = dd.Headwords[dgvHeads.SelectedRows[0].Index];
                doEnterEntry(data);
                editor.Focus();
            }
            doScrollDGV();
        }

        private void doConfirm()
        {
            bool ok = doSaveChanges(true, false);
            if (ok) doMoveToNext();
            else editor.UpdateErrorBg();
            dgvHeads.Refresh();
        }

        private void doMark()
        {
            activeHwd.Status = DictData.HwStatus.Marked;
            doSaveChanges(false, true);
            dgvHeads.Refresh();
        }

        private void doDrop()
        {
            activeHwd.Status = DictData.HwStatus.Dropped;
            doSaveChanges(false, true);
            doMoveToNext();
            dgvHeads.Refresh();
        }

        private void doEnterEntry(DictData.HwData data)
        {
            activeHwd = data;
            activeStrSenses = dd.GetSenses(data.Id);
            editor.StrSenses = activeStrSenses;

            txtHeadSimp.Text = activeHwd.Simp;
            txtHeadTrad.Text = activeHwd.Trad;
            txtHeadTrad.ForeColor = activeHwd.Simp == activeHwd.Trad ? Color.DarkGray : SystemColors.WindowText;
            txtHeadPinyin.Text = activeHwd.Pinyin;

            string[] hints = new string[] { "kutya", "kuka", "kukásautó", "karambolprimadonna", "kaka" };
            editor.TypingHints = hints;

            BackboneEntry be = dd.GetBackbone(data.Id);
            printBackbone(be);
        }

        /// <summary>
        /// Saves changes. Returns true if there are no errors in current data.
        /// </summary>
        private bool doSaveChanges(bool confirmIfPossible, bool forceSave)
        {
            string newStrSenses = editor.StrSenses;
            bool hasErrors = editor.HasErrors;
            // Save if text actually changed, or if forced to.
            if (forceSave || activeStrSenses != newStrSenses || (confirmIfPossible && activeHwd.Status != DictData.HwStatus.Done))
            {
                DictData.HwStatus status = activeHwd.Status;
                if (hasErrors && activeHwd.Status != DictData.HwStatus.Dropped) status = DictData.HwStatus.Marked;
                else
                {
                    if (!confirmIfPossible)
                    {
                        if (status != DictData.HwStatus.Marked && activeHwd.Status != DictData.HwStatus.Dropped)
                            status = DictData.HwStatus.Edited;
                    }
                    else status = DictData.HwStatus.Done;
                }
                // Time logging
                DateTime dtNow = DateTime.Now;
                dd.SaveSenses(activeHwd.Id, newStrSenses, status, dtEditStart, dtNow);
                dtEditStart = dtNow;
            }
            return !hasErrors;
        }

        private bool blockSelectionChanged = false;

        private void onHwSelectionChanged(object sender, EventArgs e)
        {
            if (blockSelectionChanged) return;

            if (activeHwd != null) doSaveChanges(false, false);
            if (dgvHeads.SelectedRows.Count == 0) doEnterNoEntry();
            else
            {
                DictData.HwData data = dd.Headwords[dgvHeads.SelectedRows[0].Index];
                doEnterEntry(data);
                editor.Focus();
            }
        }

        private enum LookupSource
        {
            GoogleMT,
            GoogleImg,
        }

        private void doLookup(LookupSource ls)
        {
            string url;
            if (ls == LookupSource.GoogleMT)
                url = "https://translate.google.com/#zh-CN/hu/" + activeHwd.Simp;
            else
                url = "https://www.google.com/search?q=" + activeHwd.Simp  + "&tbm=isch";
            Process.Start(url);
        }
    }
}
