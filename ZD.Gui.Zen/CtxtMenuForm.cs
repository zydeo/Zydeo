using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace ZD.Gui.Zen
{
    /// <summary>
    /// A borderless topmost pop-up form hosting a context menu.
    /// </summary>
    internal class CtxtMenuForm : Form, IMessageFilter
    {
        /// <summary>
        /// The actual context menu UI I'm showing.
        /// </summary>
        private readonly ICtxtMenuControl ctxtMenuControl;

        /// <summary>
        /// True if mouse, when last seen, was over form; false otherwise.
        /// </summary>
        private bool mouseOverForm = false;

        /// <summary>
        /// Ctor: takes ownership of actual context menu UI.
        /// </summary>
        /// <param name="ctxtMenuControl">The context menu UI.</param>
        public CtxtMenuForm(ICtxtMenuControl ctxtMenuControl)
        {
            this.ctxtMenuControl = ctxtMenuControl;
            SuspendLayout();
            // Configure form - borderless etc.
            AutoScaleDimensions = new System.Drawing.SizeF(6.0F, 13.0F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = ZenParams.WindowColor;
            ClientSize = new System.Drawing.Size(278, 244);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            Name = "CtxtMenuForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "CtxtMenuForm";
            TopMost = true;
            // Add UI control to children, resize to accommodate it.
            ctxtMenuControl.AsUserControl.Location = Point.Empty;
            Controls.Add(ctxtMenuControl.AsUserControl);
            ctxtMenuControl.AssumeSize();
            // KK, done.
            //ResumeLayout(false); // No! that screws up size.
            Size = ctxtMenuControl.AsUserControl.Size;
            Application.AddMessageFilter(this);
        }

        /// <summary>
        /// Overridden for drop shadow.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x00020000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        /// <summary>
        /// Overridden to prevent window from stealing focus when shown.
        /// </summary>
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        /// <summary>
        /// Handles WM_MOUSEACTIVATE to prevent window from ever getting activated (stealing focus from main form).
        /// </summary>
        protected override void DefWndProc(ref Message m)
        {
            const int WM_MOUSEACTIVATE = 0x21;
            const int MA_NOACTIVATE = 0x0003;

            switch (m.Msg)
            {
                case WM_MOUSEACTIVATE:
                    m.Result = (IntPtr)MA_NOACTIVATE;
                    return;
            }
            base.DefWndProc(ref m);
        }

        /// <summary>
        /// Gets the actual UI control the form is showing.
        /// </summary>
        public ICtxtMenuControl CtxtMenuControl
        {
            get { return ctxtMenuControl; }
        }

        /// <summary>
        /// Self-disposes when context menu is closed.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (!IsDisposed)
            {
                Application.RemoveMessageFilter(this);
                Dispose();
            }
        }

        /// <summary>
        /// Catches and forwards mouse move and click events to actual UI control.
        /// </summary>
        public bool PreFilterMessage(ref Message m)
        {
            const int WM_MOUSEMOVE = 0x200;
            const int WM_LBUTTONDOWN = 0x201;
            if (m.Msg == WM_MOUSEMOVE)
            {
                Point pt = PointToClient(MousePosition);
                bool overForm = ClientRectangle.Contains(pt);
                if (!overForm && mouseOverForm)
                {
                    mouseOverForm = false;
                    ctxtMenuControl.DoMouseLeave();
                    return false;
                }
                if (overForm)
                {
                    mouseOverForm = true;
                    Point ptControl = new Point(
                        pt.X - ctxtMenuControl.AsUserControl.Left,
                        pt.Y - ctxtMenuControl.AsUserControl.Top);
                    ctxtMenuControl.DoMouseMove(ptControl);
                    return true;
                }
                return false;
            }
            else if (m.Msg == WM_LBUTTONDOWN)
            {
                Point pt = PointToClient(MousePosition);
                if (!ClientRectangle.Contains(pt)) return false;
                Point ptControl = new Point(
                    pt.X - ctxtMenuControl.AsUserControl.Left,
                    pt.Y - ctxtMenuControl.AsUserControl.Top);
                ctxtMenuControl.DoMouseClick(ptControl);
                return true;
            }
            return false;
        }
    }
}
