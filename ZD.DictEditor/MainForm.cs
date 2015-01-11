using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ZD.DictEditor
{
    public partial class MainForm : Form
    {
        private DictData dd;

        public MainForm()
        {
            InitializeComponent();

            dgvHeads.AutoGenerateColumns = false;
            CustomColumn cc = new CustomColumn();
            cc.DataPropertyName = "Data";
            cc.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvHeads.Columns.Add(cc);
            dgvHeads.RowTemplate.Height = CustomCell.CellHeight;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            MinimumSize = new Size(3 * dgvHeads.Width, 2 * editor.Height);
            dd = DictData.InitFromXml("backbone.xml");
            dgvHeads.DataSource = new BindingList<DictData.HwBoundData>(dd.Headwords);
            dgvHeads.SelectionChanged += onHwSelectionChanged;
        }

        void onHwSelectionChanged(object sender, EventArgs e)
        {
            int iii = 0;
        }
    }
}
