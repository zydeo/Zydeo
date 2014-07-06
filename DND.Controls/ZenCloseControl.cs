using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace DND.Controls
{
    internal class ZenCloseControl : ZenControl
    {
        private bool isHover = false;

        public ZenCloseControl(float scale, IZenControlOwner owner)
            : base(scale, owner)
        { }

        public override void DoPaint(Graphics g)
        {
            Color clr = ZenParams.CloseColorBase;
            if (isHover) clr = ZenParams.CloseColorHover;
            using (Brush b = new SolidBrush(clr))
            {
                g.FillRectangle(b, Location.X, Location.Y, Size.Width, Size.Height);
            }
            using (Pen p = new Pen(Color.DarkGray))
            {
                g.DrawLine(p, Location.X, Location.Y, Location.X, Location.Y + Height - 1);
                g.DrawLine(p, Location.X + Width - 1, Location.Y, Location.X + Width - 1, Location.Y + Height - 1);
                g.DrawLine(p, Location.X, Location.Y + Height - 1, Location.X + Width - 1, Location.Y + Height - 1);
            }
        }

        public override void DoMouseEnter()
        {
            base.DoMouseEnter();
            isHover = true;
            Invalidate();
        }

        public override void DoMouseLeave()
        {
            base.DoMouseEnter();
            isHover = false;
            Invalidate();
        }
    }
}
