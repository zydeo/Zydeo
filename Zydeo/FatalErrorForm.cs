using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ZD.Common;

namespace ZD
{
    public partial class FatalErrorForm : Form
    {
        private Point formOrigLoc = Point.Empty;
        private Point moveStart;

        public FatalErrorForm(ITextProvider tprov)
        {
            InitializeComponent();
            // We want 1px to be 1px at all resolutions
            pnlOuter.Padding = new Padding(1);
            // Update localized labels
            setTexts(tprov);

            // Moveable by header
            lblHeader.MouseDown += onHeaderMouseDown;
            lblHeader.MouseUp += onHeaderMouseUp;
            lblHeader.MouseMove += onHeaderMouseMove;
        }

        private void setTexts(ITextProvider tprov)
        {
            Text = tprov.GetString("FatalErrorFormHeader");
            lblHeader.Text = tprov.GetString("FatalErrorFormHeader");
            lblSorry.Text = tprov.GetString("FatalErrorFormSorry");
            lblWhy.Text = tprov.GetString("FatalErrorFormWhy");
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
