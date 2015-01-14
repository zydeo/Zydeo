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
            this.lblPinyin = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // pbStatus
            // 
            this.pbStatus.BackColor = System.Drawing.Color.Transparent;
            this.pbStatus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbStatus.Location = new System.Drawing.Point(4, 6);
            this.pbStatus.Name = "pbStatus";
            this.pbStatus.Size = new System.Drawing.Size(12, 12);
            this.pbStatus.TabIndex = 0;
            this.pbStatus.TabStop = false;
            // 
            // lblHeadword
            // 
            this.lblHeadword.AutoEllipsis = true;
            this.lblHeadword.BackColor = System.Drawing.Color.Transparent;
            this.lblHeadword.Font = new System.Drawing.Font("Noto Sans S Chinese Regular", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadword.Location = new System.Drawing.Point(18, 0);
            this.lblHeadword.Name = "lblHeadword";
            this.lblHeadword.Size = new System.Drawing.Size(262, 22);
            this.lblHeadword.TabIndex = 1;
            this.lblHeadword.Text = "早上好给你";
            // 
            // lblExtract
            // 
            this.lblExtract.AutoEllipsis = true;
            this.lblExtract.BackColor = System.Drawing.Color.Transparent;
            this.lblExtract.ForeColor = System.Drawing.Color.DimGray;
            this.lblExtract.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExtract.Location = new System.Drawing.Point(18, 38);
            this.lblExtract.Name = "lblExtract";
            this.lblExtract.Size = new System.Drawing.Size(262, 16);
            this.lblExtract.TabIndex = 2;
            this.lblExtract.Text = "Good morning to you; I wish you a perfectly happy good morning!";
            // 
            // lblPinyin
            // 
            this.lblPinyin.AutoEllipsis = true;
            this.lblPinyin.BackColor = System.Drawing.Color.Transparent;
            this.lblPinyin.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPinyin.Location = new System.Drawing.Point(18, 22);
            this.lblPinyin.Name = "lblPinyin";
            this.lblPinyin.Size = new System.Drawing.Size(262, 16);
            this.lblPinyin.TabIndex = 3;
            this.lblPinyin.Text = "zao shang hao gei ni";
            // 
            // HwCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.lblPinyin);
            this.Controls.Add(this.lblExtract);
            this.Controls.Add(this.lblHeadword);
            this.Controls.Add(this.pbStatus);
            this.Name = "HwCtrl";
            this.Size = new System.Drawing.Size(286, 56);
            ((System.ComponentModel.ISupportInitialize)(this.pbStatus)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbStatus;
        private System.Windows.Forms.Label lblHeadword;
        private System.Windows.Forms.Label lblExtract;
        private System.Windows.Forms.Label lblPinyin;
    }
}
