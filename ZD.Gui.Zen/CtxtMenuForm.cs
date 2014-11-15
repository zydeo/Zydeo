using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ZD.Gui.Zen
{
    internal class CtxtMenuForm : Form
    {
        /// <summary>
        /// Delegate to handle context menu's notification when it is closed.
        /// </summary>
        public delegate void CtxtMenuClosedDelegate();

        public CtxtMenuForm()
        {
            // Configure form - borderless etc.
            SuspendLayout();
            AutoScaleDimensions = new System.Drawing.SizeF(6.0F, 13.0F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.LightBlue;
            ClientSize = new System.Drawing.Size(278, 244);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Name = "CtxtMenuForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "CtxtMenuForm";
            TopMost = true;
            ResumeLayout(false);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x00020000;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                const int WS_EX_NOACTIVATE = 0x08000000;
                const int WS_POPUP = (int)0x8000000;
                CreateParams cp = base.CreateParams;
                cp.Style = WS_POPUP;
                cp.ClassStyle = CS_DROPSHADOW;
                cp.ExStyle = WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                cp.Parent = IntPtr.Zero;
                return cp;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Dispose();
        }

        public void DoKey(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Close();
            else return;
        }
    }
}
