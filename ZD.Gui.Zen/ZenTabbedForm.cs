using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

using ZD.Common;

namespace ZD.Gui.Zen
{
    public partial class ZenTabbedForm : ZenControlBase, IDisposable, IZenTabsChangedListener
    {
        #region Private members

        /// <summary>
        /// Source of localized UI strings;
        /// </summary>
        private readonly ITextProvider tprov;
        /// <summary>
        /// Timer for continuous 25fps animations.
        /// </summary>
        private readonly ZenTimer zenTimer;
        /// <summary>
        /// Lock object for canvas.
        /// </summary>
        private readonly Mutex canvasMutex = new Mutex();
        /// <summary>
        /// The bitmap everyone is drawing on - the full window.
        /// </summary>
        private Bitmap canvas = null;
        /// <summary>
        /// The current bitmap size.
        /// </summary>
        private Size canvasSize = Size.Empty;
        /// <summary>
        /// The form we pretend to be towards Windows.
        /// </summary>
        private readonly ZenWinForm form;
        /// <summary>
        /// True as soon as all default controls on form are created. (No layout until that point.)
        /// </summary>
        private bool controlsCreated = false;
        /// <summary>
        /// True if Windows form has already loaded.
        /// </summary>
        private bool isLoaded = false;
        /// <summary>
        /// The form's close system button.
        /// </summary>
        private ZenSystemButton btnClose;
        /// <summary>
        /// The form's minimize system button.
        /// </summary>
        private ZenSystemButton btnMinimize;
        /// <summary>
        /// The "main" (leftmost) tab control at the top.
        /// </summary>
        private ZenTabControl mainTabCtrl;
        /// <summary>
        /// Content tabs to the right of the "main" tab.
        /// </summary>
        private readonly List<ZenTabControl> contentTabControls = new List<ZenTabControl>();
        /// <summary>
        /// The main tab, as defined by the form's consumer.
        /// </summary>
        private ZenTab mainTab;
        /// <summary>
        /// Content tabs to the right of the "main" tab, as defined by the form's consumer.
        /// </summary>
        private readonly ZenTabCollection tabs;
        /// <summary>
        /// Index of the currently active tab. -1 for main tab, 0 and onwards for other tabs.
        /// </summary>
        private int activeTabIdx = 0;
        /// <summary>
        /// Cached - height of my header area at current scale.
        /// </summary>
        private readonly int headerHeight;
        /// <summary>
        /// Cached - padding inside tooltips at current scale.
        /// </summary>
        private readonly int tooltipPadding;
        /// <summary>
        /// Cached - paddig from border to edge of content area at current scale.
        /// </summary>
        private readonly int innerPadding;
        /// <summary>
        /// Cached - padding from right border to right of close button, at current scale.
        /// </summary>
        private readonly int sysBtnPadding;
        /// <summary>
        /// My window header text.
        /// </summary>
        private string header;
        /// <summary>
        /// My window header text, with dynamically calculated ellipsis at end, to fit space.
        /// </summary>
        private string headerEllipsed = null;
        /// <summary>
        /// My minimum size in logical (unscaled) pixels.
        /// </summary>
        private Size logicalMinimumSize = Size.Empty;
        /// <summary>
        /// The cursor to be shown over form (unless some hot area is active).
        /// </summary>
        private Cursor desiredCursor = Cursors.Arrow;
        /// <summary>
        /// Info about all controls that have put in a request for a tooltip.
        /// </summary>
        private readonly Dictionary<ZenControlBase, TooltipInfo> tooltipInfos = new Dictionary<ZenControlBase, TooltipInfo>();
        /// <summary>
        /// The control currently capturing the mouse (used by scrollbar to keep thumb pushed even when mouse leaves form).
        /// </summary>
        private ZenControlBase ctrlCapturingMouse = null;
        /// <summary>
        /// Currently displayed context menu form.
        /// </summary>
        private CtxtMenuForm ctxtForm = null;

        #endregion

        #region Ctor, init, dispose

        public ZenTabbedForm(ITextProvider tprov)
            : base(null)
        {
            this.tprov = tprov;
            this.zenTimer = new ZenTimer(this);
            this.form = new ZenWinForm(getCanvas);

            headerHeight = (int)(ZenParams.HeaderHeight * Scale);
            innerPadding = (int)(ZenParams.InnerPadding * Scale);
            sysBtnPadding = (int)(ZenParams.SysBtnPadding * Scale);
            tooltipPadding = (int)(ZenParams.TooltipPadding * Scale);

            createZenControls();
            tabs = new ZenTabCollection(this);

            form.Deactivate += onFormDeactivate;
            form.KeyDown += onFormKeyDown;
            form.MouseDown += onFormMouseDown;
            form.MouseMove += onFormMouseMove;
            form.MouseUp += onFormMouseUp;
            form.MouseClick += onFormMouseClick;
            form.MouseEnter += onFormMouseEnter;
            form.MouseLeave += onFormMouseLeave;
            form.FormClosed += onFormClosed;
            form.Load += onFormLoaded;
        }

        public override void Dispose()
        {
            if (ctxtForm != null) { ctxtForm.Dispose(); ctxtForm = null; }
            form.Dispose();
            if (canvas != null)
            {
                try
                {
                    canvasMutex.WaitOne();
                    canvas.Dispose();
                    canvas = null;
                }
                finally
                {
                    canvasMutex.ReleaseMutex();
                }
            }
            base.Dispose();
        }

        private void createZenControls()
        {
            int w = 0;

            btnClose = new ZenSystemButton(this, SystemButtonType.Close);
            btnClose.AbsLocation = new Point(w - btnClose.Width - sysBtnPadding, 0);
            btnClose.MouseClick += onCloseClick;

            btnMinimize = new ZenSystemButton(this, SystemButtonType.Minimize);
            btnMinimize.AbsLocation = new Point(btnClose.AbsLeft - btnMinimize.Size.Width, 0);
            btnMinimize.MouseClick += onMinimizeClick;

            btnClose.Tooltip = new SysBtnTooltips(btnClose, tprov);
            btnMinimize.Tooltip = new SysBtnTooltips(btnMinimize, tprov);

            mainTabCtrl = new ZenTabControl(this, true);
            mainTabCtrl.Text = "Main";
            mainTabCtrl.LogicalSize = new Size(80, 30);
            mainTabCtrl.Size = new Size(mainTabCtrl.PreferredWidth, mainTabCtrl.Height);
            mainTabCtrl.AbsLocation = new Point(1, headerHeight - mainTabCtrl.Height);
            mainTabCtrl.MouseClick += onTabCtrlClick;

            controlsCreated = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Windows form we pretend to be.
        /// </summary>
        public Form WinForm
        {
            get { return form; }
        }

        /// <summary>
        /// Gets or sets the form's cursor.
        /// </summary>
        public override sealed Cursor Cursor
        {
            get { return form.Cursor; }
            set { desiredCursor = value; form.Cursor = value; }
        }

        /// <summary>
        /// Gets the scale (real pixes are logical [96DPI] pixels times this).
        /// </summary>
        public override float Scale
        {
            get { return form.Scale; }
        }

        /// <summary>
        /// Gets the size of form's content area.
        /// </summary>
        private Size ContentSize
        {
            get
            {
                return new Size(
                    form.Width - 2 * (int)ZenParams.InnerPadding,
                    form.Height - (int)ZenParams.InnerPadding - (int)ZenParams.HeaderHeight);
            }
        }

        /// <summary>
        /// Gets the location of our content area.
        /// </summary>
        private Point ContentLocation
        {
            get
            {
                return new Point((int)ZenParams.InnerPadding, (int)(ZenParams.HeaderHeight + ZenParams.InnerPadding));
            }
        }

        /// <summary>
        /// Gets or sets the main content tab (highlighted as first item in list).
        /// </summary>
        protected ZenTab MainTab
        {
            get { return mainTab; }
            set
            {
                mainTab = value;
                mainTab.Ctrl.AbsLocation = ContentLocation;
                mainTab.Ctrl.Size = ContentSize;
                mainTabCtrl.Text = mainTab.Header;
                mainTabCtrl.Size = new Size(mainTabCtrl.PreferredWidth, mainTabCtrl.Height);
            }
        }

        /// <summary>
        /// Gets form's tab collection - excluding the main tab.
        /// </summary>
        protected ZenTabCollection Tabs
        {
            get { return tabs; }
        }

        /// <summary>
        /// Gets or sets form's header text.
        /// </summary>
        protected string Header
        {
            get { return header; }
            set
            {
                header = value;
                headerEllipsed = null;
                doRepaint();
                form.Text = value;
                form.Invalidate();
            }
        }

        /// <summary>
        /// Index of the active tab. 0 is first content tab; -1 is main tab.
        /// </summary>
        protected int ActiveTabIdx
        {
            get { return activeTabIdx; }
        }

        /// <summary>
        /// Gets or sets the form's size in actual, scaled pixels.
        /// </summary>
        public override sealed Size Size
        {
            get { return AbsRect.Size; }
            set
            {
                if (AbsRect.Size == value) return;
                doRecreateCanvas(value);
                doRepaint();
                form.Size = value;
                OnSizeChanged();
            }
        }

        /// <summary>
        /// Gets or sets the form's size, expressed in logical (unscaled) pixels.
        /// </summary>
        public override sealed Size LogicalSize
        {
            set
            {
                float w = value.Width;
                float h = value.Height;
                Size sz = new Size((int)(w * Scale), (int)(h * Scale));
                Size = sz;
            }
            get
            {
                Size sz = canvasSize;
                return new Size((int)Math.Ceiling((sz.Width / Scale)), (int)Math.Ceiling(sz.Height / Scale));
            }
        }

        /// <summary>
        /// Gets or sets the form's minimum size, expressed in logical (unscaled) pixels.
        /// </summary>
        public Size LogicalMinimumSize
        {
            get { return logicalMinimumSize; }
            set { logicalMinimumSize = value; }
        }

        /// <summary>
        /// Returns mouse position in top form's coordinates (canvas's absolute coordinates).
        /// </summary>
        protected override sealed Point MousePositionAbs
        {
            get { return form.PointToClient(form.MousePosition); }
        }

        /// <summary>
        /// Gets the form's absolute rectangle (top left is 0,0; size is form's size).
        /// </summary>
        public override sealed Rectangle AbsRect
        {
            get
            {
                Size sz = canvasSize;
                return new Rectangle(0, 0, sz.Width, sz.Height);
            }
        }

        /// <summary>
        /// Seals chain. Returns this form's zen timer.
        /// </summary>
        internal sealed override ZenTimer Timer
        {
            get { return zenTimer; }
        }

        /// <summary>
        /// Gets or sets the form's location on screen.
        /// </summary>
        public Point Location
        {
            get { return form.Location; }
            set { form.Location = value; }
        }

        #endregion

        #region Layout, resize, paint

        /// <summary>
        /// Arranges controls after form's size has changed.
        /// </summary>
        private void arrangeControls()
        {
            if (!controlsCreated) return;

            headerEllipsed = null;

            // Resize and place active content tab, if any
            foreach (ZenTab zt in tabs)
            {
                if (!form.Created || ZenChildren.Contains(zt.Ctrl))
                {
                    zt.Ctrl.AbsLocation = new Point(innerPadding, headerHeight + innerPadding);
                    zt.Ctrl.Size = new Size(
                        canvasSize.Width - 2 * innerPadding,
                        canvasSize.Height - 2 * innerPadding - headerHeight);
                }
            }
            // Resize main tab, if active
            if (mainTab != null && (!form.Created || ZenChildren.Contains(mainTab.Ctrl)))
            {
                mainTab.Ctrl.AbsLocation = new Point(innerPadding, headerHeight + innerPadding);
                mainTab.Ctrl.Size = new Size(
                    canvasSize.Width - 2 * innerPadding,
                    canvasSize.Height - 2 * innerPadding - headerHeight);
            }

            btnClose.AbsLocation = new Point(canvasSize.Width - btnClose.Size.Width - sysBtnPadding, 0);
            btnMinimize.AbsLocation = new Point(btnClose.AbsLeft - btnMinimize.Size.Width, 0);
        }

        /// <summary>
        /// Returns the canvas to be blitted to screen in Windows paint event.
        /// </summary>
        private ZenWinForm.CanvasToShow getCanvas()
        {
            canvasMutex.WaitOne();
            if (canvas == null)
            {
                canvasMutex.ReleaseMutex();
                return null;
            }
            // Lock on mutex is transferred to disposable ownership class here.
            return new ZenWinForm.CanvasToShow(canvasMutex, canvas);
        }

        /// <summary>
        /// Recreates the canvas, which defines window size as far as we are concerned.
        /// </summary>
        /// <param name="sz">New desired size</param>
        /// <returns>True if canvas size actually changed.</returns>
        private bool doRecreateCanvas(Size sz)
        {
            try
            {
                canvasMutex.WaitOne();
                if (canvas != null && canvas.Size == sz) return false;
                if (canvas != null) { canvas.Dispose(); canvas = null; }
                canvas = new Bitmap(sz.Width, sz.Height);
                canvasSize = sz;
                arrangeControls();
                return true;
            }
            finally
            {
                canvasMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Trigger full repaint of form on current canvas.
        /// </summary>
        private void doRepaint()
        {
            try
            {
                canvasMutex.WaitOne();
                if (canvas == null) return;
                Graphics g = Graphics.FromImage(canvas);
                DoPaint(g);
            }
            finally
            {
                canvasMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Calls paint function of provided controls. Or repaints entire canvas (all controls) if needed.
        /// </summary>
        internal override sealed void MakeControlsPaint(ReadOnlyCollection<ControlToPaint> ctrls)
        {
            if (!isLoaded) return;
            // Strongest (most immediate) render mode requested
            RenderMode rm = RenderMode.None;
            try
            {
                canvasMutex.WaitOne();
                if (canvas == null) return;
                // Draw on canvas
                Graphics g = Graphics.FromImage(canvas);
                // Paint only individual controls, or full canvas?
                bool needBackground = false;
                // If a control requests its own repaint (e.g., from animation), AND there are tooltips
                // -> We must repaint full canvas and put tooltips on top, to avoid crazy flicker
                List<TooltipToPaint> ttpList = getTooltipsToPaint();
                if (ttpList.Count != 0)
                {
                    needBackground = true;
                    rm = RenderMode.Invalidate;
                }
                // Any control specifically requesting background paint?
                if (!needBackground)
                    foreach (ControlToPaint ctp in ctrls)
                        if (ctp.NeedBackground || ctp.Ctrl == this) { needBackground = true; break; }

                // Do the painting
                if (needBackground) DoPaint(g);
                // Collect control's render mode wishes; paint them individually if needed
                foreach (ControlToPaint ctp in ctrls)
                {
                    // If control no longer has parent: don't attempt to paint.
                    if (ctp.Ctrl.CurrentParentForm == null) continue;
                    // Strongest render mode?
                    if (ctp.RenderMode > rm) rm = ctp.RenderMode;
                    if (!needBackground)
                    {
                        g.ResetTransform(); g.ResetClip();
                        g.TranslateTransform(ctp.Ctrl.AbsLeft, ctp.Ctrl.AbsTop);
                        g.Clip = new Region(new Rectangle(0, 0, ctp.Ctrl.Width, ctp.Ctrl.Height));
                        ctp.Ctrl.DoPaint(g);
                    }
                }
            }
            finally
            {
                canvasMutex.ReleaseMutex();
            }
            if (rm == RenderMode.None) return;
            if (form.InvokeRequired)
            {
                try
                {
                    form.BeginInvoke((MethodInvoker)delegate { doFormStuffAfterControlPaint(rm); });
                }
                catch (ObjectDisposedException)
                {
                    // We just swallow this.
                    // Cannot prevent timer threads from requesting a repaint
                    // As window gets disposed in GUI thread
                }
                catch (InvalidOperationException)
                {
                    // Same as above.
                }
            }
            else doFormStuffAfterControlPaint(rm);
        }

        /// <summary>
        /// Performs UI thread actions after we made controls paint - i.e., invalidate or refresh.
        /// </summary>
        private void doFormStuffAfterControlPaint(RenderMode rm)
        {
            if (rm == RenderMode.Invalidate) form.Invalidate();
            else if (rm == RenderMode.Update) form.Refresh();
        }

        #endregion

        #region Misc top-level Zen and other ZenForm logic.

        /// <summary>
        /// Handles tab change event.
        /// </summary>
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
                ZenTabControl tc = new ZenTabControl(this, false);
                tc.Text = tabs[i].Header;
                tc.LogicalSize = new Size(80, 30);
                tc.Size = new Size(tc.PreferredWidth, tc.Height);
                tc.AbsLocation = new Point(posx, headerHeight - tc.Height);
                posx += tc.Width;
                tc.MouseClick += onTabCtrlClick;
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

        /// <summary>
        /// Handles click on close button.
        /// </summary>
        private void onCloseClick(ZenControlBase sender)
        {
            form.Close();
        }

        /// <summary>
        /// Handles click on minimize button.
        /// </summary>
        void onMinimizeClick(ZenControlBase sender)
        {
            form.WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// Handles click on a tab header (to switch tabs).
        /// </summary>
        /// <param name="sender"></param>
        private void onTabCtrlClick(ZenControlBase sender)
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

        /// <summary>
        /// End of chain - takes ownership of a WinForms control, requested by a Zen child, to our controls.
        /// </summary>
        protected override sealed void RegisterWinFormsControl(Control c)
        {
            AddWinFormsControl(c);
        }

        /// <summary>
        /// End of chain - adds a WinForms control, requested by a Zen child, to our controls.
        /// </summary>
        internal sealed override void AddWinFormsControl(Control c)
        {
            form.Controls.Add(c);
        }

        /// <summary>
        /// End of chain - removes a WinForms control, requested by a Zen child, to our controls.
        /// </summary>
        internal sealed override void RemoveWinFormsControl(Control c)
        {
            form.Controls.Remove(c);
        }

        /// <summary>
        /// Invokes provided delegate on Windows form's UI thread.
        /// </summary>
        protected override sealed void InvokeOnForm(Delegate method)
        {
            form.BeginInvoke(method);
        }

        /// <summary>
        /// Set or clears the control receiving the mouse capture.
        /// </summary>
        internal sealed override void SetControlMouseCapture(ZenControlBase ctrl, bool capture)
        {
            if (capture)
            {
                ctrlCapturingMouse = ctrl;
                form.Capture = true;
            }
            else if (ctrl == ctrlCapturingMouse) ctrlCapturingMouse = null;
            if (ctrlCapturingMouse == null) form.Capture = false;
        }

        /// <summary>
        /// Gets the screen to which the provided coordinate belongs.
        /// </summary>
        private Screen getScreenForPoint(Point pt)
        {
            foreach (Screen s in Screen.AllScreens)
                if (s.Bounds.Contains(pt))
                    return s;
            return Screen.PrimaryScreen;
        }

        /// <summary>
        /// Shows a context menu at the desired location, or the nearest best place.
        /// </summary>
        /// <param name="screenLoc">The desired location in screen coordinates.</param>
        /// <param name="ctxtMenuCtrl">The context menu UI to show.</param>
        internal void ShowContextMenu(Point screenLoc, ICtxtMenuControl ctxtMenuCtrl)
        {
            // If we currently have a context menu on the screen, kill it.
            if (ctxtForm != null)
            {
                ctxtForm.Close();
                ctxtForm = null;
            }
            // Create new form
            var ff = new CtxtMenuForm(ctxtMenuCtrl);
            // Calculate optimal location.
            // Horizontally: centered around pointer
            // Vertically: prefer above, with 3px in between
            Screen scr = getScreenForPoint(screenLoc);
            int x = screenLoc.X - ff.Width / 2;
            if (x < scr.WorkingArea.Left) x = scr.WorkingArea.Left;
            else if (x + ff.Width > scr.WorkingArea.Right) x = scr.WorkingArea.Right - ff.Width;
            int y = screenLoc.Y - ff.Height - 3;
            if (y < scr.WorkingArea.Top) y = screenLoc.Y + 3;
            ff.Location = new Point(x, y);
            // Show context menu
            ff.Show(form);
            form.Focus();
            ctxtForm = ff;
        }

        /// <summary>
        /// Closes the context menu control shopwn earlier (if it's still visible at all).
        /// </summary>
        internal void CloseContextMenu(ICtxtMenuControl ctxtMenuCtrl)
        {
            if (ctxtForm != null && ctxtMenuCtrl == ctxtForm.CtxtMenuControl)
            {
                ctxtForm.Close();
                ctxtForm = null;
            }
        }

        #endregion

        #region "Raw" Windows Forms event handlers

        private void onFormDeactivate(object sender, EventArgs e)
        {
            if (ctxtForm != null) CloseContextMenu(ctxtForm.CtxtMenuControl);
        }

        private void onFormKeyDown(object sender, KeyEventArgs e)
        {
            // We always swallow Esc here: otherwise form gives us annoying "ding"
            e.Handled = true;
            e.SuppressKeyPress = true;

            if (e.KeyCode == Keys.Escape)
            {
                if (ctxtForm != null) CloseContextMenu(ctxtForm.CtxtMenuControl);
            }
            else if (ctxtForm != null && e.Modifiers == Keys.None)
            {
                if (e.KeyCode == Keys.Down) ctxtForm.CtxtMenuControl.DoNavKey(CtxtMenuNavKey.Down);
                else if (e.KeyCode == Keys.Up) ctxtForm.CtxtMenuControl.DoNavKey(CtxtMenuNavKey.Up);
                else if (e.KeyCode == Keys.Enter) ctxtForm.CtxtMenuControl.DoNavKey(CtxtMenuNavKey.Enter);
                else if (e.KeyCode == Keys.Space) ctxtForm.CtxtMenuControl.DoNavKey(CtxtMenuNavKey.Space);
            }
            else
            {
                e.Handled = false;
                e.SuppressKeyPress = false;
            }
        }

        private void onFormMouseMove(object sender, MouseEventArgs e)
        {
            DoMouseMove(e.Location, e.Button);
        }

        private void onFormLoaded(object sender, EventArgs e)
        {
            isLoaded = true;
            doRepaint();
            form.Invalidate();
            OnFormLoaded();
        }

        private void onFormClosed(object sender, FormClosedEventArgs e)
        {
            Dispose();
        }

        private void onFormMouseLeave(object sender, EventArgs e)
        {
            DoMouseLeave();
        }

        private void onFormMouseEnter(object sender, EventArgs e)
        {
            DoMouseEnter();
        }

        private void onFormMouseClick(object sender, MouseEventArgs e)
        {
            DoMouseClick(e.Location, e.Button);
        }

        private void onFormMouseDown(object sender, MouseEventArgs e)
        {
            // Any mouse down means we close context menu if visible
            if (ctxtForm != null) CloseContextMenu(ctxtForm.CtxtMenuControl);

            // Got capture?
            if (ctrlCapturingMouse != null)
            {
                // Transform to control's coordinates
                Point pCtrl = new Point(e.Location.X - ctrlCapturingMouse.AbsLeft, e.Location.Y - ctrlCapturingMouse.AbsTop);
                ctrlCapturingMouse.DoMouseDown(pCtrl, e.Button);
                return;
            }

            // Over any control? Not our job to handle.
            if (DoMouseDown(e.Location, e.Button))
                return;

            // Enter resize/move mode if needed
            if (e.Button == MouseButtons.Left) doMouseDownRM(e.Location);
        }

        private void onFormMouseUp(object sender, MouseEventArgs e)
        {
            // End resizing/moving
            doMouseUpRM();

            // Got capture?
            if (ctrlCapturingMouse != null)
            {
                // Transform to control's coordinates
                Point pCtrl = new Point(e.Location.X - ctrlCapturingMouse.AbsLeft, e.Location.Y - ctrlCapturingMouse.AbsTop);
                ctrlCapturingMouse.DoMouseUp(pCtrl, e.Button);
                return;
            }
            // Handle otherwise
            DoMouseUp(e.Location, e.Button);
        }

        #endregion

        #region ZenControlBase event handlers

        /// <summary>
        /// Handles the timer event for animations. Unsubscribes when timer no longer needed.
        /// </summary>
        public override void DoTimer(out bool? needBackground, out RenderMode? renderMode)
        {
            bool timerNeeded = false;
            bool paintNeeded = false;
            {
                bool tn, pn;
                doTimerTooltip(out tn, out pn);
                timerNeeded |= tn;
                paintNeeded |= pn;
            }
            if (!timerNeeded) UnsubscribeFromTimer();
            if (paintNeeded)
            {
                needBackground = true;
                renderMode = RenderMode.Invalidate;
            }
            else { needBackground = null; renderMode = null; }
        }

        public override void DoMouseLeave()
        {
            base.DoMouseLeave();
            // After dragging window border, reset cursor
            if (dragMode == DragMode.None || dragMode == DragMode.Move)
                form.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Called when form has finished moving or resizing.
        /// </summary>
        protected virtual void DoMoveResizeFinished()
        {
            // Nothing really. Derived controls can handle event to their delight.
        }

        public override bool DoMouseMove(Point p, MouseButtons button)
        {
            // If we're resizing or moving, do whatever we need to do.
            if (doMouseMoveRM(p)) return true;

            // Got capture?
            if (ctrlCapturingMouse != null)
            {
                // Transform to control's coordinates
                Point pCtrl = new Point(p.X - ctrlCapturingMouse.AbsLeft, p.Y - ctrlCapturingMouse.AbsTop);
                ctrlCapturingMouse.DoMouseMove(pCtrl, button);
                return true;
            }

            // Over a control of ours? If yes, we're done.
            if (base.DoMouseMove(p, button))
            {
                form.Cursor = desiredCursor;
                return true;
            }

            // Update cursor over resize/move hot areas
            doMouseMoveRMCursor(p);

            return true;
        }

        #endregion
    }
}
