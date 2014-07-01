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
            this.pnlLeftRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlLeftRight
            // 
            this.pnlLeftRight.ColumnCount = 2;
            this.pnlLeftRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 306F));
            this.pnlLeftRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlLeftRight.Controls.Add(this.writingPad1, 0, 0);
            this.pnlLeftRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeftRight.Location = new System.Drawing.Point(0, 0);
            this.pnlLeftRight.Margin = new System.Windows.Forms.Padding(0);
            this.pnlLeftRight.Name = "pnlLeftRight";
            this.pnlLeftRight.RowCount = 1;
            this.pnlLeftRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlLeftRight.Size = new System.Drawing.Size(876, 314);
            this.pnlLeftRight.TabIndex = 0;
            // 
            // writingPad1
            // 
            this.writingPad1.Location = new System.Drawing.Point(0, 0);
            this.writingPad1.Margin = new System.Windows.Forms.Padding(0);
            this.writingPad1.Name = "writingPad1";
            this.writingPad1.Size = new System.Drawing.Size(306, 312);
            this.writingPad1.TabIndex = 0;
            this.writingPad1.Text = "writingPad1";
            // 
            // LookupControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pnlLeftRight);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "LookupControl";
            this.Size = new System.Drawing.Size(876, 314);
            this.pnlLeftRight.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel pnlLeftRight;
        private DND.Controls.WritingPad writingPad1;
    }
}
