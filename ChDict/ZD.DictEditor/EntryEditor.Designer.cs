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
            this.txtEntry = new ZD.DictEditor.HintingTextBox();
            this.pnlFrame = new System.Windows.Forms.Panel();
            this.flowHints = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlEditorBg = new System.Windows.Forms.Panel();
            this.pnlSeparator = new System.Windows.Forms.Panel();
            this.pnlFrame.SuspendLayout();
            this.pnlEditorBg.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtEntry
            // 
            this.txtEntry.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtEntry.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEntry.Location = new System.Drawing.Point(8, 8);
            this.txtEntry.Multiline = true;
            this.txtEntry.Name = "txtEntry";
            this.txtEntry.Size = new System.Drawing.Size(516, 148);
            this.txtEntry.TabIndex = 0;
            // 
            // pnlFrame
            // 
            this.pnlFrame.BackColor = System.Drawing.SystemColors.Window;
            this.pnlFrame.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlFrame.Controls.Add(this.pnlSeparator);
            this.pnlFrame.Controls.Add(this.pnlEditorBg);
            this.pnlFrame.Controls.Add(this.flowHints);
            this.pnlFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFrame.Location = new System.Drawing.Point(0, 0);
            this.pnlFrame.Name = "pnlFrame";
            this.pnlFrame.Size = new System.Drawing.Size(532, 220);
            this.pnlFrame.TabIndex = 2;
            // 
            // flowHints
            // 
            this.flowHints.BackColor = System.Drawing.SystemColors.Window;
            this.flowHints.Location = new System.Drawing.Point(4, 192);
            this.flowHints.Name = "flowHints";
            this.flowHints.Margin = new System.Windows.Forms.Padding(0);
            this.flowHints.Size = new System.Drawing.Size(516, 24);
            this.flowHints.TabIndex = 1;
            // 
            // pnlEditorBg
            // 
            this.pnlEditorBg.Controls.Add(this.txtEntry);
            this.pnlEditorBg.Location = new System.Drawing.Point(0, 0);
            this.pnlEditorBg.Name = "pnlEditorBg";
            this.pnlEditorBg.Size = new System.Drawing.Size(532, 164);
            this.pnlEditorBg.TabIndex = 2;
            // 
            // pnlSeparator
            // 
            this.pnlSeparator.BackColor = System.Drawing.Color.DarkGray;
            this.pnlSeparator.Location = new System.Drawing.Point(4, 168);
            this.pnlSeparator.Name = "pnlSeparator";
            this.pnlSeparator.Size = new System.Drawing.Size(516, 8);
            this.pnlSeparator.TabIndex = 3;
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
            this.pnlEditorBg.ResumeLayout(false);
            this.pnlEditorBg.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ZD.DictEditor.HintingTextBox txtEntry;
        private System.Windows.Forms.Panel pnlFrame;
        private System.Windows.Forms.FlowLayoutPanel flowHints;
        private System.Windows.Forms.Panel pnlSeparator;
        private System.Windows.Forms.Panel pnlEditorBg;
    }
}
