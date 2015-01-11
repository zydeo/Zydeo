using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using ZD.ChDict.Common;

namespace ZD.DictEditor
{
    public partial class MainForm : Form
    {
        private DictData dd;
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
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            MinimumSize = new Size(3 * dgvHeads.Width, 2 * editor.Height);
            // Initialize from DB, or parse full backbone XML and create DB
            if (File.Exists("chdict.sqlite"))
                dd = DictData.InitFromDB("chdict.sqlite");
            else
                dd = DictData.InitFromXml("backbone.xml", "chdict.sqlite");
            dgvHeads.DataSource = new BindingList<DictData.HwData>(dd.Headwords);
            dgvHeads.SelectionChanged += onHwSelectionChanged;
            onHwSelectionChanged(null, null);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Enter))
                doConfirm();
            else if (keyData == (Keys.Control | Keys.Up))
                doMove(-1);
            else if (keyData == (Keys.Control | Keys.Down))
                doMove(1);
            else
                return base.ProcessCmdKey(ref msg, keyData);
            return true;
        }

        private void doEnterNoEntry()
        {
            activeHwd = null;
            activeStrSenses = null;
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
        }

        private void doConfirm()
        {
            bool ok = doSaveChanges(true);
            int currRoxIx = -1;
            if (dgvHeads.SelectedRows.Count == 1) currRoxIx = dgvHeads.SelectedRows[0].Index;
            if (ok)
            {
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
            }
            else editor.UpdateErrorBg();
            dgvHeads.Refresh();
        }

        private void doEnterEntry(DictData.HwData data)
        {
            activeHwd = data;
            activeStrSenses = dd.GetSenses(data.Id);
            editor.StrSenses = activeStrSenses;

            string[] hints = new string[] { "kutya", "kuka", "kukásautó", "karambolprimadonna", "kaka" };
            editor.TypingHints = hints;

            BackboneEntry be = dd.GetBackbone(data.Id);
            printBackbone(be);
        }

        /// <summary>
        /// Saves changes. Returns true if there are no errors in current data.
        /// </summary>
        private bool doSaveChanges(bool confirmIfPossible)
        {
            string newStrSenses = editor.StrSenses;
            bool hasErrors = editor.HasErrors;
            // Save if text actually changed.
            if (activeStrSenses != newStrSenses || (confirmIfPossible && activeHwd.Status != DictData.HwStatus.Done))
            {
                DictData.HwStatus status = activeHwd.Status;
                if (hasErrors) status = DictData.HwStatus.Marked;
                else
                {
                    if (!confirmIfPossible)
                    {
                        if (status != DictData.HwStatus.Marked) status = DictData.HwStatus.Edited;
                    }
                    else status = DictData.HwStatus.Done;
                }
                dd.SaveSenses(activeHwd.Id, newStrSenses, status);
            }
            return !hasErrors;
        }

        private bool blockSelectionChanged = false;

        private void onHwSelectionChanged(object sender, EventArgs e)
        {
            if (blockSelectionChanged) return;

            if (activeHwd != null) doSaveChanges(false);
            if (dgvHeads.SelectedRows.Count == 0) doEnterNoEntry();
            else
            {
                DictData.HwData data = dd.Headwords[dgvHeads.SelectedRows[0].Index];
                doEnterEntry(data);
                editor.Focus();
            }
        }
    }
}
