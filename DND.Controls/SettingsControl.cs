using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace DND.Controls
{
    internal class SettingsControl : ZenControl
    {
        public SettingsControl(float scale, ZenControlBase owner)
            : base(scale, owner)
        { }

        public override void DoPaint(Graphics g)
        {
            using (Brush b = new SolidBrush(Color.White))
            {
                g.FillRectangle(b, AbsLocation.X, AbsLocation.Y, Size.Width, Size.Height);
            }
        }
    }
}
