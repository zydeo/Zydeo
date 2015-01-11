namespace ZD.DictEditor
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgvHeads = new System.Windows.Forms.DataGridView();
            this.pnlInfo = new System.Windows.Forms.Panel();
            this.wcInfo = new System.Windows.Forms.WebBrowser();
            this.editor = new ZD.DictEditor.EntryEditor();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHeads)).BeginInit();
            this.pnlInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvHeads
            // 
            this.dgvHeads.AllowUserToAddRows = false;
            this.dgvHeads.AllowUserToDeleteRows = false;
            this.dgvHeads.AllowUserToResizeColumns = false;
            this.dgvHeads.AllowUserToResizeRows = false;
            this.dgvHeads.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.dgvHeads.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvHeads.CausesValidation = false;
            this.dgvHeads.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvHeads.ColumnHeadersVisible = false;
            this.dgvHeads.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvHeads.GridColor = System.Drawing.Color.Lavender;
            this.dgvHeads.Location = new System.Drawing.Point(4, 4);
            this.dgvHeads.MultiSelect = false;
            this.dgvHeads.Name = "dgvHeads";
            this.dgvHeads.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvHeads.RowHeadersVisible = false;
            this.dgvHeads.RowTemplate.Height = 36;
            this.dgvHeads.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvHeads.ShowCellErrors = false;
            this.dgvHeads.ShowCellToolTips = false;
            this.dgvHeads.ShowEditingIcon = false;
            this.dgvHeads.ShowRowErrors = false;
            this.dgvHeads.Size = new System.Drawing.Size(224, 604);
            this.dgvHeads.TabIndex = 0;
            // 
            // pnlInfo
            // 
            this.pnlInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlInfo.Controls.Add(this.wcInfo);
            this.pnlInfo.Location = new System.Drawing.Point(232, 224);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Size = new System.Drawing.Size(708, 384);
            this.pnlInfo.TabIndex = 2;
            this.pnlInfo.TabStop = true;
            // 
            // wcInfo
            // 
            this.wcInfo.AllowNavigation = true;
            this.wcInfo.AllowWebBrowserDrop = false;
            this.wcInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wcInfo.IsWebBrowserContextMenuEnabled = false;
            this.wcInfo.Location = new System.Drawing.Point(0, 0);
            this.wcInfo.MinimumSize = new System.Drawing.Size(20, 20);
            this.wcInfo.Name = "wcInfo";
            this.wcInfo.Size = new System.Drawing.Size(706, 382);
            this.wcInfo.TabIndex = 2;
            this.wcInfo.TabStop = false;
            this.wcInfo.WebBrowserShortcutsEnabled = false;
            // 
            // editor
            // 
            this.editor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editor.BackColor = System.Drawing.SystemColors.Control;
            this.editor.Location = new System.Drawing.Point(232, 4);
            this.editor.Name = "editor";
            this.editor.Size = new System.Drawing.Size(708, 216);
            this.editor.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 611);
            this.Controls.Add(this.pnlInfo);
            this.Controls.Add(this.editor);
            this.Controls.Add(this.dgvHeads);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Zydeo dictionary editor";
            ((System.ComponentModel.ISupportInitialize)(this.dgvHeads)).EndInit();
            this.pnlInfo.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvHeads;
        private EntryEditor editor;
        private System.Windows.Forms.Panel pnlInfo;
        private System.Windows.Forms.WebBrowser wcInfo;
    }
}

