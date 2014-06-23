namespace Sandbox
{
    partial class MainForm
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
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlPad = new System.Windows.Forms.Panel();
            this.pnlChars = new System.Windows.Forms.Panel();
            this.lblChars = new System.Windows.Forms.Label();
            this.llClear = new System.Windows.Forms.LinkLabel();
            this.writingPad = new DND.Controls.WritingPad();
            this.pnlPad.SuspendLayout();
            this.pnlChars.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(252, 385);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(80, 23);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // pnlPad
            // 
            this.pnlPad.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlPad.BackColor = System.Drawing.Color.Thistle;
            this.pnlPad.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlPad.Controls.Add(this.writingPad);
            this.pnlPad.Location = new System.Drawing.Point(8, 56);
            this.pnlPad.Name = "pnlPad";
            this.pnlPad.Size = new System.Drawing.Size(324, 324);
            this.pnlPad.TabIndex = 1;
            // 
            // pnlChars
            // 
            this.pnlChars.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlChars.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlChars.Controls.Add(this.lblChars);
            this.pnlChars.Location = new System.Drawing.Point(8, 8);
            this.pnlChars.Name = "pnlChars";
            this.pnlChars.Size = new System.Drawing.Size(324, 44);
            this.pnlChars.TabIndex = 2;
            // 
            // lblChars
            // 
            this.lblChars.BackColor = System.Drawing.SystemColors.Window;
            this.lblChars.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblChars.Font = new System.Drawing.Font("Segoe UI", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChars.Location = new System.Drawing.Point(0, 0);
            this.lblChars.Name = "lblChars";
            this.lblChars.Size = new System.Drawing.Size(320, 40);
            this.lblChars.TabIndex = 0;
            this.lblChars.Text = "毛泽东你好";
            this.lblChars.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // llClear
            // 
            this.llClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.llClear.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.llClear.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.llClear.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.llClear.Location = new System.Drawing.Point(10, 390);
            this.llClear.Name = "llClear";
            this.llClear.Size = new System.Drawing.Size(50, 18);
            this.llClear.TabIndex = 3;
            this.llClear.TabStop = true;
            this.llClear.Text = "Clear";
            this.llClear.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.llClear.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llClear_LinkClicked);
            // 
            // writingPad
            // 
            this.writingPad.BackColor = System.Drawing.SystemColors.Window;
            this.writingPad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.writingPad.Location = new System.Drawing.Point(0, 0);
            this.writingPad.Name = "writingPad";
            this.writingPad.Size = new System.Drawing.Size(320, 320);
            this.writingPad.TabIndex = 0;
            this.writingPad.Text = "writingPad1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(339, 416);
            this.Controls.Add(this.llClear);
            this.Controls.Add(this.pnlChars);
            this.Controls.Add(this.pnlPad);
            this.Controls.Add(this.btnClose);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "WritingPad sandbox";
            this.pnlPad.ResumeLayout(false);
            this.pnlChars.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel pnlPad;
        private System.Windows.Forms.Panel pnlChars;
        private System.Windows.Forms.Label lblChars;
        private DND.Controls.WritingPad writingPad;
        private System.Windows.Forms.LinkLabel llClear;
    }
}

