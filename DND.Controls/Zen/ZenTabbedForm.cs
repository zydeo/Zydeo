using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;


namespace DND.Controls
{
    public class ZenTabbedForm : Form, IZenControlOwner, IZenTabsChangedListener
    {
        private readonly int headerHeight;
        private readonly int innerPadding;
        private readonly float scale;
        private readonly object bufferLockObj = new object();
        private Bitmap dbuffer = null;
        private string header;
        private readonly List<ZenControl> zenControls = new List<ZenControl>();
        private ZenCloseControl ctrlClose;
        private ZenTabControl mainTabCtrl;
        private readonly List<ZenTabControl> contentTabControls = new List<ZenTabControl>();
        private bool controlsCreated = false;
        private ZenTab mainTab;
        private readonly ZenTabCollection tabs;
        private int activeTabIdx = 0;

        public ZenTabbedForm()
        {
            SuspendLayout();

            DoubleBuffered = false;
            FormBorderStyle = FormBorderStyle.None;

            Size = new Size(800, 300);
            //contentPanel = new Panel();
            //contentPanel.BackColor = ZenParams.PaddingBackColor;
            //contentPanel.BorderStyle = BorderStyle.None;
            //contentPanel.Location = new Point((int)ZenParams.InnerPadding, (int)(ZenParams.HeaderHeight + ZenParams.InnerPadding));
            //contentPanel.Size = new Size(
            //    Width - 2 * (int)ZenParams.InnerPadding,
            //    Height - (int)ZenParams.InnerPadding - (int)ZenParams.HeaderHeight);
            //Controls.Add(contentPanel);
            AutoScaleDimensions = new SizeF(6.0F, 13.0F);
            AutoScaleMode = AutoScaleMode.Font;
            scale = CurrentAutoScaleDimensions.Height / 13.0F;
            ResumeLayout();

            headerHeight = (int)(ZenParams.HeaderHeight * scale);
            innerPadding = (int)(ZenParams.InnerPadding * scale);

            createZenControls();
            tabs = new ZenTabCollection(this);
        }

        protected new float Scale
        {
            get { return scale; }
        }

        private Size ContentSize
        {
            get
            {
                return new Size(
                    Width - 2 * (int)ZenParams.InnerPadding,
                    Height - (int)ZenParams.InnerPadding - (int)ZenParams.HeaderHeight);
            }
        }

        private Point ContentLocation
        {
            get
            {
                return new Point((int)ZenParams.InnerPadding, (int)(ZenParams.HeaderHeight + ZenParams.InnerPadding));
            }
        }

        protected ZenTab MainTab
        {
            get { return mainTab; }
            set
            {
                if (mainTab != null)
                {
                    //Controls.Remove(mainTab);
                    mainTab = null;
                }
                mainTab = value;
                //mainTab.Dock = DockStyle.Fill;
                mainTab.Ctrl.AbsLocation = ContentLocation;
                mainTab.Ctrl.Size = ContentSize;
                //mainTab.Visible = false;
                //mainTab.BackColor = Color.White;
                //contentPanel.Controls.Add(mainTab);
            }
        }

        protected ZenTabCollection Tabs
        {
            get { return tabs; }
        }

        protected string Header
        {
            get { return header; }
            set { header = value; Invalidate(); }
        }

        /// <summary>
        /// Index of the active tab. 0 is first content tab; -1 is main tab.
        /// </summary>
        protected int ActiveTabIdx
        {
            get { return activeTabIdx; }
        }

        protected Size LogicalSize
        {
            set
            {
                float w = value.Width;
                float h = value.Height;
                Size s = new Size((int)(w * scale), (int)(h * scale));
                Size = s;
            }
        }

        public Point MousePositionAbs
        {
            get { return PointToClient(MousePosition); }
        }

        public Rectangle AbsRect
        {
            get { return new Rectangle(0, 0, Width, Height); }
        }

        void IZenTabsChangedListener.ZenTabsChanged()
        {
            // Remove old tab controls in header, re-add them
            List<ZenControl> toRemove = new List<ZenControl>();
            foreach (ZenControl zc in zenControls)
            {
                ZenTabControl ztc = zc as ZenTabControl;
                if (ztc == null || ztc.IsMain) continue;
                toRemove.Add(zc);
            }
            foreach (ZenControl zc in toRemove)
            {
                zenControls.Remove(zc);
                zc.Dispose();
            }
            contentTabControls.Clear();
            // Recreate all header tabs; add WinForms control to form's children
            int posx = mainTabCtrl.AbsLocation.X + mainTabCtrl.Width;
            for (int i = 0; i != tabs.Count; ++i)
            {
                ZenTabControl tc = new ZenTabControl(scale, this, false);
                tc.Text = tabs[i].Header;
                tc.LogicalSize = new Size(80, 30);
                tc.Size = new Size(tc.PreferredWidth, tc.Height);
                tc.AbsLocation = new Point(posx, headerHeight - tc.Height);
                posx += tc.Width;
                tc.MouseClick += tabCtrl_MouseClick;
                zenControls.Add(tc);
                contentTabControls.Add(tc);
            }
            // If this is the first content tab being added, this will be the active (visible) one
            if (tabs.Count == 1)
            {
                zenControls.Add(tabs[0].Ctrl);
                contentTabControls[0].Selected = true;
            }
            // Must arrange controls - so newly added, and perhaps displayed, content tab
            // gets sized and placed
            arrangeControls();
            // Redraw
            Invalidate();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (tabs.Count == 0 || mainTab == null)
                throw new InvalidOperationException("ZenTabbedForm must have main tab and at least one content tab set by the time it's loaded.");
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                foreach (ZenControl zc in zenControls) zc.Dispose();
                lock (bufferLockObj)
                {
                    if (dbuffer != null) { dbuffer.Dispose(); dbuffer = null; }
                }
            }
        }

        private void createZenControls()
        {
            ctrlClose = new ZenCloseControl(scale, this);
            ctrlClose.LogicalSize = new Size(40, 20);
            ctrlClose.AbsLocation = new Point(Width - ctrlClose.Width - innerPadding, 1);
            ctrlClose.MouseClick += ctrlClose_MouseClick;
            zenControls.Add(ctrlClose);

            mainTabCtrl = new ZenTabControl(scale, this, true);
            mainTabCtrl.Text = "Main";
            mainTabCtrl.LogicalSize = new Size(80, 30);
            mainTabCtrl.Size = new Size(mainTabCtrl.PreferredWidth, mainTabCtrl.Height);
            mainTabCtrl.AbsLocation = new Point(1, headerHeight - mainTabCtrl.Height);
            mainTabCtrl.MouseClick += tabCtrl_MouseClick;
            zenControls.Add(mainTabCtrl);

            controlsCreated = true;
        }

        private void arrangeControls()
        {
            if (!controlsCreated) return;
            
            // Resize and place active content tab, if any
            foreach (ZenTab zt in tabs)
            {
                if (!this.Created || zenControls.Contains(zt.Ctrl))
                {
                    zt.Ctrl.AbsLocation = new Point(innerPadding, headerHeight + innerPadding);
                    zt.Ctrl.Size = new Size(
                        Width - 2 * innerPadding,
                        Height - 2 * innerPadding - headerHeight);
                }
            }
            // Resize main tab, if active
            if (mainTab != null && (!this.Created || zenControls.Contains(mainTab.Ctrl)))
            {
                mainTab.Ctrl.AbsLocation = new Point(innerPadding, headerHeight + innerPadding);
                mainTab.Ctrl.Size = new Size(
                    Width - 2 * innerPadding,
                    Height - 2 * innerPadding - headerHeight);
            }

            ctrlClose.AbsLocation = new Point(Width - ctrlClose.Size.Width - innerPadding, 1);
        }

        private void ctrlClose_MouseClick(ZenControl sender)
        {
            Close();
        }


        private void tabCtrl_MouseClick(ZenControl sender)
        {
            ZenTabControl ztc = sender as ZenTabControl;
            // Click on active tab - nothing to do
            if (ztc.IsMain && activeTabIdx == -1) return;
            int idx = contentTabControls.IndexOf(ztc);
            if (idx == activeTabIdx) return;
            // Switching to main
            if (idx == -1)
            {
                contentTabControls[activeTabIdx].Selected = false;
                mainTabCtrl.Selected = true;
                zenControls.Remove(tabs[activeTabIdx].Ctrl);
                zenControls.Add(mainTab.Ctrl);
                activeTabIdx = -1;
            }
            // Switching away to a content tab
            else
            {
                mainTabCtrl.Selected = false;
                contentTabControls[idx].Selected = true;
                zenControls.Remove(mainTab.Ctrl);
                zenControls.Add(tabs[idx].Ctrl);
                activeTabIdx = idx;
            }
            // Newly active contol still has old size if window was resized
            arrangeControls();
            // Refresh
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            lock (bufferLockObj)
            {
                if (dbuffer != null)
                {
                    dbuffer.Dispose();
                    dbuffer = null;
                }
            }
            base.OnSizeChanged(e);
            arrangeControls();
            Invalidate();
        }

        void IZenControlOwner.ControlAdded(ZenControl ctrl)
        {
            // We do nothing here: main form creates its own controls and keeps track of them
            // explicitly (so we can pretend content controls are not there)
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // NOP!
        }

        void IZenControlOwner.MakeCtrlPaint(ZenControl ctrl, bool needBackground, RenderMode rm)
        {
            lock (bufferLockObj)
            {
                if (dbuffer == null) return;
                using (Graphics g = Graphics.FromImage(dbuffer))
                {
                    if (needBackground) doPaint(g);
                    else ctrl.DoPaint(g);
                }
            }
            if (rm == RenderMode.None) return;
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    if (rm == RenderMode.Invalidate) Invalidate();
                    else Refresh();
                });
            }
            else
            {
                if (rm == RenderMode.Invalidate) Invalidate();
                else Refresh();
            }
        }

        private void doPaintMyBackground(Graphics g)
        {
            using (Brush b = new SolidBrush(ZenParams.HeaderBackColor))
            {
                g.FillRectangle(b, 0, 0, Width, headerHeight);
            }
            // For content tab and main tab, pad with different color
            Color colPad = ZenParams.PaddingBackColor;
            if (activeTabIdx == -1) colPad = Color.White;
            using (Brush b = new SolidBrush(colPad))
            {
                g.FillRectangle(b, innerPadding, headerHeight, Width - 2 * innerPadding, innerPadding);
                g.FillRectangle(b, 0, headerHeight, innerPadding, Height - headerHeight);
                g.FillRectangle(b, Width - innerPadding, headerHeight, innerPadding, Height - headerHeight);
                g.FillRectangle(b, innerPadding, Height - innerPadding - 1, Width - 2 * innerPadding, innerPadding);
            }
            using (Pen p = new Pen(ZenParams.BorderColor))
            {
                p.Width = 1;
                g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
            }
        }

        private void doPaint(Graphics g)
        {
            doPaintMyBackground(g);
            float x = contentTabControls[contentTabControls.Count - 1].AbsRight;
            x += ZenParams.HeaderTabPadding * 3.0F;
            float y = 7.0F * scale;
            RectangleF rect = new RectangleF(x, y, ctrlClose.AbsLeft - x, headerHeight - y);
            using (Brush b = new SolidBrush(Color.Black))
            using (Font f = new Font(new FontFamily(ZenParams.HeaderFontFamily), ZenParams.HeaderFontSize))
            {
                StringFormat sf = StringFormat.GenericDefault;
                sf.Trimming = StringTrimming.Word;
                sf.FormatFlags |= StringFormatFlags.NoWrap;
                g.DrawString(header, f, b, rect, sf);
            }
            // Draw my zen controls
            foreach (ZenControl ctrl in zenControls) ctrl.DoPaint(g);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            lock (bufferLockObj)
            {
                // Do all the remaining drawing through my own hand-made double-buffering for speed
                if (dbuffer == null)
                {
                    dbuffer = new Bitmap(Width, Height);
                    using (Graphics g = Graphics.FromImage(dbuffer))
                    {
                        doPaint(g);
                    }
                }
                e.Graphics.DrawImageUnscaled(dbuffer, 0, 0);
            }
        }

        private enum DragMode
        {
            None,
            ResizeW,
            ResizeE,
            ResizeN,
            ResizeS,
            ResizeNW,
            ResizeSE,
            ResizeNE,
            ResizeSW,
            Move
        }

        private ZenControl zenCtrlWithMouse = null;
        private DragMode dragMode = DragMode.None;
        private Point dragStart;
        private Point formBeforeDragLocation;
        private Size formBeforeDragSize;

        private DragMode getDragArea(Point p)
        {
            Rectangle rHeader = new Rectangle(
                innerPadding,
                innerPadding,
                Width - 2 * innerPadding,
                headerHeight - innerPadding);
            if (rHeader.Contains(p))
                return DragMode.Move;
            Rectangle rEast = new Rectangle(
                Width - innerPadding,
                0,
                innerPadding,
                Height);
            if (rEast.Contains(p))
            {
                if (p.Y < 2 * innerPadding) return DragMode.ResizeNE;
                if (p.Y > Height - 2 * innerPadding) return DragMode.ResizeSE;
                return DragMode.ResizeE;
            }
            Rectangle rWest = new Rectangle(
                0,
                0,
                innerPadding,
                Height);
            if (rWest.Contains(p))
            {
                if (p.Y < 2 * innerPadding) return DragMode.ResizeNW;
                if (p.Y > Height - 2 * innerPadding) return DragMode.ResizeSW;
                return DragMode.ResizeW;
            }
            Rectangle rNorth = new Rectangle(
                0,
                0,
                Width,
                innerPadding);
            if (rNorth.Contains(p))
            {
                if (p.X < 2 * innerPadding) return DragMode.ResizeNW;
                if (p.X > Width - 2 * innerPadding) return DragMode.ResizeNE;
                return DragMode.ResizeN;
            }
            Rectangle rSouth = new Rectangle(
                0,
                Height - innerPadding,
                Width,
                innerPadding);
            if (rSouth.Contains(p))
            {
                if (p.X < 2 * innerPadding) return DragMode.ResizeSW;
                if (p.X > Width - 2 * innerPadding) return DragMode.ResizeSE;
                return DragMode.ResizeS;
            }
            return DragMode.None;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Over any control? Not our job to handle.
            ZenControl ctrl = getControl(e.Location);
            if (ctrl != null)
            {
                ctrl.DoMouseDown(translateToControl(ctrl, e.Location), e.Button);
                return;
            }

            var area = getDragArea(e.Location);
            if (area == DragMode.Move)
            {
                dragMode = DragMode.Move;
                dragStart = PointToScreen(e.Location);
                formBeforeDragLocation = Location;
            }
            else if (area != DragMode.None && area != DragMode.Move)
            {
                dragMode = area;
                dragStart = PointToScreen(e.Location);
                formBeforeDragSize = Size;
                formBeforeDragLocation = Location;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point loc = PointToScreen(e.Location);
            if (dragMode == DragMode.Move)
            {
                int dx = loc.X - dragStart.X;
                int dy = loc.Y - dragStart.Y;
                Point newLocation = new Point(formBeforeDragLocation.X + dx, formBeforeDragLocation.Y + dy);
                ((Form)TopLevelControl).Location = newLocation;
                return;
            }
            else if (dragMode == DragMode.ResizeE)
            {
                int dx = loc.X - dragStart.X;
                Size = new Size(formBeforeDragSize.Width + dx, formBeforeDragSize.Height);
                Refresh();
                return;
            }
            else if (dragMode == DragMode.ResizeW)
            {
                int dx = loc.X - dragStart.X;
                Left = formBeforeDragLocation.X + dx;
                Size = new Size(formBeforeDragSize.Width - dx, formBeforeDragSize.Height);
                Refresh();
                return;
            }
            else if (dragMode == DragMode.ResizeN)
            {
                int dy = loc.Y - dragStart.Y;
                Top = formBeforeDragLocation.Y + dy;
                Size = new Size(formBeforeDragSize.Width, formBeforeDragSize.Height - dy);
                Refresh();
                return;
            }
            else if (dragMode == DragMode.ResizeS)
            {
                int dy = loc.Y - dragStart.Y;
                Size = new Size(formBeforeDragSize.Width, formBeforeDragSize.Height + dy);
                Refresh();
                return;
            }
            else if (dragMode == DragMode.ResizeNW)
            {
                int dx = loc.X - dragStart.X;
                int dy = loc.Y - dragStart.Y;
                Size = new Size(formBeforeDragSize.Width - dx, formBeforeDragSize.Height - dy);
                Location = new Point(formBeforeDragLocation.X + dx, formBeforeDragLocation.Y + dy);
                Refresh();
                return;
            }
            else if (dragMode == DragMode.ResizeSE)
            {
                int dx = loc.X - dragStart.X;
                int dy = loc.Y - dragStart.Y;
                Size = new Size(formBeforeDragSize.Width + dx, formBeforeDragSize.Height + dy);
                Refresh();
                return;
            }
            else if (dragMode == DragMode.ResizeNE)
            {
                int dx = loc.X - dragStart.X;
                int dy = loc.Y - dragStart.Y;
                Size = new Size(formBeforeDragSize.Width + dx, formBeforeDragSize.Height - dy);
                Location = new Point(formBeforeDragLocation.X, formBeforeDragLocation.Y + dy);
                Refresh();
                return;
            }
            else if (dragMode == DragMode.ResizeSW)
            {
                int dx = loc.X - dragStart.X;
                int dy = loc.Y - dragStart.Y;
                Size = new Size(formBeforeDragSize.Width - dx, formBeforeDragSize.Height + dy);
                Location = new Point(formBeforeDragLocation.X + dx, formBeforeDragLocation.Y);
                Refresh();
                return;
            }

            // Over any controls? Not our job to handle.
            ZenControl ctrl = getControl(e.Location);
            if (ctrl != null)
            {
                Cursor = Cursors.Arrow;
                if (zenCtrlWithMouse != ctrl)
                {
                    if (zenCtrlWithMouse != null) zenCtrlWithMouse.DoMouseLeave();
                    ctrl.DoMouseEnter();
                    zenCtrlWithMouse = ctrl;
                }
                ctrl.DoMouseMove(translateToControl(ctrl, e.Location), e.Button);
                return;
            }
            else if (zenCtrlWithMouse != null)
            {
                zenCtrlWithMouse.DoMouseLeave();
                zenCtrlWithMouse = null;
            }

            var area = getDragArea(e.Location);
            if (area == DragMode.ResizeW || area == DragMode.ResizeE)
                Cursor = Cursors.SizeWE;
            else if (area == DragMode.ResizeN || area == DragMode.ResizeS)
                Cursor = Cursors.SizeNS;
            else if (area == DragMode.ResizeNW || area == DragMode.ResizeSE)
                Cursor = Cursors.SizeNWSE;
            else if (area == DragMode.ResizeNE || area == DragMode.ResizeSW)
                Cursor = Cursors.SizeNESW;
            else Cursor = Cursors.Arrow;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            dragMode = DragMode.None;

            ZenControl ctrl = getControl(e.Location);
            if (ctrl != null) ctrl.DoMouseUp(translateToControl(ctrl, e.Location), e.Button);
        }

        private ZenControl getControl(Point p)
        {
            foreach (ZenControl ctrl in zenControls)
                if (ctrl.Contains(p)) return ctrl;
            return null;
        }

        private Point translateToControl(ZenControl ctrl, Point p)
        {
            int x = p.X - ctrl.AbsLocation.X;
            int y = p.Y - ctrl.AbsLocation.Y;
            return new Point(x, y);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            ZenControl ctrl = getControl(e.Location);
            if (ctrl != null) ctrl.DoMouseClick(translateToControl(ctrl, e.Location), e.Button);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Point loc = PointToClient(MousePosition);
            ZenControl ctrl = getControl(loc);
            if (ctrl != null)
            {
                if (zenCtrlWithMouse != ctrl)
                {
                    if (zenCtrlWithMouse != null) zenCtrlWithMouse.DoMouseLeave();
                    ctrl.DoMouseEnter();
                    zenCtrlWithMouse = ctrl;
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (zenCtrlWithMouse != null)
            {
                zenCtrlWithMouse.DoMouseLeave();
                zenCtrlWithMouse = null;
            }
            if (dragMode == DragMode.None || dragMode == DragMode.Move)
            {
                Cursor = Cursors.Arrow;
            }
        }
    }
}
