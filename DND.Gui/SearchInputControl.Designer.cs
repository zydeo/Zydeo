namespace DND.Gui
{
    partial class SearchInputControl
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
            this.txtInput = new System.Windows.Forms.TextBox();
            this.pnlBg = new System.Windows.Forms.Panel();
            this.pnlBg.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtInput
            // 
            this.txtInput.BackColor = System.Drawing.Color.White;
            this.txtInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInput.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtInput.Location = new System.Drawing.Point(4, 4);
            this.txtInput.Margin = new System.Windows.Forms.Padding(0);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(290, 23);
            this.txtInput.TabIndex = 0;
            // 
            // pnlBg
            // 
            this.pnlBg.BackColor = System.Drawing.Color.White;
            this.pnlBg.Controls.Add(this.txtInput);
            this.pnlBg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBg.Location = new System.Drawing.Point(0, 0);
            this.pnlBg.Margin = new System.Windows.Forms.Padding(0);
            this.pnlBg.Name = "pnlBg";
            this.pnlBg.Padding = new System.Windows.Forms.Padding(4);
            this.pnlBg.Size = new System.Drawing.Size(298, 30);
            this.pnlBg.TabIndex = 1;
            // 
            // SearchInputControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.pnlBg);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "SearchInputControl";
            this.Size = new System.Drawing.Size(298, 30);
            this.pnlBg.ResumeLayout(false);
            this.pnlBg.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.Panel pnlBg;
    }
}
