using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DND.Controls
{
    partial class ZenTabbedForm
    {
        private void InitializeComponent()
        {
            this.headerCtrl = new DND.Controls.ZenFormHeader();
            this.pnlZenFrame = new DND.Controls.ZenContentPanel();
            this.SuspendLayout();
            // 
            // headerCtrl
            // 
            this.headerCtrl.BackColor = System.Drawing.Color.PaleGreen;
            this.headerCtrl.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerCtrl.HeaderText = "Header";
            this.headerCtrl.Location = new System.Drawing.Point(5, 5);
            this.headerCtrl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.headerCtrl.Name = "headerCtrl";
            this.headerCtrl.Size = new System.Drawing.Size(890, 43);
            this.headerCtrl.TabIndex = 0;
            // 
            // pnlZenFrame
            // 
            this.pnlZenFrame.BackColor = System.Drawing.Color.Honeydew;
            this.pnlZenFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlZenFrame.Location = new System.Drawing.Point(5, 48);
            this.pnlZenFrame.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.pnlZenFrame.Name = "pnlZenFrame";
            this.pnlZenFrame.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.pnlZenFrame.Size = new System.Drawing.Size(890, 316);
            this.pnlZenFrame.TabIndex = 1;
            // 
            // ZenTabbedForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Magenta;
            this.ClientSize = new System.Drawing.Size(900, 369);
            this.Controls.Add(this.pnlZenFrame);
            this.Controls.Add(this.headerCtrl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "ZenTabbedForm";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.TransparencyKey = System.Drawing.Color.Magenta;
            this.ResumeLayout(false);

        }
    }
}
