namespace ZD.DictEditor
{
    partial class HwCtrl
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
            this.pbStatus = new System.Windows.Forms.PictureBox();
            this.lblHeadword = new System.Windows.Forms.Label();
            this.lblExtract = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // pbStatus
            // 
            this.pbStatus.BackColor = System.Drawing.SystemColors.Control;
            this.pbStatus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbStatus.Location = new System.Drawing.Point(4, 0);
            this.pbStatus.Name = "pbStatus";
            this.pbStatus.Size = new System.Drawing.Size(20, 20);
            this.pbStatus.TabIndex = 0;
            this.pbStatus.TabStop = false;
            // 
            // lblHeadword
            // 
            this.lblHeadword.AutoEllipsis = true;
            this.lblHeadword.BackColor = System.Drawing.SystemColors.Control;
            this.lblHeadword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadword.Location = new System.Drawing.Point(28, 0);
            this.lblHeadword.Name = "lblHeadword";
            this.lblHeadword.Size = new System.Drawing.Size(252, 20);
            this.lblHeadword.TabIndex = 1;
            this.lblHeadword.Text = "早上好给你";
            // 
            // lblExtract
            // 
            this.lblExtract.AutoEllipsis = true;
            this.lblExtract.BackColor = System.Drawing.Color.SeaShell;
            this.lblExtract.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExtract.Location = new System.Drawing.Point(28, 20);
            this.lblExtract.Name = "lblExtract";
            this.lblExtract.Size = new System.Drawing.Size(252, 16);
            this.lblExtract.TabIndex = 2;
            this.lblExtract.Text = "Good morning to you; I wish you a perfectly happy good morning!";
            // 
            // HwCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Magenta;
            this.Controls.Add(this.lblExtract);
            this.Controls.Add(this.lblHeadword);
            this.Controls.Add(this.pbStatus);
            this.Name = "HwCtrl";
            this.Size = new System.Drawing.Size(286, 36);
            ((System.ComponentModel.ISupportInitialize)(this.pbStatus)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbStatus;
        private System.Windows.Forms.Label lblHeadword;
        private System.Windows.Forms.Label lblExtract;
    }
}
