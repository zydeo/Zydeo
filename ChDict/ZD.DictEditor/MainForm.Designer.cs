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
            this.txtHeadSimp = new System.Windows.Forms.TextBox();
            this.txtHeadTrad = new System.Windows.Forms.TextBox();
            this.txtHeadPinyin = new System.Windows.Forms.TextBox();
            this.pnlCommands = new System.Windows.Forms.Panel();
            this.btnDrop = new System.Windows.Forms.Button();
            this.btnMark = new System.Windows.Forms.Button();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnGoogleImg = new System.Windows.Forms.Button();
            this.btnGoogleTrans = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHeads)).BeginInit();
            this.pnlInfo.SuspendLayout();
            this.pnlCommands.SuspendLayout();
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
            this.dgvHeads.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dgvHeads.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvHeads.ColumnHeadersVisible = false;
            this.dgvHeads.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
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
            this.pnlInfo.Location = new System.Drawing.Point(232, 256);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Size = new System.Drawing.Size(708, 352);
            this.pnlInfo.TabIndex = 2;
            this.pnlInfo.TabStop = true;
            // 
            // wcInfo
            // 
            this.wcInfo.AllowWebBrowserDrop = false;
            this.wcInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wcInfo.IsWebBrowserContextMenuEnabled = false;
            this.wcInfo.Location = new System.Drawing.Point(0, 0);
            this.wcInfo.MinimumSize = new System.Drawing.Size(20, 20);
            this.wcInfo.Name = "wcInfo";
            this.wcInfo.Size = new System.Drawing.Size(706, 350);
            this.wcInfo.TabIndex = 2;
            this.wcInfo.TabStop = false;
            this.wcInfo.WebBrowserShortcutsEnabled = false;
            // 
            // editor
            // 
            this.editor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editor.BackColor = System.Drawing.SystemColors.Control;
            this.editor.Location = new System.Drawing.Point(232, 40);
            this.editor.Name = "editor";
            this.editor.Size = new System.Drawing.Size(708, 160);
            this.editor.StrSenses = "";
            this.editor.TabIndex = 1;
            // 
            // txtHeadSimp
            // 
            this.txtHeadSimp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHeadSimp.Font = new System.Drawing.Font("Noto Sans S Chinese Regular", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtHeadSimp.Location = new System.Drawing.Point(232, 4);
            this.txtHeadSimp.Name = "txtHeadSimp";
            this.txtHeadSimp.ReadOnly = true;
            this.txtHeadSimp.Size = new System.Drawing.Size(132, 33);
            this.txtHeadSimp.TabIndex = 3;
            this.txtHeadSimp.Text = "很好的天很好好";
            // 
            // txtHeadTrad
            // 
            this.txtHeadTrad.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHeadTrad.Font = new System.Drawing.Font("Noto Sans T Chinese Regular", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtHeadTrad.Location = new System.Drawing.Point(368, 4);
            this.txtHeadTrad.Name = "txtHeadTrad";
            this.txtHeadTrad.ReadOnly = true;
            this.txtHeadTrad.Size = new System.Drawing.Size(132, 33);
            this.txtHeadTrad.TabIndex = 4;
            this.txtHeadTrad.Text = "很好的天很好好";
            // 
            // txtHeadPinyin
            // 
            this.txtHeadPinyin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHeadPinyin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHeadPinyin.Font = new System.Drawing.Font("Noto Sans S Chinese Regular", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHeadPinyin.Location = new System.Drawing.Point(504, 4);
            this.txtHeadPinyin.Name = "txtHeadPinyin";
            this.txtHeadPinyin.ReadOnly = true;
            this.txtHeadPinyin.Size = new System.Drawing.Size(436, 33);
            this.txtHeadPinyin.TabIndex = 5;
            this.txtHeadPinyin.Text = "hen3 hao3 de5 tian1 hen3 hao3 hao3";
            // 
            // pnlCommands
            // 
            this.pnlCommands.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlCommands.Controls.Add(this.btnDrop);
            this.pnlCommands.Controls.Add(this.btnMark);
            this.pnlCommands.Controls.Add(this.btnConfirm);
            this.pnlCommands.Controls.Add(this.btnGoogleImg);
            this.pnlCommands.Controls.Add(this.btnGoogleTrans);
            this.pnlCommands.Location = new System.Drawing.Point(232, 204);
            this.pnlCommands.Name = "pnlCommands";
            this.pnlCommands.Size = new System.Drawing.Size(708, 46);
            this.pnlCommands.TabIndex = 6;
            // 
            // btnDrop
            // 
            this.btnDrop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDrop.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnDrop.Location = new System.Drawing.Point(376, 0);
            this.btnDrop.Name = "btnDrop";
            this.btnDrop.Size = new System.Drawing.Size(188, 23);
            this.btnDrop.TabIndex = 4;
            this.btnDrop.Text = "Drop (Cltr+D)";
            this.btnDrop.UseVisualStyleBackColor = true;
            // 
            // btnMark
            // 
            this.btnMark.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMark.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMark.Location = new System.Drawing.Point(188, 0);
            this.btnMark.Name = "btnMark";
            this.btnMark.Size = new System.Drawing.Size(188, 23);
            this.btnMark.TabIndex = 3;
            this.btnMark.Text = "Mark (Cltr+M)";
            this.btnMark.UseVisualStyleBackColor = true;
            // 
            // btnConfirm
            // 
            this.btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirm.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfirm.Location = new System.Drawing.Point(0, 0);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(188, 23);
            this.btnConfirm.TabIndex = 2;
            this.btnConfirm.Text = "Confirm (Cltr+Enter)";
            this.btnConfirm.UseVisualStyleBackColor = true;
            // 
            // btnGoogleImg
            // 
            this.btnGoogleImg.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGoogleImg.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnGoogleImg.Location = new System.Drawing.Point(188, 23);
            this.btnGoogleImg.Name = "btnGoogleImg";
            this.btnGoogleImg.Size = new System.Drawing.Size(188, 23);
            this.btnGoogleImg.TabIndex = 1;
            this.btnGoogleImg.Text = "Google Image Search (Cltr+I)";
            this.btnGoogleImg.UseVisualStyleBackColor = true;
            // 
            // btnGoogleTrans
            // 
            this.btnGoogleTrans.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGoogleTrans.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnGoogleTrans.Location = new System.Drawing.Point(0, 23);
            this.btnGoogleTrans.Name = "btnGoogleTrans";
            this.btnGoogleTrans.Size = new System.Drawing.Size(188, 23);
            this.btnGoogleTrans.TabIndex = 0;
            this.btnGoogleTrans.Text = "Google MT (Cltr+T)";
            this.btnGoogleTrans.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 611);
            this.Controls.Add(this.pnlCommands);
            this.Controls.Add(this.txtHeadPinyin);
            this.Controls.Add(this.txtHeadTrad);
            this.Controls.Add(this.txtHeadSimp);
            this.Controls.Add(this.pnlInfo);
            this.Controls.Add(this.editor);
            this.Controls.Add(this.dgvHeads);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Zydeo dictionary editor";
            ((System.ComponentModel.ISupportInitialize)(this.dgvHeads)).EndInit();
            this.pnlInfo.ResumeLayout(false);
            this.pnlCommands.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvHeads;
        private EntryEditor editor;
        private System.Windows.Forms.Panel pnlInfo;
        private System.Windows.Forms.WebBrowser wcInfo;
        private System.Windows.Forms.TextBox txtHeadSimp;
        private System.Windows.Forms.TextBox txtHeadTrad;
        private System.Windows.Forms.TextBox txtHeadPinyin;
        private System.Windows.Forms.Panel pnlCommands;
        private System.Windows.Forms.Button btnDrop;
        private System.Windows.Forms.Button btnMark;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnGoogleImg;
        private System.Windows.Forms.Button btnGoogleTrans;
    }
}

