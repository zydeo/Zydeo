using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DND.Controls
{
    public partial class ZenFormHeader : UserControl
    {
        private float scaleFactor = 0;
        private bool initializing = true;
        private Size lastSize;

        // Moving parent form
        private Point mouseDown;
        private Point formLocation;
        private bool capture;

        public ZenFormHeader()
        {
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            AutoScaleDimensions = new System.Drawing.SizeF(6.0F, 13.0F);
            InitializeComponent();
            initializing = false;
        }

        public string HeaderText
        {
            get { return lblHeader.Text; }
            set { lblHeader.Text = value; }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            int dx = Size.Width - lastSize.Width;
            int dy = Size.Height - lastSize.Height;
            lastSize = Size;
            if (initializing) return;
            if (scaleFactor == 0)
            {
                base.OnSizeChanged(e);
                return;
            }
            foreach (Control c in Controls)
            {
                int x = c.Left;
                int y = c.Top;
                int w = c.Width;
                int h = c.Height;
                if ((c.Anchor & AnchorStyles.Right) != 0)
                {
                    if ((c.Anchor & AnchorStyles.Left) != 0) w += dx;
                    else x += dx;
                }
                if ((c.Anchor & AnchorStyles.Bottom) != 0)
                {
                    if ((c.Anchor & AnchorStyles.Top) != 0) h += dy;
                    else y += dy;
                }
                c.Location = new Point(x, y);
                c.Size = new Size(w, h);
            }
        }

        private void arrangeAll()
        {
            pnlClose.Top = 0;
            pnlClose.Left = Width - (int)(4.0F * scaleFactor) - pnlClose.Width;
            lblHeader.Left = (int)(52.0F * scaleFactor);
            lblHeader.Width = pnlClose.Left - 1 - lblHeader.Left;
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            if (scaleFactor == 0 && ParentForm != null)
            {
                scaleFactor = ParentForm.AutoScaleDimensions.Height / AutoScaleDimensions.Height;
                // Resize all controls
                foreach (Control c in Controls)
                {
                    SizeF oldSize = new SizeF(c.Width, c.Height);
                    SizeF newSize = new SizeF(oldSize.Width * scaleFactor, oldSize.Height * scaleFactor);
                    c.Size = new Size((int)newSize.Width, (int)newSize.Height);
                    c.Font = new Font(c.Font.FontFamily, c.Font.Size * scaleFactor);
                }
                // Manually arrange all controls
                arrangeAll();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            pnlClose.MouseEnter += pnlClose_MouseEnter;
            pnlClose.MouseLeave += pnlClose_MouseLeave;
            pnlClose.Click += pnlClose_Click;
            lblHeader.MouseDown += lblHeader_MouseDown;
            lblHeader.MouseMove += lblHeader_MouseMove;
            lblHeader.MouseUp += lblHeader_MouseUp;
            base.OnLoad(e);
        }

        void lblHeader_MouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        void lblHeader_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        void lblHeader_MouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        void pnlClose_Click(object sender, EventArgs e)
        {
            ParentForm.Close();
        }

        void pnlClose_MouseLeave(object sender, EventArgs e)
        {
            pnlClose.BackColor = Color.DarkRed;
        }

        void pnlClose_MouseEnter(object sender, EventArgs e)
        {
            pnlClose.BackColor = Color.Red;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            capture = true;
            mouseDown = e.Location;
            formLocation = ((Form)TopLevelControl).Location;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            capture = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (capture)
            {
                int dx = e.Location.X - mouseDown.X;
                int dy = e.Location.Y - mouseDown.Y;
                Point newLocation = new Point(formLocation.X + dx, formLocation.Y + dy);
                ((Form)TopLevelControl).Location = newLocation;
                formLocation = newLocation;
            }
        }

    }
}
