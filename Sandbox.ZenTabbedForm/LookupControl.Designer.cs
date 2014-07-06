namespace Sandbox
{
    partial class LookupControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlLeftRight = new System.Windows.Forms.TableLayoutPanel();
            this.writingPad1 = new DND.Controls.WritingPad();
            this.pnlLookup = new System.Windows.Forms.TableLayoutPanel();
            this.siCtrl = new DND.Controls.SearchInputControl();
            this.resultsCtrl = new DND.Controls.ResultsControl();
            this.pnlLeftRight.SuspendLayout();
            this.pnlLookup.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlLeftRight
            // 
            this.pnlLeftRight.ColumnCount = 2;
            this.pnlLeftRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 204F));
            this.pnlLeftRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlLeftRight.Controls.Add(this.writingPad1, 0, 0);
            this.pnlLeftRight.Controls.Add(this.pnlLookup, 1, 0);
            this.pnlLeftRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeftRight.Location = new System.Drawing.Point(4, 4);
            this.pnlLeftRight.Margin = new System.Windows.Forms.Padding(0);
            this.pnlLeftRight.Name = "pnlLeftRight";
            this.pnlLeftRight.RowCount = 1;
            this.pnlLeftRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlLeftRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pnlLeftRight.Size = new System.Drawing.Size(576, 196);
            this.pnlLeftRight.TabIndex = 0;
            // 
            // writingPad1
            // 
            this.writingPad1.Location = new System.Drawing.Point(0, 0);
            this.writingPad1.Margin = new System.Windows.Forms.Padding(0);
            this.writingPad1.Name = "writingPad1";
            this.writingPad1.Size = new System.Drawing.Size(204, 196);
            this.writingPad1.TabIndex = 0;
            this.writingPad1.Text = "writingPad1";
            // 
            // pnlLookup
            // 
            this.pnlLookup.ColumnCount = 1;
            this.pnlLookup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlLookup.Controls.Add(this.siCtrl, 0, 0);
            this.pnlLookup.Controls.Add(this.resultsCtrl, 0, 1);
            this.pnlLookup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLookup.Location = new System.Drawing.Point(208, 0);
            this.pnlLookup.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.pnlLookup.Name = "pnlLookup";
            this.pnlLookup.RowCount = 2;
            this.pnlLookup.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlLookup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlLookup.Size = new System.Drawing.Size(368, 196);
            this.pnlLookup.TabIndex = 1;
            // 
            // siCtrl
            // 
            this.siCtrl.BackColor = System.Drawing.Color.White;
            this.siCtrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.siCtrl.Dock = System.Windows.Forms.DockStyle.Top;
            this.siCtrl.Location = new System.Drawing.Point(0, 0);
            this.siCtrl.Margin = new System.Windows.Forms.Padding(0);
            this.siCtrl.Name = "siCtrl";
            this.siCtrl.Size = new System.Drawing.Size(368, 34);
            this.siCtrl.TabIndex = 0;
            // 
            // resultsCtrl
            // 
            this.resultsCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultsCtrl.Location = new System.Drawing.Point(0, 38);
            this.resultsCtrl.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.resultsCtrl.Name = "resultsCtrl";
            this.resultsCtrl.Size = new System.Drawing.Size(368, 158);
            this.resultsCtrl.TabIndex = 1;
            this.resultsCtrl.Text = "resultsControl1";
            // 
            // LookupControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pnlLeftRight);
            this.Name = "LookupControl";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.Size = new System.Drawing.Size(584, 204);
            this.pnlLeftRight.ResumeLayout(false);
            this.pnlLookup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel pnlLeftRight;
        private DND.Controls.WritingPad writingPad1;
        private System.Windows.Forms.TableLayoutPanel pnlLookup;
        private DND.Controls.SearchInputControl siCtrl;
        private DND.Controls.ResultsControl resultsCtrl;
    }
}
