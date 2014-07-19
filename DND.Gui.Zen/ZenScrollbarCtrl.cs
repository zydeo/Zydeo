using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace DND.Gui.Zen
{
    internal class ZenScrollbarCtrl : ZenControl
    {
        private bool isHover = false;

        public ZenScrollbarCtrl(float scale, ZenControl owner)
            : base(scale, owner)
        { }

        public override void DoPaint(Graphics g)
        {

        }

        public override bool DoMouseEnter()
        {
            isHover = true;
            MakeMePaint(false, RenderMode.Invalidate);
            return true;
        }

        public override bool DoMouseLeave()
        {
            isHover = false;
            MakeMePaint(false, RenderMode.Invalidate);
            return true;
        }
    }
}
