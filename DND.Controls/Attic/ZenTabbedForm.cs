using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace DND.Controls
{
    public partial class ZenTabbedForm : Form
    {
        private float scaleFactor;
        private ZenContentPanel pnlZenFrame;
        private ZenFormHeader headerCtrl;

        public ZenTabbedForm()
        {
            InitializeComponent();
            Text = "ZenTabbedForm";
            scaleFactor = AutoScaleDimensions.Height / 13.0F;
        }

        public override string Text
        {
            get
            {
                if (headerCtrl == null) return "ZenTabbedForm";
                return headerCtrl.HeaderText;
            }
            set { headerCtrl.HeaderText = value; }
        }

        public new FormBorderStyle FormBorderStyle
        {
            get { return base.FormBorderStyle; }
            set { base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None; }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            pnlZenFrame.MouseMove += pnlZenFrame_MouseMove;
            pnlZenFrame.MouseLeave += pnlZenFrame_MouseLeave;
            pnlZenFrame.MouseDown += pnlZenFrame_MouseDown;
            pnlZenFrame.MouseUp += pnlZenFrame_MouseUp;
            base.OnLoad(e);
        }

        enum ResizeModes
        {
            WE,
            NWSE,
            NS
        }
        bool captureResize = false;
        ResizeModes captureMode;
        Point captureStartPoint;
        Size captureSizeBefore;

        void pnlZenFrame_MouseUp(object sender, MouseEventArgs e)
        {
            captureResize = false;
        }

        void pnlZenFrame_MouseDown(object sender, MouseEventArgs e)
        {
            int th = (int)(4.0F * scaleFactor);
            Rectangle rR = new Rectangle(pnlZenFrame.Width - th, 0, th, pnlZenFrame.Height - th);
            Rectangle rBR = new Rectangle(pnlZenFrame.Width - th, pnlZenFrame.Height - th, th, th);
            Rectangle rB = new Rectangle(0, pnlZenFrame.Height - th, pnlZenFrame.Width - th, th);
            if (rR.Contains(e.Location))
            {
                captureResize = true;
                captureStartPoint = e.Location;
                captureMode = ResizeModes.WE;
                captureSizeBefore = Size;
            }
            else if (rBR.Contains(e.Location))
            {
                captureResize = true;
                captureStartPoint = e.Location;
                captureMode = ResizeModes.NWSE;
                captureSizeBefore = Size;
            }
            else if (rB.Contains(e.Location))
            {
                captureResize = true;
                captureStartPoint = e.Location;
                captureMode = ResizeModes.NS;
                captureSizeBefore = Size;
            }
        }

        void pnlZenFrame_MouseLeave(object sender, EventArgs e)
        {
            if (!captureResize) Cursor = Cursors.Arrow;
        }

        void pnlZenFrame_MouseMove(object sender, MouseEventArgs e)
        {
            if (captureResize)
            {
                if (captureMode == ResizeModes.WE)
                {
                    int dx = e.X - captureStartPoint.X;
                    Size = new Size(captureSizeBefore.Width + dx, captureSizeBefore.Height);
                }
                else if (captureMode == ResizeModes.NWSE)
                {
                    int dx = e.X - captureStartPoint.X;
                    int dy = e.Y - captureStartPoint.Y;
                    Size = new Size(captureSizeBefore.Width + dx, captureSizeBefore.Height + dy);
                }
                else if (captureMode == ResizeModes.NS)
                {
                    int dy = e.Y - captureStartPoint.Y;
                    Size = new Size(captureSizeBefore.Width, captureSizeBefore.Height + dy);
                }
            }
            else
            {
                int th = (int)(4.0F * scaleFactor);
                Rectangle rR = new Rectangle(pnlZenFrame.Width - th, 0, th, pnlZenFrame.Height - th);
                Rectangle rBR = new Rectangle(pnlZenFrame.Width - th, pnlZenFrame.Height - th, th, th);
                Rectangle rB = new Rectangle(0, pnlZenFrame.Height - th, pnlZenFrame.Width - th, th);
                if (rR.Contains(e.Location))
                    Cursor = Cursors.SizeWE;
                else if (rBR.Contains(e.Location))
                    Cursor = Cursors.SizeNWSE;
                else if (rB.Contains(e.Location))
                    Cursor = Cursors.SizeNS;
                else
                {
                    Cursor = Cursors.Arrow;
                }
            }
        }
    }
}
