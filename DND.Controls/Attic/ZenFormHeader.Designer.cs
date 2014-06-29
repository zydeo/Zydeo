namespace DND.Controls
{
    partial class ZenFormHeader
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
            this.pnlClose = new System.Windows.Forms.Panel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // pnlClose
            // 
            this.pnlClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlClose.BackColor = System.Drawing.Color.DarkRed;
            this.pnlClose.Location = new System.Drawing.Point(308, 0);
            this.pnlClose.Name = "pnlClose";
            this.pnlClose.Size = new System.Drawing.Size(28, 18);
            this.pnlClose.TabIndex = 0;
            // 
            // lblHeader
            // 
            this.lblHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHeader.AutoEllipsis = true;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.Location = new System.Drawing.Point(52, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(252, 24);
            this.lblHeader.TabIndex = 1;
            this.lblHeader.Text = "Header";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ZenFormHeader
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.PaleGreen;
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.pnlClose);
            this.Name = "ZenFormHeader";
            this.Size = new System.Drawing.Size(344, 28);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlClose;
        private System.Windows.Forms.Label lblHeader;
    }
}
