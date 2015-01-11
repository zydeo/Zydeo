using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace ZD.DictEditor
{
    partial class MainForm
    {
        public class CustomCell : DataGridViewCell
        {
            public override Type ValueType
            {
                get { return typeof(object); }
            }

            private static HwCtrl ctrl = new HwCtrl();

            public static int CellHeight
            {
                get { return ctrl.Height; }
            }

            protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
            {
                var img = new Bitmap(cellBounds.Width, cellBounds.Height);
                ctrl.Size = img.Size;
                var data = DataGridView.DataSource as BindingList<DictData.HwData>;
				ctrl.Data = data[rowIndex];
                if (DataGridView.SelectedRows.Count == 1 && DataGridView.SelectedRows[0].Index == rowIndex) ctrl.Selected = true;
                else ctrl.Selected = false;
                ctrl.DrawToBitmap(img, new Rectangle(0, 0, ctrl.Width, ctrl.Height));
                graphics.DrawImage(img, cellBounds.Location);
            }

            protected override void OnClick(DataGridViewCellEventArgs e)
            {
                base.OnClick(e);
            }
        }

        public class CustomColumn : DataGridViewColumn
        {
            public CustomColumn() : base(new CustomCell()) { }

            public override DataGridViewCell CellTemplate
            {
                get { return base.CellTemplate; }
                set { base.CellTemplate = value; }
            }
        }
    }
}
