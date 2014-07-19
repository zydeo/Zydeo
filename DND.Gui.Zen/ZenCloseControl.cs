using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace DND.Gui.Zen
{
    internal class ZenCloseControl : ZenControl
    {
        private bool isHover = false;

        public ZenCloseControl(float scale, ZenTabbedForm owner)
            : base(scale, owner)
        { }

        public override void DoPaint(Graphics g)
        {
            Color clr = ZenParams.CloseColorBase;
            if (isHover) clr = ZenParams.CloseColorHover;
            using (Brush b = new SolidBrush(clr))
            {
                g.FillRectangle(b, AbsLocation.X, AbsLocation.Y, Size.Width, Size.Height);
            }
            using (Pen p = new Pen(Color.DarkGray))
            {
                g.DrawLine(p, AbsLocation.X, AbsLocation.Y, AbsLocation.X, AbsLocation.Y + Height - 1);
                g.DrawLine(p, AbsLocation.X + Width - 1, AbsLocation.Y, AbsLocation.X + Width - 1, AbsLocation.Y + Height - 1);
                g.DrawLine(p, AbsLocation.X, AbsLocation.Y + Height - 1, AbsLocation.X + Width - 1, AbsLocation.Y + Height - 1);
            }
        }

        public override void DoMouseEnter()
        {
            isHover = true;
            MakeMePaint(false, RenderMode.Invalidate);
        }

        public override void DoMouseLeave()
        {
            isHover = false;
            MakeMePaint(false, RenderMode.Invalidate);
        }
    }
}
