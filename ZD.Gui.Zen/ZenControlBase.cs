﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace ZD.Gui.Zen
{
    /// <summary>
    /// Abstract base class for Zen controls as well as Zen tabbed form.
    /// </summary>
    public abstract class ZenControlBase : IDisposable
    {
        /// <summary>
        /// Delegate for <see cref="MouseClick"/> event.
        /// </summary>
        /// <param name="sender"></param>
        public delegate void ClickDelegate(ZenControlBase sender);
        /// <summary>
        /// Emitted when the control is clicked.
        /// </summary>
        public event ClickDelegate MouseClick;
        
        /// <summary>
        /// The control's parent (owner) control.
        /// </summary>
        private ZenControlBase parent;
        /// <summary>
        /// The control's child controls.
        /// </summary>
        private readonly List<ZenControlBase> zenChildren = new List<ZenControlBase>();
        /// <summary>
        /// The WinForms controls owned by this Zen control.
        /// </summary>
        private readonly List<Control> winFormsControls = new List<Control>();
        /// <summary>
        /// The control's absolute rectangle on top-level window canvas, in real screen pixels at current scale.
        /// </summary>
        private Rectangle absRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// The child control over which the mouse currently hovers.
        /// </summary>
        /// <remarks>
        /// Used to send "mouse leave" and "mouse enter" notifications to child controls.
        /// </remarks>
        private ZenControlBase ctrlWithMouse = null;
        /// <summary>
        /// True if control has already been disposed.
        /// </summary>
        private bool isDisposed = false;

        /// <summary>
        /// Ctor: take parent.
        /// </summary>
        /// <param name="parent"></param>
        internal ZenControlBase(ZenControlBase parent)
        {
            this.parent = parent;
            if (Parent != null) Parent.zenChildren.Add(this);
        }

        public virtual void Dispose()
        {
            foreach (ZenControlBase ctrl in zenChildren) ctrl.Dispose();
            isDisposed = true;
        }

        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public virtual float Scale
        {
            get { return Parent.Scale; }
        }

        public Size Size
        {
            get { return absRect.Size; }
            set
            {
                if (absRect.Size == value) return;
                absRect.Size = value;
                OnSizeChanged();
            }
        }

        public virtual Rectangle AbsRect
        {
            get { return absRect; }
        }

        public Rectangle RelRect
        {
            get { return new Rectangle(absRect.X - Parent.AbsRect.X, absRect.Y - Parent.AbsRect.Y, absRect.Width, absRect.Height); }
        }

        public int AbsLeft
        {
            get { return absRect.X; }
            set
            {
                int diff = value - absRect.X;
                absRect.Location = new Point(value, absRect.Location.Y);
                foreach (ZenControl ctrl in zenChildren) ctrl.AbsLeft += diff;
            }
        }

        public int AbsRight
        {
            get { return absRect.X + absRect.Width; }
        }

        public int AbsTop
        {
            get { return absRect.Y; }
            set
            {
                int diff = value - absRect.Y;
                absRect.Location = new Point(absRect.X, value);
                foreach (ZenControl ctrl in zenChildren) ctrl.AbsTop += diff;
            }
        }

        public int AbsBottom
        {
            get { return absRect.Y + absRect.Height; }
        }

        public int Width
        {
            get { return absRect.Width; }
            set
            {
                if (Width == value) return;
                absRect.Size = new Size(value, absRect.Height);
                OnSizeChanged();
            }
        }

        public int Height
        {
            get { return absRect.Height; }
            set
            {
                if (Height == value) return;
                absRect.Size = new Size(absRect.Width, value);
                OnSizeChanged();
            }
        }

        public virtual Size LogicalSize
        {
            set
            {
                float w = ((float)value.Width) * Scale;
                float h = ((float)value.Height) * Scale;
                Size newSize = new Size((int)w, (int)h);
                if (absRect.Size == newSize) return;
                absRect.Size = newSize;
                OnSizeChanged();
            }
            get { return new Size((int)(absRect.Width / Scale), (int)(absRect.Height / Scale)); }
        }

        public Point AbsLocation
        {
            get { return absRect.Location; }
            set
            {
                Point newLoc = value;
                int diffX = newLoc.X - absRect.X;
                int diffY = newLoc.Y - absRect.Y;
                absRect.Location = newLoc;
                foreach (ZenControl ctrl in zenChildren)
                {
                    Point childNewLoc = new Point(ctrl.AbsLocation.X + diffX, ctrl.AbsLocation.Y + diffY);
                    ctrl.AbsLocation = childNewLoc;
                }
            }
        }

        public Point RelLocation
        {
            get { return RelRect.Location; }
            set
            {
                Point newLoc = value;
                int diffX = newLoc.X - RelRect.X;
                int diffY = newLoc.Y - RelRect.Y;
                absRect.Location = new Point(absRect.X + diffX, absRect.Y + diffY);
                foreach (ZenControl ctrl in zenChildren)
                {
                    Point childNewLoc = new Point(ctrl.AbsLocation.X + diffX, ctrl.AbsLocation.Y + diffY);
                    ctrl.AbsLocation = childNewLoc;
                }
            }
        }

        public int RelLeft
        {
            get { return absRect.X - Parent.absRect.X; }
            set { RelLocation = new Point(value, RelTop); }
        }

        public int RelTop
        {
            get { return absRect.Y - parent.absRect.Y; }
            set { RelLocation = new Point(RelLeft, value); }
        }

        public int RelRight
        {
            get { return RelLeft + Width; }
        }

        public int RelBottom
        {
            get { return RelTop + Height; }
        }

        public Point AbsLogicalLocation
        {
            set
            {
                float x = ((float)value.X) * Scale;
                float y = ((float)value.Y) * Scale;
                AbsLocation = new Point((int)x, (int)y);
            }
            get { return new Point((int)(absRect.X / Scale), (int)(absRect.Y / Scale)); }
        }

        public Point RelLogicalLocation
        {
            set
            {
                float x = ((float)value.X) * Scale;
                float y = ((float)value.Y) * Scale;
                RelLocation = new Point((int)x, (int)y);
            }
            get { return new Point((int)(RelRect.X / Scale), (int)(RelRect.Y / Scale)); }
        }

        /// <summary>
        /// Gets or sets the cursor.
        /// </summary>
        public abstract Cursor Cursor
        {
            get;
            set;
        }

        protected virtual void OnSizeChanged()
        {
        }

        protected virtual void OnFormLoaded()
        {
            foreach (ZenControlBase child in zenChildren) child.OnFormLoaded();
        }

        public abstract void DoPaint(Graphics g);

        protected SizeF MeasureText(string text, Font font, StringFormat fmt)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                return g.MeasureString(text, font, int.MaxValue, fmt);
            }
        }

        protected void DoPaintChildren(Graphics g)
        {
            g.ResetTransform();
            foreach (ZenControl ctrl in zenChildren)
            {
                g.TranslateTransform(ctrl.AbsLeft, ctrl.AbsTop);
                g.Clip = new Region(new Rectangle(0, 0, ctrl.Width, ctrl.Height));
                ctrl.DoPaint(g);
                g.ResetTransform();
            }
        }

        /// <summary>
        /// Register/unregister chain. Implemented and sealed by <see cref="ZenTabbedForm"/>.
        /// </summary>
        /// <param name="ctrl">Control to register for showing tooltips.</param>
        /// <param name="tt">Tooltip informatio provider. To show not tooltips, pass null.</param>
        internal virtual void RegisterControlForTooltip(ZenControlBase ctrl, IZenTooltip tt)
        {
            ZenControlBase parent = Parent;
            if (parent != null) parent.RegisterControlForTooltip(ctrl, tt);
        }

        /// <summary>
        /// Signal mouse action to form, to start countdown/animation for showing or hiding tooltip.
        /// </summary>
        /// <param name="ctrl">The affected control.</param>
        /// <param name="enter">
        /// <para>If true, countdown for showing begins. If false, animation to take down tooltip begins</para>
        /// <para>Can be called multiple times with false: hide animation will continue if already in progress.</para>
        /// <para>If called with false after tooltip has already expired and been hidden, has no effect.</para>
        /// </param>
        internal virtual void TooltipMouseAction(ZenControlBase ctrl, bool show)
        {
            ZenControlBase parent = Parent;
            if (parent != null) parent.TooltipMouseAction(ctrl, show);
        }

        /// <summary>
        /// Hides tooltip if visible.
        /// </summary>
        protected void KillTooltip()
        {
            TooltipMouseAction(this, false);
        }

        /// <summary>
        /// Triggers a callback to control's <see cref="DoPaint"/>, then re-renders canvas.
        /// </summary>
        /// <param name="needBackground">If true, full canvas is repainted to control can render over stuff outside it's own rectangle.</param>
        /// <param name="rm">Determines when canvas is re-rendered after painting.</param>
        protected void MakeMePaint(bool needBackground, RenderMode rm)
        {
            // Request comes from background thread. In the UI thread, parent may just have been removed.
            // If I have no parent, silently do not invoke.
            // Happens when an animation is in progress and user switches to different tab in top form
            ZenControlBase parent = Parent;
            if (parent != null) parent.MakeCtrlPaint(this, needBackground, rm);
        }

        /// <summary>
        /// End of chain; see <see cref="MakeMePaint"/>.
        /// </summary>
        internal virtual void MakeCtrlPaint(ZenControlBase ctrl, bool needBackground, RenderMode rm)
        {
            // Request comes from background thread. In the UI thread, parent may just have been removed.
            // If I have no parent, silently do not invoke.
            // Happens when an animation is in progress and user switches to different tab in top form
            ZenControlBase parent = Parent;
            if (parent != null) parent.MakeCtrlPaint(ctrl, needBackground, rm);
        }

        /// <summary>
        /// <para>Makes the control repaint itself and invalidates Form for rendering.</para>
        /// <para>Does not request new background. Override in controls using opacity/transparency.</para>
        /// </summary>
        public virtual void Invalidate()
        {
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Returns mouse position in top form's coordinates (canvas's absolute coordinates).
        /// </summary>
        protected virtual Point MousePositionAbs
        {
            get { return Parent.MousePositionAbs; }
        }

        protected virtual void InvokeOnForm(Delegate method)
        {
            // Invoke comes from background thread. In the UI thread, parent may just have been removed.
            // If I have no parent, silently do not invoke.
            // Happens when an animation is in progress and user switches to different tab in top form
            ZenControlBase parent = Parent;
            if (parent != null) parent.InvokeOnForm(method);
        }


        public ZenControlBase Parent
        {
            get { return parent; }
        }

        protected ReadOnlyCollection<ZenControlBase> ZenChildren
        {
            get { return new ReadOnlyCollection<ZenControlBase>(zenChildren); }
        }

        protected void RemoveChild(ZenControlBase ctrl)
        {
            IEnumerable<Control> containedWinFormsControls = ctrl.GetWinFormsControlsRecursive();
            foreach (Control c in containedWinFormsControls)
                RemoveWinFormsControl(c);
            zenChildren.Remove(ctrl);
            ctrl.parent = null;
        }

        protected void AddChild(ZenControlBase ctrl)
        {
            if (zenChildren.Contains(ctrl))
            {
                if (ctrl.Parent != this)
                    throw new InvalidOperationException("Control is already a child of a different parent.");
                return;
            }
            ctrl.parent = this;
            zenChildren.Add(ctrl);
            IEnumerable<Control> containedWinFormsControls = ctrl.GetWinFormsControlsRecursive();
            foreach (Control c in containedWinFormsControls)
                AddWinFormsControl(c);
        }

        /// <summary>
        /// See <see cref="ZenControl.RegisterWinFormsControl"/>.
        /// </summary>
        protected virtual void RegisterWinFormsControl(Control c)
        {
            winFormsControls.Add(c);
            Parent.RegisterWinFormsControl(c);
        }

        /// <summary>
        /// Add/remove chain. Implemented by <see cref="ZenTabbedForm"/>.
        /// </summary>
        internal virtual void AddWinFormsControl(Control c)
        {
            Parent.AddWinFormsControl(c);
        }

        /// <summary>
        /// Add/remove chain. Implemented by <see cref="ZenTabbedForm"/>.
        /// </summary>
        internal virtual void RemoveWinFormsControl(Control c)
        {
            Parent.RemoveWinFormsControl(c);
        }

        internal IEnumerable<Control> GetWinFormsControlsRecursive()
        {
            List<Control> res = new List<Control>(winFormsControls);
            foreach (ZenControlBase child in zenChildren)
                res.AddRange(child.GetWinFormsControlsRecursive());
            return res;
        }

        /// <summary>
        /// Subscribes current control to timer events, so control receives callbacks to <see cref="DoTimer"/>.
        /// </summary>
        protected void SubscribeToTimer()
        {
            ZenTimer.SubscribeToTimer(this);
        }

        /// <summary>
        /// Unsubscribes from timer events, so <see cref="DoTimer"/> stops getting called.
        /// </summary>
        protected void UnsubscribeFromTimer()
        {
            ZenTimer.UnsubscribeFromTimer(this);
        }

        /// <summary>
        /// Called periodically after control has subscribed to timer events via <see cref="SubscribeToTimer"/>.
        /// </summary>
        public virtual void DoTimer()
        {
        }

        /// <summary>
        /// Returns true if point is inside control, as expressed in parent's coordinate system.
        /// </summary>
        public bool Contains(Point pParent)
        {
            return RelRect.Contains(pParent);
        }

        /// <summary>
        /// Gets the child control that contains point, as expressed in this control's coordinate system.
        /// </summary>
        protected ZenControlBase GetControl(Point p)
        {
            foreach (ZenControlBase ctrl in zenChildren)
                if (ctrl.Contains(p)) return ctrl;
            return null;
        }

        /// <summary>
        /// Translates a location in the parent's coordinate system to a control's coordinate system.
        /// </summary>
        private Point parentToControl(ZenControlBase ctrl, Point pParent)
        {
            int x = pParent.X - ctrl.RelRect.X;
            int y = pParent.Y - ctrl.RelRect.Y;
            return new Point(x, y);
        }

        /// <summary>
        /// Translates absolute (canvas) coordinates to this control's local coordinates.
        /// </summary>
        /// <param name="pAbs">The point in absolute coordinates.</param>
        /// <returns>The local position within this control.</returns>
        protected Point AbsToControl(Point pAbs)
        {
            return new Point(pAbs.X - RelLocation.X, pAbs.Y - RelLocation.Y);
        }

        /// <summary>
        /// Fires the mouse click event.
        /// </summary>
        protected void FireClick()
        {
            if (MouseClick != null) MouseClick(this);
        }

        public virtual bool DoMouseClick(Point p, MouseButtons button)
        {
            ZenControlBase ctrl = GetControl(p);
            if (ctrl != null)
                return ctrl.DoMouseClick(parentToControl(ctrl, p), button);
            else if (MouseClick != null)
                MouseClick(this);
            return true;
        }

        public virtual bool DoMouseMove(Point p, MouseButtons button)
        {
            bool res = false;
            ZenControlBase ctrl = GetControl(p);
            if (ctrl != null)
            {
                if (ctrlWithMouse != ctrl)
                {
                    if (ctrlWithMouse != null) ctrlWithMouse.DoMouseLeave();
                    ctrl.DoMouseEnter();
                    ctrlWithMouse = ctrl;
                }
                ctrl.DoMouseMove(parentToControl(ctrl, p), button);
                res = true;
            }
            else if (ctrlWithMouse != null)
            {
                ctrlWithMouse.DoMouseLeave();
                ctrlWithMouse = null;
            }
            return res;
        }

        public virtual bool DoMouseDown(Point p, MouseButtons button)
        {
            ZenControlBase ctrl = GetControl(p);
            if (ctrl != null)
                return ctrl.DoMouseDown(parentToControl(ctrl, p), button);
            return false;
        }

        public virtual bool DoMouseUp(Point p, MouseButtons button)
        {
            ZenControlBase ctrl = GetControl(p);
            if (ctrl != null)
                return ctrl.DoMouseUp(parentToControl(ctrl, p), button);
            return false;
        }

        public virtual void DoMouseEnter()
        {
            // Let parent form know so it can show tooltips if needed.
            TooltipMouseAction(this, true);
            // Forward leave/enter notifiations to any affected child controls.
            Point pAbs = MousePositionAbs;
            Point pRel = new Point(pAbs.X - AbsLeft, pAbs.Y - AbsTop);
            ZenControlBase ctrl = GetControl(pRel);
            if (ctrl != null)
            {
                if (ctrlWithMouse != ctrl)
                {
                    if (ctrlWithMouse != null) ctrlWithMouse.DoMouseLeave();
                    ctrl.DoMouseEnter();
                    ctrlWithMouse = ctrl;
                }
            }
        }

        public virtual void DoMouseLeave()
        {
            // Let parent form know so it can hide any visible tooltip.
            TooltipMouseAction(this, false);
            // Forward leave notifiation to any affected child control.
            if (ctrlWithMouse != null)
            {
                ctrlWithMouse.DoMouseLeave();
                ctrlWithMouse = null;
            }
        }
    }
}