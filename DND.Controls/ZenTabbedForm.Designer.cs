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
            this.headerCtrl.Location = new System.Drawing.Point(0, 0);
            this.headerCtrl.Name = "headerCtrl";
            this.headerCtrl.TabIndex = 0;
            // 
            // pnlZenFrame
            // 
            this.pnlZenFrame.BackColor = System.Drawing.Color.Honeydew;
            this.pnlZenFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlZenFrame.Location = new System.Drawing.Point(0, 28);
            this.pnlZenFrame.Name = "pnlZenFrame";
            this.pnlZenFrame.Padding = new System.Windows.Forms.Padding(4);
            this.pnlZenFrame.Size = new System.Drawing.Size(600, 212);
            this.pnlZenFrame.TabIndex = 1;
            // 
            // ZenTabbedForm
            // 
            this.ClientSize = new System.Drawing.Size(600, 240);
            this.Controls.Add(this.pnlZenFrame);
            this.Controls.Add(this.headerCtrl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ZenTabbedForm";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ResumeLayout(false);
        }
    }
}
