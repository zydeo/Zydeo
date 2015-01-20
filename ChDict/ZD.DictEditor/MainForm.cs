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
        private bool loaded = false;
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
            pnlSplit.Panel1.SizeChanged += onCommandsSizeChanged;
            pnlSplit.SplitterMoved += onSplitterMoved;

            btnConfirm.Click += onButtonClick;
            btnMark.Click += onButtonClick;
            btnDrop.Click += onButtonClick;
            btnGoogleTrans.Click += onButtonClick;
            btnGoogleImg.Click += onButtonClick;

            txtFilter.TextChanged += onFilterChanged;
            cbEdMarked.CheckedChanged += onFilterChanged;
            llJump.Click += onJump;

            // Restore window size and position, unless designing
            if (Process.GetCurrentProcess().ProcessName == "devenv") return;
            // -- not designing
            Point loc = new Point(Settings.WindowX, Settings.WindowY);
            Size sz = new Size(Settings.WindowW, Settings.WindowH);
            if (loc.X > 0 && loc.Y > 0 && sz.Width > 0 && sz.Height > 0)
            {
                StartPosition = FormStartPosition.Manual;
                Location = loc;
                Size = sz;
            }
            else StartPosition = FormStartPosition.WindowsDefaultLocation;
            if (Settings.WindowStateMaximized)
                WindowState = FormWindowState.Maximized;
            pnlSplit.SplitterDistance = Settings.SplitterDist;
        }

        static MainForm()
        {
            htmlSkeleton = readHtmlSkeleton();
        }

        private void onSplitterMoved(object sender, SplitterEventArgs e)
        {
            if (!loaded) return;
            Settings.SplitterDist = pnlSplit.SplitterDistance;
        }

        private void onCommandsSizeChanged(object sender, EventArgs e)
        {
            int ofs = pnlSplit.Left - dgvHeads.Right;
            Size sz = pnlSplit.Panel1.ClientSize;
            pnlCommands.Location = new Point(0, sz.Height - pnlCommands.Height);
            pnlCommands.Width = sz.Width;

            btnConfirm.Width = pnlCommands.Width / 3;
            btnMark.Left = btnConfirm.Right;
            btnMark.Width = pnlCommands.Width / 3;
            btnDrop.Left = btnMark.Right;
            btnDrop.Width = pnlCommands.Width - btnDrop.Left;

            btnGoogleTrans.Width = pnlCommands.Width / 2;
            btnGoogleImg.Left = btnGoogleTrans.Right;
            btnGoogleImg.Width = pnlCommands.Width - btnGoogleImg.Left;

            editor.Location = Point.Empty;
            editor.Size = new Size(sz.Width, sz.Height - pnlCommands.Height - ofs);
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

            // Loaded!
            loaded = true;
            saveWindowSize();
            saveWindowLocation();
            doMoveToId(Settings.ActiveEntryId);
            doUpdateStatusBar();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            doSaveChanges(false, false);
        }

        private void saveWindowSize()
        {
            Settings.WindowW = Size.Width;
            Settings.WindowH = Size.Height;
            Settings.WindowStateMaximized = WindowState == FormWindowState.Maximized;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (!loaded) return;
            if (WindowState == FormWindowState.Normal) saveWindowSize();
            Settings.WindowStateMaximized = WindowState == FormWindowState.Maximized;
        }

        private void saveWindowLocation()
        {
            Settings.WindowX = Location.X;
            Settings.WindowY = Location.Y;
            Settings.WindowStateMaximized = WindowState == FormWindowState.Maximized;
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            if (!loaded) return;
            if (WindowState == FormWindowState.Normal) saveWindowLocation();
            Settings.WindowStateMaximized = WindowState == FormWindowState.Maximized;
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
            else if (keyData == (Keys.Control | Keys.G))
                onJump(null, null);
            else if (keyData == (Keys.Control | Keys.F))
                onFilterIf();
            else if (keyData == (Keys.Control | Keys.Up))
                doMove(-1);
            else if (keyData == (Keys.Control | Keys.Down))
                doMove(1);
            else
                return base.ProcessCmdKey(ref msg, keyData);
            return true;
        }

        private void onFilterChanged(object sender, EventArgs e)
        {
            bool txtFilterHadFocus = txtFilter.Focused;

            dd.Headwords.SetSimpFilter(txtFilter.Text);
            dd.Headwords.OnlyMarkedOrEdited = cbEdMarked.Checked;

            int ix = -1;
            if (activeHwd != null)
            {
                for (int i = 0; i != dd.Headwords.Count; ++i)
                    if (dd.Headwords[i].Id == activeHwd.Id) { ix = i; break; }
            }
            dgvHeads.DataSource = new BindingList<DictData.HwData>(dd.Headwords);
            if (ix >= 0) dgvHeads.Rows[ix].Selected = true;
            doScrollDGV();

            if (txtFilterHadFocus) txtFilter.Focus();
        }

        private void onFilterIf()
        {
            txtFilter.Text = txtHeadSimp.SelectedText;
        }

        private void onJump(object sender, EventArgs e)
        {
            if (dgvHeads.Rows.Count == 0) return;
            int start = 0;
            if (dgvHeads.SelectedRows.Count != 0) start = dgvHeads.SelectedRows[0].Index + 1;
            for (int i = start; i < dd.Headwords.Count; ++i)
            {
                if (dd.Headwords[i].Status == DictData.HwStatus.NotStarted)
                {
                    dgvHeads.Rows[i].Selected = true;
                    break;
                }
            }
            doScrollDGV();
        }

        private void onButtonClick(object sender, EventArgs e)
        {
            if (sender == btnConfirm) doConfirm();
            else if (sender == btnMark) doMark();
            else if (sender == btnDrop) doDrop();
            else if (sender == btnGoogleTrans) doLookup(LookupSource.GoogleMT);
            else if (sender == btnGoogleImg) doLookup(LookupSource.GoogleImg);
        }

        private void doScrollDGV()
        {
            if (dgvHeads.Rows.Count == 0) return;
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

        private void doMoveToId(int hwId)
        {
            if (dgvHeads.RowCount == 0) return;
            int rowIx = -1;
            for (int i = 0; i != dgvHeads.RowCount; ++i)
            {
                if ((dgvHeads.Rows[i].DataBoundItem as DictData.HwData).Id == hwId)
                { rowIx = i; break; }
            }
            if (rowIx == -1) return;
            dgvHeads.Rows[rowIx].Selected = true;
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

        private void doUpdateStatusBar()
        {
            int done, editedMarked, dropped, notStarted;
            dd.Headwords.GetStatCounts(out done, out editedMarked, out dropped, out notStarted);
            float donePercent = ((float)done) * 100F / ((float)(dd.Headwords.Count - dropped));
            tsLabelDoneVal.Text = done.ToString() + " (" + donePercent.ToString("0.0") + "%)";
            tsEditedMarkedVal.Text = editedMarked.ToString();
            tsDroppedVal.Text = dropped.ToString();
            tsRemainingVal.Text = notStarted.ToString();
        }

        private void doEnterNoEntry()
        {
            activeHwd = null;
            activeStrSenses = null;
            txtHeadSimp.Text = txtHeadTrad.Text = txtHeadPinyin.Text = "";
            editor.Clear();
        }

        private void doEnterEntry(DictData.HwData data)
        {
            activeHwd = data;
            activeStrSenses = dd.GetSenses(data.Id);
            editor.StrSenses = activeStrSenses;

            txtHeadSimp.Text = activeHwd.Simp;
            txtHeadTrad.Text = activeHwd.Trad;
            txtHeadTrad.ForeColor = activeHwd.Simp == activeHwd.Trad ? Color.DarkGray : SystemColors.WindowText;
            txtHeadPinyin.Text = PinyinDisplay.GetPinyinDisplay(activeHwd.Pinyin);

            BackboneEntry be = dd.GetBackbone(data.Id);
            doPrintBackbone(be);
            editor.SetVocabulary(be);
            editor.FillClassifierIfEmpty(be);

            if (loaded) Settings.ActiveEntryId = activeHwd.Id;
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
            doUpdateStatusBar();
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
                url = "https://www.google.com/search?q='" + activeHwd.Simp + "'+site:CN+OR+site:TW&tbm=isch&tbm=isch";
            Process.Start(url);
        }
    }
}
