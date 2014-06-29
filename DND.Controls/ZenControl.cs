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
    public class ZenControl
    {
        public delegate void ClickDelegate(ZenControl sender);
        public event ClickDelegate MouseClick;

        private readonly float scale;
        private IZenControlOwner owner;
        private Size size;
        private Point location;
        private Rectangle myRect;

        public Size Size
        {
            get { return size; }
            set { size = value; calcRect(); }
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

        public IZenControlOwner Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        public ZenControl(float scale)
        {
            this.scale = scale;
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
