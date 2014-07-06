using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace DND.Controls
{
    internal class ZenScrollbarCtrl : ZenControl
    {
        private bool isHover = false;

        public ZenScrollbarCtrl(float scale, IZenControlOwner owner)
            : base(scale, owner)
        { }

        public override void DoPaint(Graphics g)
        {

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
