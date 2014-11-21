namespace ZD
{
    partial class FatalErrorForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblHeader = new System.Windows.Forms.Label();
            this.pnlOuter = new System.Windows.Forms.Panel();
            this.pnlInner = new System.Windows.Forms.Panel();
            this.lblSorry = new System.Windows.Forms.Label();
            this.lblWhy = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlOuter.SuspendLayout();
            this.pnlInner.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            this.lblHeader.BackColor = System.Drawing.Color.Green;
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(513, 24);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "Zydeo error";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pnlOuter
            // 
            this.pnlOuter.BackColor = System.Drawing.Color.DimGray;
            this.pnlOuter.Controls.Add(this.pnlInner);
            this.pnlOuter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOuter.Location = new System.Drawing.Point(0, 24);
            this.pnlOuter.Name = "pnlOuter";
            this.pnlOuter.Padding = new System.Windows.Forms.Padding(1);
            this.pnlOuter.Size = new System.Drawing.Size(513, 145);
            this.pnlOuter.TabIndex = 1;
            // 
            // pnlInner
            // 
            this.pnlInner.BackColor = System.Drawing.Color.White;
            this.pnlInner.Controls.Add(this.btnClose);
            this.pnlInner.Controls.Add(this.lblWhy);
            this.pnlInner.Controls.Add(this.lblSorry);
            this.pnlInner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlInner.Location = new System.Drawing.Point(1, 1);
            this.pnlInner.Name = "pnlInner";
            this.pnlInner.Size = new System.Drawing.Size(511, 143);
            this.pnlInner.TabIndex = 0;
            // 
            // lblSorry
            // 
            this.lblSorry.AutoSize = true;
            this.lblSorry.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSorry.Location = new System.Drawing.Point(43, 12);
            this.lblSorry.Name = "lblSorry";
            this.lblSorry.Size = new System.Drawing.Size(96, 25);
            this.lblSorry.TabIndex = 0;
            this.lblSorry.Text = ";-(  Sorry.";
            // 
            // lblWhy
            // 
            this.lblWhy.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWhy.Location = new System.Drawing.Point(44, 40);
            this.lblWhy.Name = "lblWhy";
            this.lblWhy.Size = new System.Drawing.Size(456, 66);
            this.lblWhy.TabIndex = 1;
            this.lblWhy.Text = "Zydeo encountered a problem and had to close.\r\nIt is Zydeo\'s fault, not yours.";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(409, 109);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(91, 23);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // FatalErrorForm
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(513, 169);
            this.Controls.Add(this.pnlOuter);
            this.Controls.Add(this.lblHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FatalErrorForm";
            this.Text = "FatalErrorForm";
            this.pnlOuter.ResumeLayout(false);
            this.pnlInner.ResumeLayout(false);
            this.pnlInner.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Panel pnlOuter;
        private System.Windows.Forms.Panel pnlInner;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblWhy;
        private System.Windows.Forms.Label lblSorry;
    }
}