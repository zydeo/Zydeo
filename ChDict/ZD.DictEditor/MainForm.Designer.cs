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
            this.txtFilter = new System.Windows.Forms.TextBox();
            this.statStrip = new System.Windows.Forms.StatusStrip();
            this.tsLabelDone = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsLabelDoneVal = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsEditedMarked = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsEditedMarkedVal = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsDropped = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsDroppedVal = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsRemaining = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsRemainingVal = new System.Windows.Forms.ToolStripStatusLabel();
            this.cbEdMarked = new System.Windows.Forms.CheckBox();
            this.lblPipe = new System.Windows.Forms.Label();
            this.llJump = new System.Windows.Forms.LinkLabel();
            this.pnlSplit = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHeads)).BeginInit();
            this.pnlInfo.SuspendLayout();
            this.pnlCommands.SuspendLayout();
            this.statStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlSplit)).BeginInit();
            this.pnlSplit.Panel1.SuspendLayout();
            this.pnlSplit.Panel2.SuspendLayout();
            this.pnlSplit.SuspendLayout();
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
            this.dgvHeads.Location = new System.Drawing.Point(6, 60);
            this.dgvHeads.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
            this.dgvHeads.Size = new System.Drawing.Size(336, 844);
            this.dgvHeads.TabIndex = 0;
            // 
            // pnlInfo
            // 
            this.pnlInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlInfo.Controls.Add(this.wcInfo);
            this.pnlInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlInfo.Location = new System.Drawing.Point(0, 0);
            this.pnlInfo.Margin = new System.Windows.Forms.Padding(0);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Size = new System.Drawing.Size(1060, 480);
            this.pnlInfo.TabIndex = 2;
            this.pnlInfo.TabStop = true;
            // 
            // wcInfo
            // 
            this.wcInfo.AllowWebBrowserDrop = false;
            this.wcInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wcInfo.IsWebBrowserContextMenuEnabled = false;
            this.wcInfo.Location = new System.Drawing.Point(0, 0);
            this.wcInfo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.wcInfo.MinimumSize = new System.Drawing.Size(30, 31);
            this.wcInfo.Name = "wcInfo";
            this.wcInfo.Size = new System.Drawing.Size(1058, 478);
            this.wcInfo.TabIndex = 2;
            this.wcInfo.TabStop = false;
            this.wcInfo.WebBrowserShortcutsEnabled = false;
            // 
            // editor
            // 
            this.editor.BackColor = System.Drawing.SystemColors.Control;
            this.editor.Location = new System.Drawing.Point(12, 8);
            this.editor.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.editor.Name = "editor";
            this.editor.Size = new System.Drawing.Size(1032, 260);
            this.editor.StrSenses = "";
            this.editor.TabIndex = 1;
            // 
            // txtHeadSimp
            // 
            this.txtHeadSimp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHeadSimp.Font = new System.Drawing.Font("Noto Sans S Chinese Regular", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtHeadSimp.Location = new System.Drawing.Point(348, 6);
            this.txtHeadSimp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtHeadSimp.Name = "txtHeadSimp";
            this.txtHeadSimp.ReadOnly = true;
            this.txtHeadSimp.Size = new System.Drawing.Size(197, 46);
            this.txtHeadSimp.TabIndex = 3;
            this.txtHeadSimp.Text = "很好的天很好好";
            this.txtHeadSimp.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtHeadTrad
            // 
            this.txtHeadTrad.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHeadTrad.Font = new System.Drawing.Font("Noto Sans T Chinese Regular", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtHeadTrad.Location = new System.Drawing.Point(552, 6);
            this.txtHeadTrad.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtHeadTrad.Name = "txtHeadTrad";
            this.txtHeadTrad.ReadOnly = true;
            this.txtHeadTrad.Size = new System.Drawing.Size(197, 46);
            this.txtHeadTrad.TabIndex = 4;
            this.txtHeadTrad.Text = "很好的天很好好";
            this.txtHeadTrad.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtHeadPinyin
            // 
            this.txtHeadPinyin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHeadPinyin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHeadPinyin.Font = new System.Drawing.Font("Noto Sans S Chinese Regular", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHeadPinyin.Location = new System.Drawing.Point(756, 6);
            this.txtHeadPinyin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtHeadPinyin.Name = "txtHeadPinyin";
            this.txtHeadPinyin.ReadOnly = true;
            this.txtHeadPinyin.Size = new System.Drawing.Size(653, 46);
            this.txtHeadPinyin.TabIndex = 5;
            this.txtHeadPinyin.Text = "hen3 hao3 de5 tian1 hen3 hao3 hao3";
            // 
            // pnlCommands
            // 
            this.pnlCommands.Controls.Add(this.btnDrop);
            this.pnlCommands.Controls.Add(this.btnMark);
            this.pnlCommands.Controls.Add(this.btnConfirm);
            this.pnlCommands.Controls.Add(this.btnGoogleImg);
            this.pnlCommands.Controls.Add(this.btnGoogleTrans);
            this.pnlCommands.Location = new System.Drawing.Point(12, 280);
            this.pnlCommands.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlCommands.Name = "pnlCommands";
            this.pnlCommands.Size = new System.Drawing.Size(1032, 71);
            this.pnlCommands.TabIndex = 6;
            // 
            // btnDrop
            // 
            this.btnDrop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDrop.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnDrop.Location = new System.Drawing.Point(564, 0);
            this.btnDrop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDrop.Name = "btnDrop";
            this.btnDrop.Size = new System.Drawing.Size(282, 35);
            this.btnDrop.TabIndex = 4;
            this.btnDrop.Text = "Drop (Cltr+D)";
            this.btnDrop.UseVisualStyleBackColor = true;
            // 
            // btnMark
            // 
            this.btnMark.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMark.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMark.Location = new System.Drawing.Point(282, 0);
            this.btnMark.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnMark.Name = "btnMark";
            this.btnMark.Size = new System.Drawing.Size(282, 35);
            this.btnMark.TabIndex = 3;
            this.btnMark.Text = "Mark (Cltr+M)";
            this.btnMark.UseVisualStyleBackColor = true;
            // 
            // btnConfirm
            // 
            this.btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirm.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfirm.Location = new System.Drawing.Point(0, 0);
            this.btnConfirm.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(282, 35);
            this.btnConfirm.TabIndex = 2;
            this.btnConfirm.Text = "Confirm (Cltr+Enter)";
            this.btnConfirm.UseVisualStyleBackColor = true;
            // 
            // btnGoogleImg
            // 
            this.btnGoogleImg.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGoogleImg.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnGoogleImg.Location = new System.Drawing.Point(282, 35);
            this.btnGoogleImg.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnGoogleImg.Name = "btnGoogleImg";
            this.btnGoogleImg.Size = new System.Drawing.Size(282, 35);
            this.btnGoogleImg.TabIndex = 1;
            this.btnGoogleImg.Text = "Google Image Search (Cltr+I)";
            this.btnGoogleImg.UseVisualStyleBackColor = true;
            // 
            // btnGoogleTrans
            // 
            this.btnGoogleTrans.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGoogleTrans.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnGoogleTrans.Location = new System.Drawing.Point(0, 35);
            this.btnGoogleTrans.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnGoogleTrans.Name = "btnGoogleTrans";
            this.btnGoogleTrans.Size = new System.Drawing.Size(282, 35);
            this.btnGoogleTrans.TabIndex = 0;
            this.btnGoogleTrans.Text = "Google MT (Cltr+T)";
            this.btnGoogleTrans.UseVisualStyleBackColor = true;
            // 
            // txtFilter
            // 
            this.txtFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFilter.Font = new System.Drawing.Font("Noto Sans S Chinese Regular", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtFilter.Location = new System.Drawing.Point(8, 8);
            this.txtFilter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new System.Drawing.Size(190, 46);
            this.txtFilter.TabIndex = 7;
            this.txtFilter.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // statStrip
            // 
            this.statStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsLabelDone,
            this.tsLabelDoneVal,
            this.tsEditedMarked,
            this.tsEditedMarkedVal,
            this.tsDropped,
            this.tsDroppedVal,
            this.tsRemaining,
            this.tsRemainingVal});
            this.statStrip.Location = new System.Drawing.Point(0, 910);
            this.statStrip.Name = "statStrip";
            this.statStrip.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
            this.statStrip.Size = new System.Drawing.Size(1416, 30);
            this.statStrip.TabIndex = 8;
            // 
            // tsLabelDone
            // 
            this.tsLabelDone.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tsLabelDone.Name = "tsLabelDone";
            this.tsLabelDone.Size = new System.Drawing.Size(62, 25);
            this.tsLabelDone.Text = "Done:";
            // 
            // tsLabelDoneVal
            // 
            this.tsLabelDoneVal.Name = "tsLabelDoneVal";
            this.tsLabelDoneVal.Size = new System.Drawing.Size(82, 25);
            this.tsLabelDoneVal.Text = "145 (1%)";
            // 
            // tsEditedMarked
            // 
            this.tsEditedMarked.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tsEditedMarked.Name = "tsEditedMarked";
            this.tsEditedMarked.Size = new System.Drawing.Size(143, 25);
            this.tsEditedMarked.Text = "Edited/marked:";
            // 
            // tsEditedMarkedVal
            // 
            this.tsEditedMarkedVal.Name = "tsEditedMarkedVal";
            this.tsEditedMarkedVal.Size = new System.Drawing.Size(22, 25);
            this.tsEditedMarkedVal.Text = "3";
            // 
            // tsDropped
            // 
            this.tsDropped.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tsDropped.Name = "tsDropped";
            this.tsDropped.Size = new System.Drawing.Size(91, 25);
            this.tsDropped.Text = "Dropped:";
            // 
            // tsDroppedVal
            // 
            this.tsDroppedVal.Name = "tsDroppedVal";
            this.tsDroppedVal.Size = new System.Drawing.Size(22, 25);
            this.tsDroppedVal.Text = "4";
            // 
            // tsRemaining
            // 
            this.tsRemaining.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tsRemaining.Name = "tsRemaining";
            this.tsRemaining.Size = new System.Drawing.Size(108, 25);
            this.tsRemaining.Text = "Remaining:";
            // 
            // tsRemainingVal
            // 
            this.tsRemainingVal.Name = "tsRemainingVal";
            this.tsRemainingVal.Size = new System.Drawing.Size(62, 25);
            this.tsRemainingVal.Text = "12634";
            // 
            // cbEdMarked
            // 
            this.cbEdMarked.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbEdMarked.Location = new System.Drawing.Point(210, 12);
            this.cbEdMarked.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbEdMarked.Name = "cbEdMarked";
            this.cbEdMarked.Size = new System.Drawing.Size(84, 43);
            this.cbEdMarked.TabIndex = 9;
            this.cbEdMarked.Text = "E&&M";
            this.cbEdMarked.UseVisualStyleBackColor = true;
            // 
            // lblPipe
            // 
            this.lblPipe.Location = new System.Drawing.Point(294, 12);
            this.lblPipe.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPipe.Name = "lblPipe";
            this.lblPipe.Size = new System.Drawing.Size(12, 37);
            this.lblPipe.TabIndex = 10;
            this.lblPipe.Text = "|";
            this.lblPipe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // llJump
            // 
            this.llJump.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.llJump.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.llJump.Location = new System.Drawing.Point(312, 12);
            this.llJump.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.llJump.Name = "llJump";
            this.llJump.Size = new System.Drawing.Size(30, 37);
            this.llJump.TabIndex = 11;
            this.llJump.TabStop = true;
            this.llJump.Text = ">>";
            this.llJump.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlSplit
            // 
            this.pnlSplit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.pnlSplit.Location = new System.Drawing.Point(348, 60);
            this.pnlSplit.Margin = new System.Windows.Forms.Padding(0);
            this.pnlSplit.Name = "pnlSplit";
            this.pnlSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // pnlSplit.Panel1
            // 
            this.pnlSplit.Panel1.Controls.Add(this.pnlCommands);
            this.pnlSplit.Panel1.Controls.Add(this.editor);
            // 
            // pnlSplit.Panel2
            // 
            this.pnlSplit.Panel2.Controls.Add(this.pnlInfo);
            this.pnlSplit.Size = new System.Drawing.Size(1060, 844);
            this.pnlSplit.SplitterDistance = 360;
            this.pnlSplit.TabIndex = 12;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1416, 940);
            this.Controls.Add(this.pnlSplit);
            this.Controls.Add(this.llJump);
            this.Controls.Add(this.lblPipe);
            this.Controls.Add(this.cbEdMarked);
            this.Controls.Add(this.statStrip);
            this.Controls.Add(this.txtFilter);
            this.Controls.Add(this.txtHeadPinyin);
            this.Controls.Add(this.txtHeadTrad);
            this.Controls.Add(this.txtHeadSimp);
            this.Controls.Add(this.dgvHeads);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Zydeo dictionary editor";
            ((System.ComponentModel.ISupportInitialize)(this.dgvHeads)).EndInit();
            this.pnlInfo.ResumeLayout(false);
            this.pnlCommands.ResumeLayout(false);
            this.statStrip.ResumeLayout(false);
            this.statStrip.PerformLayout();
            this.pnlSplit.Panel1.ResumeLayout(false);
            this.pnlSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pnlSplit)).EndInit();
            this.pnlSplit.ResumeLayout(false);
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
        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.StatusStrip statStrip;
        private System.Windows.Forms.ToolStripStatusLabel tsLabelDone;
        private System.Windows.Forms.ToolStripStatusLabel tsLabelDoneVal;
        private System.Windows.Forms.ToolStripStatusLabel tsEditedMarked;
        private System.Windows.Forms.ToolStripStatusLabel tsEditedMarkedVal;
        private System.Windows.Forms.ToolStripStatusLabel tsRemaining;
        private System.Windows.Forms.ToolStripStatusLabel tsRemainingVal;
        private System.Windows.Forms.CheckBox cbEdMarked;
        private System.Windows.Forms.Label lblPipe;
        private System.Windows.Forms.LinkLabel llJump;
        private System.Windows.Forms.ToolStripStatusLabel tsDropped;
        private System.Windows.Forms.ToolStripStatusLabel tsDroppedVal;
        private System.Windows.Forms.SplitContainer pnlSplit;
    }
}

