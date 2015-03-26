using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

namespace ZD.AU
{
    /// <summary>
    /// UI, running in user's name, for process of downloading update and installing it.
    /// </summary>
    internal partial class ZydeoUpdateForm : Form
    {
        private Point formOrigLoc = Point.Empty;
        private Point moveStart;

        public ZydeoUpdateForm()
        {
            InitializeComponent();

            // If we're in designer, done here
            if (Process.GetCurrentProcess().ProcessName == "devenv") return;

            // We want 1px to be 1px at all resolutions
            pnlOuter.Padding = new Padding(1);

            // Set image and icon
            Assembly a = Assembly.GetExecutingAssembly();
            var img = Image.FromStream(a.GetManifestResourceStream("ZD.AU.Resources.installer1.bmp"));
            pictureBox1.BackgroundImage = img;
            Icon = new Icon(a.GetManifestResourceStream("ZD.AU.Resources.ZydeoSetup.ico"));

            // Moveable by header
            lblHeader.MouseDown += onHeaderMouseDown;
            lblHeader.MouseUp += onHeaderMouseUp;
            lblHeader.MouseMove += onHeaderMouseMove;
        }

        /// <summary>
        /// Force-closes and disposes the window.
        /// </summary>
        public void ForceClose()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    Close();
                });
                return;
            }
            Close();
        }

        private void onHeaderMouseMove(object sender, MouseEventArgs e)
        {
            if (formOrigLoc == Point.Empty) return;
            Point mouseLoc = lblHeader.PointToScreen(e.Location);
            Point loc = formOrigLoc;
            loc.X += mouseLoc.X - moveStart.X;
            loc.Y += mouseLoc.Y - moveStart.Y;
            Location = loc;
        }

        private void onHeaderMouseUp(object sender, MouseEventArgs e)
        {
            formOrigLoc = Point.Empty;
        }

        private void onHeaderMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            moveStart = lblHeader.PointToScreen(e.Location);
            formOrigLoc = Location;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }
    }
}
