namespace ZD.Colloc
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtMaxFreq = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtRightWin = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtLeftWin = new System.Windows.Forms.TextBox();
            this.rbChSqCorr = new System.Windows.Forms.RadioButton();
            this.rbLogLike = new System.Windows.Forms.RadioButton();
            this.txtMinFreq = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pbar = new System.Windows.Forms.ProgressBar();
            this.btnGo = new System.Windows.Forms.Button();
            this.lnkLoadFreq = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.txtQuery = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.wb = new System.Windows.Forms.WebBrowser();
            this.btnClose = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.txtMaxFreq);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtRightWin);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtLeftWin);
            this.groupBox1.Controls.Add(this.rbChSqCorr);
            this.groupBox1.Controls.Add(this.rbLogLike);
            this.groupBox1.Controls.Add(this.txtMinFreq);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.pbar);
            this.groupBox1.Controls.Add(this.btnGo);
            this.groupBox1.Controls.Add(this.lnkLoadFreq);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtQuery);
            this.groupBox1.Location = new System.Drawing.Point(8, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1053, 140);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Test details";
            // 
            // txtMaxFreq
            // 
            this.txtMaxFreq.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMaxFreq.Location = new System.Drawing.Point(420, 72);
            this.txtMaxFreq.Name = "txtMaxFreq";
            this.txtMaxFreq.Size = new System.Drawing.Size(100, 30);
            this.txtMaxFreq.TabIndex = 14;
            this.txtMaxFreq.Text = "10000";
            this.txtMaxFreq.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(308, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 34);
            this.label5.TabIndex = 13;
            this.label5.Text = "Max freq";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(540, 32);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(132, 34);
            this.label4.TabIndex = 11;
            this.label4.Text = "Right window";
            // 
            // txtRightWin
            // 
            this.txtRightWin.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRightWin.Location = new System.Drawing.Point(672, 31);
            this.txtRightWin.Name = "txtRightWin";
            this.txtRightWin.Size = new System.Drawing.Size(60, 30);
            this.txtRightWin.TabIndex = 12;
            this.txtRightWin.Text = "12";
            this.txtRightWin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(328, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(132, 34);
            this.label3.TabIndex = 9;
            this.label3.Text = "Left window";
            // 
            // txtLeftWin
            // 
            this.txtLeftWin.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLeftWin.Location = new System.Drawing.Point(460, 31);
            this.txtLeftWin.Name = "txtLeftWin";
            this.txtLeftWin.Size = new System.Drawing.Size(60, 30);
            this.txtLeftWin.TabIndex = 10;
            this.txtLeftWin.Text = "12";
            this.txtLeftWin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // rbChSqCorr
            // 
            this.rbChSqCorr.AutoSize = true;
            this.rbChSqCorr.Checked = true;
            this.rbChSqCorr.Location = new System.Drawing.Point(644, 76);
            this.rbChSqCorr.Name = "rbChSqCorr";
            this.rbChSqCorr.Size = new System.Drawing.Size(103, 25);
            this.rbChSqCorr.TabIndex = 8;
            this.rbChSqCorr.TabStop = true;
            this.rbChSqCorr.Text = "ChSqCorr";
            this.rbChSqCorr.UseVisualStyleBackColor = true;
            // 
            // rbLogLike
            // 
            this.rbLogLike.AutoSize = true;
            this.rbLogLike.Location = new System.Drawing.Point(544, 76);
            this.rbLogLike.Name = "rbLogLike";
            this.rbLogLike.Size = new System.Drawing.Size(89, 25);
            this.rbLogLike.TabIndex = 7;
            this.rbLogLike.Text = "LogLike";
            this.rbLogLike.UseVisualStyleBackColor = true;
            // 
            // txtMinFreq
            // 
            this.txtMinFreq.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMinFreq.Location = new System.Drawing.Point(116, 72);
            this.txtMinFreq.Name = "txtMinFreq";
            this.txtMinFreq.Size = new System.Drawing.Size(100, 30);
            this.txtMinFreq.TabIndex = 6;
            this.txtMinFreq.Text = "100";
            this.txtMinFreq.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(4, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 34);
            this.label2.TabIndex = 5;
            this.label2.Text = "Min freq";
            // 
            // pbar
            // 
            this.pbar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbar.Location = new System.Drawing.Point(8, 116);
            this.pbar.MarqueeAnimationSpeed = 30;
            this.pbar.Name = "pbar";
            this.pbar.Size = new System.Drawing.Size(1037, 12);
            this.pbar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.pbar.TabIndex = 4;
            this.pbar.Visible = false;
            // 
            // btnGo
            // 
            this.btnGo.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnGo.Location = new System.Drawing.Point(216, 29);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(84, 36);
            this.btnGo.TabIndex = 2;
            this.btnGo.Text = "&Go";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // lnkLoadFreq
            // 
            this.lnkLoadFreq.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkLoadFreq.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkLoadFreq.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lnkLoadFreq.Location = new System.Drawing.Point(841, 32);
            this.lnkLoadFreq.Name = "lnkLoadFreq";
            this.lnkLoadFreq.Size = new System.Drawing.Size(208, 32);
            this.lnkLoadFreq.TabIndex = 3;
            this.lnkLoadFreq.TabStop = true;
            this.lnkLoadFreq.Text = "Load frequency data";
            this.lnkLoadFreq.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lnkLoadFreq.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkLoadFreq_LinkClicked);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(4, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 34);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Word";
            // 
            // txtQuery
            // 
            this.txtQuery.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtQuery.Location = new System.Drawing.Point(80, 29);
            this.txtQuery.Name = "txtQuery";
            this.txtQuery.Size = new System.Drawing.Size(136, 35);
            this.txtQuery.TabIndex = 1;
            this.txtQuery.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.wb);
            this.panel1.Location = new System.Drawing.Point(8, 156);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1053, 397);
            this.panel1.TabIndex = 1;
            // 
            // wb
            // 
            this.wb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wb.Location = new System.Drawing.Point(0, 0);
            this.wb.MinimumSize = new System.Drawing.Size(20, 21);
            this.wb.Name = "wb";
            this.wb.Size = new System.Drawing.Size(1049, 393);
            this.wb.TabIndex = 2;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(941, 557);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 39);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // MainForm
            // 
            this.AcceptButton = this.btnGo;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(1067, 607);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "MainForm";
            this.Text = "Collocation Experiment";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ProgressBar pbar;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.LinkLabel lnkLoadFreq;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtQuery;
        private System.Windows.Forms.WebBrowser wb;
        private System.Windows.Forms.RadioButton rbChSqCorr;
        private System.Windows.Forms.RadioButton rbLogLike;
        private System.Windows.Forms.TextBox txtMinFreq;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtRightWin;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtLeftWin;
        private System.Windows.Forms.TextBox txtMaxFreq;
        private System.Windows.Forms.Label label5;
    }
}

