namespace ZD.FontTest
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
            this.pnlCanvas = new System.Windows.Forms.Panel();
            this.canvas = new ZD.FontTest.FontTestCtrl();
            this.rbArphic = new System.Windows.Forms.RadioButton();
            this.gbParams = new System.Windows.Forms.GroupBox();
            this.btnGo = new System.Windows.Forms.Button();
            this.txtSz = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.rbNoto = new System.Windows.Forms.RadioButton();
            this.rbSimp = new System.Windows.Forms.RadioButton();
            this.pnlLayout = new System.Windows.Forms.TableLayoutPanel();
            this.pnlSmart = new System.Windows.Forms.Panel();
            this.pretty = new ZD.FontTest.PrettyTestCtrl();
            this.pnlCanvas.SuspendLayout();
            this.gbParams.SuspendLayout();
            this.pnlLayout.SuspendLayout();
            this.pnlSmart.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlCanvas
            // 
            this.pnlCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlCanvas.BackColor = System.Drawing.Color.LavenderBlush;
            this.pnlCanvas.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlCanvas.Controls.Add(this.canvas);
            this.pnlCanvas.Location = new System.Drawing.Point(0, 0);
            this.pnlCanvas.Margin = new System.Windows.Forms.Padding(0);
            this.pnlCanvas.Name = "pnlCanvas";
            this.pnlCanvas.Size = new System.Drawing.Size(840, 190);
            this.pnlCanvas.TabIndex = 0;
            // 
            // canvas
            // 
            this.canvas.BackColor = System.Drawing.Color.Wheat;
            this.canvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.canvas.Location = new System.Drawing.Point(0, 0);
            this.canvas.Name = "canvas";
            this.canvas.Size = new System.Drawing.Size(836, 186);
            this.canvas.TabIndex = 0;
            this.canvas.Text = "我英语给您";
            // 
            // rbArphic
            // 
            this.rbArphic.Checked = true;
            this.rbArphic.Location = new System.Drawing.Point(8, 16);
            this.rbArphic.Name = "rbArphic";
            this.rbArphic.Size = new System.Drawing.Size(104, 24);
            this.rbArphic.TabIndex = 1;
            this.rbArphic.TabStop = true;
            this.rbArphic.Text = "AR PL UKai";
            this.rbArphic.UseVisualStyleBackColor = true;
            // 
            // gbParams
            // 
            this.gbParams.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbParams.Controls.Add(this.btnGo);
            this.gbParams.Controls.Add(this.txtSz);
            this.gbParams.Controls.Add(this.label1);
            this.gbParams.Controls.Add(this.rbNoto);
            this.gbParams.Controls.Add(this.rbSimp);
            this.gbParams.Controls.Add(this.rbArphic);
            this.gbParams.Location = new System.Drawing.Point(8, 8);
            this.gbParams.Name = "gbParams";
            this.gbParams.Size = new System.Drawing.Size(840, 44);
            this.gbParams.TabIndex = 2;
            this.gbParams.TabStop = false;
            this.gbParams.Text = "Params";
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(368, 16);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(75, 24);
            this.btnGo.TabIndex = 6;
            this.btnGo.Text = "&Go";
            this.btnGo.UseVisualStyleBackColor = true;
            // 
            // txtSz
            // 
            this.txtSz.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSz.Location = new System.Drawing.Point(316, 16);
            this.txtSz.Name = "txtSz";
            this.txtSz.Size = new System.Drawing.Size(52, 24);
            this.txtSz.TabIndex = 5;
            this.txtSz.Text = "48";
            this.txtSz.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(236, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 24);
            this.label1.TabIndex = 4;
            this.label1.Text = "Size (pt)";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rbNoto
            // 
            this.rbNoto.Location = new System.Drawing.Point(172, 16);
            this.rbNoto.Name = "rbNoto";
            this.rbNoto.Size = new System.Drawing.Size(69, 24);
            this.rbNoto.TabIndex = 3;
            this.rbNoto.TabStop = true;
            this.rbNoto.Text = "Noto";
            this.rbNoto.UseVisualStyleBackColor = true;
            // 
            // rbSimp
            // 
            this.rbSimp.Location = new System.Drawing.Point(96, 16);
            this.rbSimp.Name = "rbSimp";
            this.rbSimp.Size = new System.Drawing.Size(82, 24);
            this.rbSimp.TabIndex = 2;
            this.rbSimp.TabStop = true;
            this.rbSimp.Text = "Simplified";
            this.rbSimp.UseVisualStyleBackColor = true;
            // 
            // pnlLayout
            // 
            this.pnlLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlLayout.ColumnCount = 1;
            this.pnlLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnlLayout.Controls.Add(this.pnlCanvas, 0, 0);
            this.pnlLayout.Controls.Add(this.pnlSmart, 0, 1);
            this.pnlLayout.Location = new System.Drawing.Point(8, 60);
            this.pnlLayout.Name = "pnlLayout";
            this.pnlLayout.RowCount = 2;
            this.pnlLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnlLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnlLayout.Size = new System.Drawing.Size(840, 380);
            this.pnlLayout.TabIndex = 3;
            // 
            // pnlSmart
            // 
            this.pnlSmart.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlSmart.Controls.Add(this.pretty);
            this.pnlSmart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSmart.Location = new System.Drawing.Point(0, 190);
            this.pnlSmart.Margin = new System.Windows.Forms.Padding(0);
            this.pnlSmart.Name = "pnlSmart";
            this.pnlSmart.Size = new System.Drawing.Size(840, 190);
            this.pnlSmart.TabIndex = 1;
            // 
            // pretty
            // 
            this.pretty.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pretty.Location = new System.Drawing.Point(0, 0);
            this.pretty.Name = "pretty";
            this.pretty.Size = new System.Drawing.Size(836, 186);
            this.pretty.TabIndex = 0;
            this.pretty.Text = "prettyTestCtrl1";
            // 
            // MainForm
            // 
            this.AcceptButton = this.btnGo;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(856, 449);
            this.Controls.Add(this.pnlLayout);
            this.Controls.Add(this.gbParams);
            this.Name = "MainForm";
            this.Text = "We\'re in great form";
            this.pnlCanvas.ResumeLayout(false);
            this.gbParams.ResumeLayout(false);
            this.gbParams.PerformLayout();
            this.pnlLayout.ResumeLayout(false);
            this.pnlSmart.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlCanvas;
        private System.Windows.Forms.RadioButton rbArphic;
        private System.Windows.Forms.GroupBox gbParams;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.TextBox txtSz;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton rbNoto;
        private System.Windows.Forms.RadioButton rbSimp;
        private FontTestCtrl canvas;
        private System.Windows.Forms.TableLayoutPanel pnlLayout;
        private System.Windows.Forms.Panel pnlSmart;
        private PrettyTestCtrl pretty;
    }
}

