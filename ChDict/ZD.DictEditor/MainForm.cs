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

        private void doEnterNoEntry()
        {
            activeHwd = null;
            activeStrSenses = null;
        }

        private void doEnterEntry(DictData.HwData data)
        {
            activeHwd = data;
            activeStrSenses = dd.GetSenses(data.Id);
            editor.StrSenses = activeStrSenses;
            BackboneEntry be = dd.GetBackbone(data.Id);
            printBackbone(be);
        }

        private void doSaveChanges()
        {
            string newStrSenses = editor.StrSenses;
            // Save if text actually changed.
            if (activeStrSenses != newStrSenses)
            {
                DictData.HwStatus status = activeHwd.Status;
                if (editor.HasErrors) status = DictData.HwStatus.Marked;
                else if (status != DictData.HwStatus.Marked) status = DictData.HwStatus.Edited;
                dd.SaveSenses(activeHwd.Id, newStrSenses, status);
            }
        }

        private void onHwSelectionChanged(object sender, EventArgs e)
        {
            if (activeHwd != null) doSaveChanges();
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
