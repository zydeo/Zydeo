namespace ZD.DictEditor
{
    partial class EntryEditor
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
            this.txtEntry = new System.Windows.Forms.TextBox();
            this.pnlFrame = new System.Windows.Forms.Panel();
            this.flowHints = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlFrame.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtEntry
            // 
            this.txtEntry.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtEntry.Location = new System.Drawing.Point(4, 4);
            this.txtEntry.Multiline = true;
            this.txtEntry.Name = "txtEntry";
            this.txtEntry.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtEntry.Size = new System.Drawing.Size(516, 152);
            this.txtEntry.TabIndex = 0;
            // 
            // pnlFrame
            // 
            this.pnlFrame.BackColor = System.Drawing.Color.DarkGray;
            this.pnlFrame.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlFrame.Controls.Add(this.flowHints);
            this.pnlFrame.Controls.Add(this.txtEntry);
            this.pnlFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFrame.Location = new System.Drawing.Point(0, 0);
            this.pnlFrame.Name = "pnlFrame";
            this.pnlFrame.Size = new System.Drawing.Size(532, 220);
            this.pnlFrame.TabIndex = 2;
            // 
            // flowHints
            // 
            this.flowHints.BackColor = System.Drawing.SystemColors.Window;
            this.flowHints.Location = new System.Drawing.Point(4, 160);
            this.flowHints.Name = "flowHints";
            this.flowHints.Size = new System.Drawing.Size(516, 48);
            this.flowHints.TabIndex = 1;
            // 
            // EntryEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.pnlFrame);
            this.Name = "EntryEditor";
            this.Size = new System.Drawing.Size(532, 220);
            this.pnlFrame.ResumeLayout(false);
            this.pnlFrame.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtEntry;
        private System.Windows.Forms.Panel pnlFrame;
        private System.Windows.Forms.FlowLayoutPanel flowHints;
    }
}
