using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DND.Gui.Zen
{
    public class ZenControl : ZenControlBase, IDisposable
    {
        public delegate void ClickDelegate(ZenControl sender);
        public event ClickDelegate MouseClick;

        protected readonly float scale;
        protected readonly ZenControlBase parent;
        private Rectangle absRect = new Rectangle(0, 0, 0, 0);
        private List<ZenControl> zenChildren = new List<ZenControl>();
        private ZenControl zenCtrlWithMouse = null;
        private bool paintSuspended = false;

        public Size Size
        {
            get { return absRect.Size; }
            set { absRect.Size = value; OnSizeChanged(); MakeMePaint(true, RenderMode.Invalidate); }
        }

        public override sealed Rectangle AbsRect
        {
            get { return absRect; }
        }

        public Rectangle RelRect
        {
            get { return new Rectangle(absRect.X - parent.AbsRect.X, absRect.Y - parent.AbsRect.Y, absRect.Width, absRect.Height); }
        }

        public int AbsLeft
        {
            get { return absRect.X; }
            set
            {
                int diff = value - absRect.X;
                absRect.Location = new Point(value, absRect.Location.Y);
                foreach (ZenControl ctrl in zenChildren) ctrl.AbsLeft += diff;
                MakeMePaint(true, RenderMode.Invalidate);
            }
        }

        public int AbsRight
        {
            get { return absRect.X + absRect.Width - 1; }
        }

        public int AbsTop
        {
            get { return absRect.Y; }
            set
            {
                int diff = value - absRect.Y;
                absRect.Location = new Point(absRect.X, value);
                foreach (ZenControl ctrl in zenChildren) ctrl.AbsTop += diff;
                MakeMePaint(true, RenderMode.Invalidate);
            }
        }

        public int AbsBottom
        {
            get { return absRect.Y + absRect.Height - 1; }
        }

        public int Width
        {
            get { return absRect.Width; }
            set
            {
                absRect.Size = new Size(value, absRect.Height);
                OnSizeChanged();
                MakeMePaint(true, RenderMode.Invalidate);
            }
        }

        public int Height
        {
            get { return absRect.Height; }
            set
            {
                absRect.Size = new Size(absRect.Width, value);
                OnSizeChanged();
                MakeMePaint(true, RenderMode.Invalidate);
            }
        }

        public Size LogicalSize
        {
            set
            {
                float w = ((float)value.Width) * scale;
                float h = ((float)value.Height) * scale;
                Size newSize = new Size((int)w, (int)h);
                absRect.Size = newSize;
                OnSizeChanged();
                MakeMePaint(true, RenderMode.Invalidate);
            }
            get { return new Size((int)(absRect.Width / scale), (int)(absRect.Height / scale)); }
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
                MakeMePaint(true, RenderMode.Invalidate);
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
                MakeMePaint(true, RenderMode.Invalidate);
            }
        }

        public Point AbsLogicalLocation
        {
            set
            {
                float x = ((float)value.X) * scale;
                float y = ((float)value.Y) * scale;
                AbsLocation = new Point((int)x, (int)y);
            }
            get { return new Point((int)(absRect.X / scale), (int)(absRect.Y / scale)); }
        }

        public Point RelLogicalLocation
        {
            set
            {
                float x = ((float)value.X) * scale;
                float y = ((float)value.Y) * scale;
                RelLocation = new Point((int)x, (int)y);
            }
            get { return new Point((int)(RelRect.X / scale), (int)(RelRect.Y / scale)); }
        }

        public ZenControl(float scale, ZenControlBase parent)
            : base(parent)
        {
            this.scale = scale;
            this.parent = parent;
            parent.ControlAdded(this);
        }

        public void Dispose()
        {
            DoDispose();
        }

        protected void MakeMePaint(bool needBackground, RenderMode rm)
        {
            if (paintSuspended) return;
            MakeCtrlPaint(this, needBackground, rm);
        }

        protected void SuspendPaint()
        {
            paintSuspended = true;
        }

        protected void ResumePaint(bool needBackground, RenderMode rm)
        {
            paintSuspended = false;
            MakeMePaint(needBackground, rm);
        }

        protected void DoPaintChildren(Graphics g)
        {
            foreach (ZenControl ctrl in zenChildren)
                ctrl.DoPaint(g);
        }

        public virtual void DoPaint(Graphics g)
        {
            using (Brush b = new SolidBrush(SystemColors.Control))
            {
                g.FillRectangle(b, AbsLocation.X, AbsLocation.Y, Size.Width, Size.Height);
            }
            using (Pen p = new Pen(Color.DarkGray))
            {
                g.DrawRectangle(p, AbsLocation.X, AbsLocation.Y, Size.Width, Size.Height);
            }
            // Paint children
            DoPaintChildren(g);
        }

        protected virtual void OnSizeChanged()
        {

        }

        protected void SetSizeNoInvalidate(Size sz)
        {
            absRect.Size = sz;
        }

        protected virtual void DoDispose()
        {
        }

        protected SizeF MeasureText(string text, Font font, StringFormat fmt)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                return g.MeasureString(text, font, int.MaxValue, fmt);
            }
        }

        private Point translateToControl(ZenControl ctrl, Point pParent)
        {
            int x = pParent.X - ctrl.RelRect.X;
            int y = pParent.Y - ctrl.RelRect.Y;
            return new Point(x, y);
        }

        private ZenControl getControl(Point pParent)
        {
            foreach (ZenControl ctrl in zenChildren)
                if (ctrl.Contains(pParent)) return ctrl;
            return null;
        }

        public virtual bool DoMouseClick(Point p, MouseButtons button)
        {
            ZenControl ctrl = getControl(p);
            if (ctrl != null)
            {
                if (ctrl.DoMouseClick(translateToControl(ctrl, p), button))
                    return true;
            }
            if (MouseClick != null)
            {
                MouseClick(this);
                return true;
            }
            return false;
        }

        public virtual bool DoMouseMove(Point p, MouseButtons button)
        {
            ZenControl ctrl = getControl(p);
            if (ctrl != null)
            {
                if (zenCtrlWithMouse != ctrl)
                {
                    if (zenCtrlWithMouse != null) zenCtrlWithMouse.DoMouseLeave();
                    ctrl.DoMouseEnter();
                    zenCtrlWithMouse = ctrl;
                }
                if (ctrl.DoMouseMove(translateToControl(ctrl, p), button))
                    return true;
            }
            else if (zenCtrlWithMouse != null)
            {
                zenCtrlWithMouse.DoMouseLeave();
                zenCtrlWithMouse = null;
            }
            return false;
        }

        public virtual bool DoMouseDown(Point p, MouseButtons button)
        {
            ZenControl ctrl = getControl(p);
            if (ctrl != null)
            {
                if (ctrl.DoMouseDown(translateToControl(ctrl, p), button))
                    return true;
            }
            return false;
        }

        public virtual bool DoMouseUp(Point p, MouseButtons button)
        {
            ZenControl ctrl = getControl(p);
            if (ctrl != null)
            {
                if (ctrl.DoMouseUp(translateToControl(ctrl, p), button))
                    return true;
            }
            return false;
        }

        public virtual bool DoMouseEnter()
        {
            bool res = false;
            Point pAbs = MousePositionAbs;
            Point pRel = new Point(pAbs.X - AbsLeft, pAbs.Y - AbsTop);
            ZenControl ctrl = getControl(pRel);
            if (ctrl != null)
            {
                if (zenCtrlWithMouse != ctrl)
                {
                    if (zenCtrlWithMouse != null) zenCtrlWithMouse.DoMouseLeave();
                    res = ctrl.DoMouseEnter();
                    zenCtrlWithMouse = ctrl;
                }
            }
            return res;
        }

        public virtual bool DoMouseLeave()
        {
            bool res = false;
            if (zenCtrlWithMouse != null)
            {
                res = zenCtrlWithMouse.DoMouseLeave();
                zenCtrlWithMouse = null;
            }
            return res;
        }

        public bool Contains(Point pParent)
        {
            return RelRect.Contains(pParent);
        }

        internal override sealed void ControlAdded(ZenControl ctrl)
        {
            zenChildren.Add(ctrl);
        }
    }
}
