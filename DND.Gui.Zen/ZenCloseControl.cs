﻿using System;
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
                g.FillRectangle(b, AbsLocation.X, AbsLocation.Y + 1, Size.Width, Size.Height - 1);
            }
            using (Pen p = new Pen(Color.DarkGray))
            {
                g.DrawLine(p, AbsLocation.X, AbsLocation.Y + 1, AbsLocation.X, AbsLocation.Y + Height - 1);
                g.DrawLine(p, AbsLocation.X + Width - 1, AbsLocation.Y + 1, AbsLocation.X + Width - 1, AbsLocation.Y + Height - 1);
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
