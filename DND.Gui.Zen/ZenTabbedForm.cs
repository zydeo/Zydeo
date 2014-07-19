using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;


namespace DND.Gui.Zen
{
    public class ZenTabbedForm : ZenControlBase, IDisposable, IZenTabsChangedListener
    {
        private readonly ZenWinForm form;

        private readonly int headerHeight;
        private readonly int innerPadding;
        private string header;
        private ZenCloseControl ctrlClose;
        private ZenTabControl mainTabCtrl;
        private readonly List<ZenTabControl> contentTabControls = new List<ZenTabControl>();
        private bool controlsCreated = false;
        private ZenTab mainTab;
        private readonly ZenTabCollection tabs;
        private int activeTabIdx = 0;

        public ZenTabbedForm()
            : base(null)
        {
            form = new ZenWinForm(DoPaint);

            headerHeight = (int)(ZenParams.HeaderHeight * Scale);
            innerPadding = (int)(ZenParams.InnerPadding * Scale);

            createZenControls();
            tabs = new ZenTabCollection(this);

            form.SizeChanged += onFormSizeChanged;
            form.MouseDown += onFormMouseDown;
            form.MouseMove += onFormMouseMove;
            form.MouseUp += onFormMouseUp;
            form.MouseClick += onFormMouseClick;
            form.MouseEnter += onFormMouseEnter;
            form.MouseLeave += onFormMouseLeave;
            form.FormClosed += onFormClosed;
        }

        public Form WinForm
        {
            get { return form; }
        }

        public override float Scale
        {
            get { return form.Scale; }
        }

        private Size ContentSize
        {
            get
            {
                return new Size(
                    form.Width - 2 * (int)ZenParams.InnerPadding,
                    form.Height - (int)ZenParams.InnerPadding - (int)ZenParams.HeaderHeight);
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
                mainTab = value;
                mainTab.Ctrl.AbsLocation = ContentLocation;
                mainTab.Ctrl.Size = ContentSize;
                mainTabCtrl.Text = mainTab.Header;
            }
        }

        protected ZenTabCollection Tabs
        {
            get { return tabs; }
        }

        protected string Header
        {
            get { return header; }
            set { header = value; doRepaint(); form.Invalidate(); }
        }

        /// <summary>
        /// Index of the active tab. 0 is first content tab; -1 is main tab.
        /// </summary>
        protected int ActiveTabIdx
        {
            get { return activeTabIdx; }
        }

        public override sealed Size LogicalSize
        {
            set
            {
                float w = value.Width;
                float h = value.Height;
                Size s = new Size((int)(w * Scale), (int)(h * Scale));
                form.Size = s;
            }
            get
            {
                Rectangle rect = form.DisplayRectangle;
                return new Size((int)(rect.Width / Scale), (int)(rect.Height / Scale));
            }
        }

        protected override sealed Point MousePositionAbs
        {
            get { return form.PointToClient(form.MousePosition); }
        }

        public override sealed Rectangle AbsRect
        {
            get { return new Rectangle(0, 0, form.Width, form.Height); }
        }

        void IZenTabsChangedListener.ZenTabsChanged()
        {
            // Remove old tab controls in header, re-add them
            List<ZenControl> toRemove = new List<ZenControl>();
            foreach (ZenControl zc in ZenChildren)
            {
                ZenTabControl ztc = zc as ZenTabControl;
                if (ztc == null || ztc.IsMain) continue;
                toRemove.Add(zc);
            }
            foreach (ZenControl zc in toRemove)
            {
                RemoveChild(zc);
                zc.Dispose();
            }
            contentTabControls.Clear();
            // Recreate all header tabs; add content control to form's children
            int posx = mainTabCtrl.AbsLocation.X + mainTabCtrl.Width;
            for (int i = 0; i != tabs.Count; ++i)
            {
                ZenTabControl tc = new ZenTabControl(Scale, this, false);
                tc.Text = tabs[i].Header;
                tc.LogicalSize = new Size(80, 30);
                tc.Size = new Size(tc.PreferredWidth, tc.Height);
                tc.AbsLocation = new Point(posx, headerHeight - tc.Height);
                posx += tc.Width;
                tc.MouseClick += tabCtrl_MouseClick;
                contentTabControls.Add(tc);
            }
            // If this is the first content tab being added, this will be the active (visible) one
            if (tabs.Count == 1)
            {
                RemoveChild(mainTab.Ctrl);
                contentTabControls[0].Selected = true;
            }
            // Must arrange controls - so newly added, and perhaps displayed, content tab
            // gets sized and placed
            arrangeControls();
            // Redraw
            doRepaint();
            form.Invalidate();
        }

        private void onFormClosed(object sender, FormClosedEventArgs e)
        {
            Dispose();
        }

        public override void Dispose()
        {
            form.Dispose();
            base.Dispose();
        }

        private void createZenControls()
        {
            ctrlClose = new ZenCloseControl(Scale, this);
            ctrlClose.LogicalSize = new Size(40, 20);
            ctrlClose.AbsLocation = new Point(form.Width - ctrlClose.Width - innerPadding, 1);
            ctrlClose.MouseClick += ctrlClose_MouseClick;

            mainTabCtrl = new ZenTabControl(Scale, this, true);
            mainTabCtrl.Text = "Main";
            mainTabCtrl.LogicalSize = new Size(80, 30);
            mainTabCtrl.Size = new Size(mainTabCtrl.PreferredWidth, mainTabCtrl.Height);
            mainTabCtrl.AbsLocation = new Point(1, headerHeight - mainTabCtrl.Height);
            mainTabCtrl.MouseClick += tabCtrl_MouseClick;

            controlsCreated = true;
        }

        private void arrangeControls()
        {
            if (!controlsCreated) return;
            
            // Resize and place active content tab, if any
            foreach (ZenTab zt in tabs)
            {
                if (!form.Created || ZenChildren.Contains(zt.Ctrl))
                {
                    zt.Ctrl.AbsLocation = new Point(innerPadding, headerHeight + innerPadding);
                    zt.Ctrl.Size = new Size(
                        form.Width - 2 * innerPadding,
                        form.Height - 2 * innerPadding - headerHeight);
                }
            }
            // Resize main tab, if active
            if (mainTab != null && (!form.Created || ZenChildren.Contains(mainTab.Ctrl)))
            {
                mainTab.Ctrl.AbsLocation = new Point(innerPadding, headerHeight + innerPadding);
                mainTab.Ctrl.Size = new Size(
                    form.Width - 2 * innerPadding,
                    form.Height - 2 * innerPadding - headerHeight);
            }

            ctrlClose.AbsLocation = new Point(form.Width - ctrlClose.Size.Width - innerPadding, 1);
        }

        private void ctrlClose_MouseClick(ZenControl sender)
        {
            form.Close();
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
                RemoveChild(tabs[activeTabIdx].Ctrl);
                AddChild(mainTab.Ctrl);
                activeTabIdx = -1;
            }
            // Switching away to a content tab
            else
            {
                mainTabCtrl.Selected = false;
                contentTabControls[idx].Selected = true;
                RemoveChild(mainTab.Ctrl);
                AddChild(tabs[idx].Ctrl);
                activeTabIdx = idx;
            }
            // Newly active contol still has old size if window was resized
            arrangeControls();
            // Refresh
            doRepaint();
            form.Invalidate();
        }

        private void onFormSizeChanged(object sender, EventArgs e)
        {
            arrangeControls();
            doRepaint();
            form.Update();
        }

        protected override sealed void RegisterWinFormsControl(Control c)
        {
            AddWinFormsControl(c);
        }

        internal sealed override void AddWinFormsControl(Control c)
        {
            form.Controls.Add(c);
        }

        internal sealed override void RemoveWinFormsControl(Control c)
        {
            form.Controls.Remove(c);
        }


        protected override sealed void InvokeOnForm(Delegate method)
        {
            form.Invoke(method);
        }

        private void doRepaint()
        {
            ZenWinForm.PaintCanvas pc = null;
            try
            {
                pc = form.GetBitmapRenderer();
                if (pc == null) return;
                Graphics g = pc.Graphics;
                DoPaint(g);
            }
            finally
            {
                if (pc != null) pc.Dispose();
            }
        }

        internal override sealed void MakeCtrlPaint(ZenControlBase ctrl, bool needBackground, RenderMode rm)
        {
            ZenWinForm.PaintCanvas pc = null;
            try
            {
                pc = form.GetBitmapRenderer();
                if (pc == null) return;
                Graphics g = pc.Graphics;
                if (needBackground) DoPaint(g);
                else ctrl.DoPaint(g);
            }
            finally
            {
                if (pc != null) pc.Dispose();
            }
            if (rm == RenderMode.None) return;
            if (form.InvokeRequired)
            {
                form.Invoke((MethodInvoker)delegate
                {
                    if (rm == RenderMode.Invalidate) form.Invalidate();
                    else form.Refresh();
                });
            }
            else
            {
                if (rm == RenderMode.Invalidate) form.Invalidate();
                else form.Refresh();
            }
        }

        private void doPaintBackground(Graphics g)
        {
            using (Brush b = new SolidBrush(ZenParams.HeaderBackColor))
            {
                g.FillRectangle(b, 0, 0, form.Width, headerHeight);
            }
            // For content tab and main tab, pad with different color
            Color colPad = ZenParams.PaddingBackColor;
            if (activeTabIdx == -1) colPad = Color.White;
            using (Brush b = new SolidBrush(colPad))
            {
                g.FillRectangle(b, innerPadding, headerHeight, form.Width - 2 * innerPadding, innerPadding);
                g.FillRectangle(b, 0, headerHeight, innerPadding, form.Height - headerHeight);
                g.FillRectangle(b, form.Width - innerPadding, headerHeight, innerPadding, form.Height - headerHeight);
                g.FillRectangle(b, innerPadding, form.Height - innerPadding - 1, form.Width - 2 * innerPadding, innerPadding);
            }
            using (Pen p = new Pen(ZenParams.BorderColor))
            {
                p.Width = 1;
                g.DrawRectangle(p, 0, 0, form.Width - 1, form.Height - 1);
            }
        }

        public override void DoPaint(Graphics g)
        {
            doPaintBackground(g);
            float x = contentTabControls[contentTabControls.Count - 1].AbsRight;
            x += ZenParams.HeaderTabPadding * 3.0F;
            float y = 7.0F * Scale;
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
            foreach (ZenControl ctrl in ZenChildren) ctrl.DoPaint(g);
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
                form.Width - 2 * innerPadding,
                headerHeight - innerPadding);
            if (rHeader.Contains(p))
                return DragMode.Move;
            Rectangle rEast = new Rectangle(
                form.Width - innerPadding,
                0,
                innerPadding,
                form.Height);
            if (rEast.Contains(p))
            {
                if (p.Y < 2 * innerPadding) return DragMode.ResizeNE;
                if (p.Y > form.Height - 2 * innerPadding) return DragMode.ResizeSE;
                return DragMode.ResizeE;
            }
            Rectangle rWest = new Rectangle(
                0,
                0,
                innerPadding,
                form.Height);
            if (rWest.Contains(p))
            {
                if (p.Y < 2 * innerPadding) return DragMode.ResizeNW;
                if (p.Y > form.Height - 2 * innerPadding) return DragMode.ResizeSW;
                return DragMode.ResizeW;
            }
            Rectangle rNorth = new Rectangle(
                0,
                0,
                form.Width,
                innerPadding);
            if (rNorth.Contains(p))
            {
                if (p.X < 2 * innerPadding) return DragMode.ResizeNW;
                if (p.X > form.Width - 2 * innerPadding) return DragMode.ResizeNE;
                return DragMode.ResizeN;
            }
            Rectangle rSouth = new Rectangle(
                0,
                form.Height - innerPadding,
                form.Width,
                innerPadding);
            if (rSouth.Contains(p))
            {
                if (p.X < 2 * innerPadding) return DragMode.ResizeSW;
                if (p.X > form.Width - 2 * innerPadding) return DragMode.ResizeSE;
                return DragMode.ResizeS;
            }
            return DragMode.None;
        }

        private void onFormMouseLeave(object sender, EventArgs e)
        {
            if (zenCtrlWithMouse != null)
            {
                zenCtrlWithMouse.DoMouseLeave();
                zenCtrlWithMouse = null;
            }
            if (dragMode == DragMode.None || dragMode == DragMode.Move)
            {
                form.Cursor = Cursors.Arrow;
            }
        }

        private void onFormMouseEnter(object sender, EventArgs e)
        {
            Point loc = form.PointToClient(form.MousePosition);
            ZenControl ctrl = GetControl(loc);
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

        private void onFormMouseClick(object sender, MouseEventArgs e)
        {
            ZenControl ctrl = GetControl(e.Location);
            if (ctrl != null) ctrl.DoMouseClick(TranslateToControl(ctrl, e.Location), e.Button);
        }

        private void onFormMouseDown(object sender, MouseEventArgs e)
        {
            // Over any control? Not our job to handle.
            ZenControl ctrl = GetControl(e.Location);
            if (ctrl != null)
            {
                ctrl.DoMouseDown(TranslateToControl(ctrl, e.Location), e.Button);
                return;
            }

            var area = getDragArea(e.Location);
            if (area == DragMode.Move)
            {
                dragMode = DragMode.Move;
                dragStart = form.PointToScreen(e.Location);
                formBeforeDragLocation = form.Location;
            }
            else if (area != DragMode.None && area != DragMode.Move)
            {
                dragMode = area;
                dragStart = form.PointToScreen(e.Location);
                formBeforeDragSize = form.Size;
                formBeforeDragLocation = form.Location;
            }
        }

        private void onFormMouseUp(object sender, MouseEventArgs e)
        {
            dragMode = DragMode.None;

            ZenControl ctrl = GetControl(e.Location);
            if (ctrl != null) ctrl.DoMouseUp(TranslateToControl(ctrl, e.Location), e.Button);
        }

        private void onFormMouseMove(object sender, MouseEventArgs e)
        {
            Point loc = form.PointToScreen(e.Location);
            if (dragMode == DragMode.Move)
            {
                int dx = loc.X - dragStart.X;
                int dy = loc.Y - dragStart.Y;
                Point newLocation = new Point(formBeforeDragLocation.X + dx, formBeforeDragLocation.Y + dy);
                ((Form)form.TopLevelControl).Location = newLocation;
                return;
            }
            else if (dragMode == DragMode.ResizeE)
            {
                int dx = loc.X - dragStart.X;
                form.Size = new Size(formBeforeDragSize.Width + dx, formBeforeDragSize.Height);
                form.Refresh();
                return;
            }
            else if (dragMode == DragMode.ResizeW)
            {
                int dx = loc.X - dragStart.X;
                form.Left = formBeforeDragLocation.X + dx;
                form.Size = new Size(formBeforeDragSize.Width - dx, formBeforeDragSize.Height);
                form.Refresh();
                return;
            }
            else if (dragMode == DragMode.ResizeN)
            {
                int dy = loc.Y - dragStart.Y;
                form.Top = formBeforeDragLocation.Y + dy;
                form.Size = new Size(formBeforeDragSize.Width, formBeforeDragSize.Height - dy);
                form.Refresh();
                return;
            }
            else if (dragMode == DragMode.ResizeS)
            {
                int dy = loc.Y - dragStart.Y;
                form.Size = new Size(formBeforeDragSize.Width, formBeforeDragSize.Height + dy);
                form.Refresh();
                return;
            }
            else if (dragMode == DragMode.ResizeNW)
            {
                int dx = loc.X - dragStart.X;
                int dy = loc.Y - dragStart.Y;
                form.Size = new Size(formBeforeDragSize.Width - dx, formBeforeDragSize.Height - dy);
                form.Location = new Point(formBeforeDragLocation.X + dx, formBeforeDragLocation.Y + dy);
                form.Refresh();
                return;
            }
            else if (dragMode == DragMode.ResizeSE)
            {
                int dx = loc.X - dragStart.X;
                int dy = loc.Y - dragStart.Y;
                form.Size = new Size(formBeforeDragSize.Width + dx, formBeforeDragSize.Height + dy);
                form.Refresh();
                return;
            }
            else if (dragMode == DragMode.ResizeNE)
            {
                int dx = loc.X - dragStart.X;
                int dy = loc.Y - dragStart.Y;
                form.Size = new Size(formBeforeDragSize.Width + dx, formBeforeDragSize.Height - dy);
                form.Location = new Point(formBeforeDragLocation.X, formBeforeDragLocation.Y + dy);
                form.Refresh();
                return;
            }
            else if (dragMode == DragMode.ResizeSW)
            {
                int dx = loc.X - dragStart.X;
                int dy = loc.Y - dragStart.Y;
                form.Size = new Size(formBeforeDragSize.Width - dx, formBeforeDragSize.Height + dy);
                form.Location = new Point(formBeforeDragLocation.X + dx, formBeforeDragLocation.Y);
                form.Refresh();
                return;
            }

            // Over any controls? Not our job to handle.
            ZenControl ctrl = GetControl(e.Location);
            if (ctrl != null)
            {
                form.Cursor = Cursors.Arrow;
                if (zenCtrlWithMouse != ctrl)
                {
                    if (zenCtrlWithMouse != null) zenCtrlWithMouse.DoMouseLeave();
                    ctrl.DoMouseEnter();
                    zenCtrlWithMouse = ctrl;
                }
                ctrl.DoMouseMove(TranslateToControl(ctrl, e.Location), e.Button);
                return;
            }
            else if (zenCtrlWithMouse != null)
            {
                zenCtrlWithMouse.DoMouseLeave();
                zenCtrlWithMouse = null;
            }

            var area = getDragArea(e.Location);
            if (area == DragMode.ResizeW || area == DragMode.ResizeE)
                form.Cursor = Cursors.SizeWE;
            else if (area == DragMode.ResizeN || area == DragMode.ResizeS)
                form.Cursor = Cursors.SizeNS;
            else if (area == DragMode.ResizeNW || area == DragMode.ResizeSE)
                form.Cursor = Cursors.SizeNWSE;
            else if (area == DragMode.ResizeNE || area == DragMode.ResizeSW)
                form.Cursor = Cursors.SizeNESW;
            else form.Cursor = Cursors.Arrow;
        }
    }
}
