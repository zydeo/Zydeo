using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using DND.Gui.Zen;

namespace DND.Gui
{
    internal class SettingsControl : ZenControl
    {
        public SettingsControl(ZenControlBase owner)
            : base(owner)
        { }

        public override void DoPaint(Graphics g)
        {
            using (Brush b = new SolidBrush(Color.White))
            {
                g.FillRectangle(b, 0, 0, Width, Height);
            }
        }
    }
}
