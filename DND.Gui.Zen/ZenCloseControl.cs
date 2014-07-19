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

        public ZenCloseControl(ZenTabbedForm owner)
            : base(owner)
        { }

        public override void DoPaint(Graphics g)
        {
            Color clr = ZenParams.CloseColorBase;
            if (isHover) clr = ZenParams.CloseColorHover;
            using (Brush b = new SolidBrush(clr))
            {
                g.FillRectangle(b, 0, 1, Width, Height - 1);
            }
            using (Pen p = new Pen(Color.DarkGray))
            {
                g.DrawLine(p, 0, 1, 0, Height - 1);
                g.DrawLine(p, Width - 1, 1, Width - 1, Height - 1);
                g.DrawLine(p, 0, Height - 1, Width - 1, Height - 1);
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
