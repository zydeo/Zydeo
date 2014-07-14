using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DND.Controls
{
    public class ZenControl : IDisposable, IZenControlOwner
    {
        public delegate void ClickDelegate(ZenControl sender);
        public event ClickDelegate MouseClick;

        private readonly List<IDisposable> disposables = new List<IDisposable>();
        protected readonly float scale;
        protected readonly IZenControlOwner owner;
        private Rectangle absRect = new Rectangle(0, 0, 0, 0);
        private List<ZenControl> zenChildren = new List<ZenControl>();

        public Size Size
        {
            get { return absRect.Size; }
            set { absRect.Size = value; OnSizeChanged(); Invalidate(); }
        }

        public Rectangle AbsRect
        {
            get { return absRect; }
        }

        public Rectangle RelRect
        {
            get { return new Rectangle(absRect.X - owner.AbsRect.X, absRect.Y - owner.AbsRect.Y, absRect.Width, absRect.Height); }
        }

        public int AbsLeft
        {
            get { return absRect.X; }
            set
            {
                int diff = value - absRect.X;
                absRect.Location = new Point(value, absRect.Location.Y);
                foreach (ZenControl ctrl in zenChildren) ctrl.AbsLeft += diff;
                Invalidate();
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
                Invalidate();
            }
        }

        public int AbsBottom
        {
            get { return absRect.Y + absRect.Height - 1; }
        }

        public int Width
        {
            get { return absRect.Width; }
            set { absRect.Size = new Size(value, absRect.Height); OnSizeChanged(); Invalidate(); }
        }

        public int Height
        {
            get { return absRect.Height; }
            set { absRect.Size = new Size(absRect.Width, value); OnSizeChanged(); Invalidate(); }
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
                Invalidate();
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
                Invalidate();
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
                Invalidate();
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

        public ZenControl(float scale, IZenControlOwner owner)
        {
            this.scale = scale;
            this.owner = owner;
            owner.ControlAdded(this);
        }

        public virtual void Dispose()
        {
            foreach (IDisposable d in disposables) d.Dispose();
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

        protected void AddDisposable(IDisposable d)
        {
            disposables.Add(d);
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
            foreach (ZenControl ctrl in zenChildren)
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
                if (ctrl.DoMouseMove(translateToControl(ctrl, p), button))
                    return true;
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
            // TO-DO: children
            return false;
        }

        public virtual bool DoMouseLeave()
        {
            // TO-DO: children
            return false;
        }

        public bool Contains(Point pParent)
        {
            return RelRect.Contains(pParent);
        }

        public void Invalidate()
        {
            if (owner != null) owner.Invalidate(this);
        }

        public void Invalidate(ZenControl ctrl)
        {
            if (owner != null) owner.Invalidate(this);
        }

        public void ControlAdded(ZenControl ctrl)
        {
            zenChildren.Add(ctrl);
        }
    }
}
