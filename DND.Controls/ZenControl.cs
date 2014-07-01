using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DND.Controls
{
    internal class ZenControl : IDisposable
    {
        public delegate void ClickDelegate(ZenControl sender);
        public event ClickDelegate MouseClick;

        private readonly List<IDisposable> disposables = new List<IDisposable>();
        protected readonly float scale;
        protected readonly IZenControlOwner owner;
        private Size size;
        private Point location;
        private Rectangle myRect;

        public Size Size
        {
            get { return size; }
            set { size = value; calcRect(); Invalidate(); }
        }

        public int Left
        {
            get { return location.X; }
            set { location = new Point(value, location.Y); Invalidate(); }
        }

        public int Right
        {
            get { return location.X + size.Width - 1; }
        }

        public int Top
        {
            get { return location.Y; }
            set { location = new Point(location.X, value); Invalidate(); }
        }

        public int Bottom
        {
            get { return location.Y + size.Height - 1; }
        }

        public int Width
        {
            get { return size.Width; }
            set { size = new Size(value, size.Height); calcRect(); Invalidate(); }
        }

        public int Height
        {
            get { return size.Height; }
            set { size = new Size(size.Width, value); calcRect(); Invalidate(); }
        }

        public Size LogicalSize
        {
            set
            {
                float w = ((float)value.Width) * scale;
                float h = ((float)value.Height) * scale;
                size = new Size((int)w, (int)h);
                calcRect();
            }
            get { return new Size((int)(size.Width / scale), (int)(size.Height / scale)); }
        }

        public Point Location
        {
            get { return location; }
            set { location = value; calcRect(); }
        }

        public Point LogicalLocation
        {
            set
            {
                float x = ((float)value.X) * scale;
                float y = ((float)value.Y) * scale;
                location = new Point((int)x, (int)y);
                calcRect();
            }
            get { return new Point((int)(location.X / scale), (int)(location.Y / scale)); }
        }

        public ZenControl(float scale, IZenControlOwner owner)
        {
            this.scale = scale;
            this.owner = owner;
        }

        public void Dispose()
        {
            foreach (IDisposable d in disposables) d.Dispose();
        }

        public virtual void DoPaint(Graphics g)
        {
            using (Brush b = new SolidBrush(SystemColors.Control))
            {
                g.FillRectangle(b, Location.X, Location.Y, Size.Width, Size.Height);
            }
            using (Pen p = new Pen(Color.DarkGray))
            {
                g.DrawRectangle(p, Location.X, Location.Y, Size.Width, Size.Height);
            }
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

        public virtual void DoMouseClick(Point p, MouseButtons button)
        {
            if (MouseClick != null) MouseClick(this);
        }

        public virtual void DoMouseMove(Point p, MouseButtons button)
        {
        }

        public virtual void DoMouseDown(Point p, MouseButtons button)
        {
        }

        public virtual void DoMouseUp(Point p, MouseButtons button)
        {
        }

        public virtual void DoMouseEnter()
        {
        }

        public virtual void DoMouseLeave()
        {
        }

        private void calcRect()
        {
            myRect = new Rectangle(location, size);
        }

        public bool Contains(Point p)
        {
            return myRect.Contains(p);
        }

        public void Invalidate()
        {
            if (owner != null) owner.Invalidate(this);
        }
    }
}
