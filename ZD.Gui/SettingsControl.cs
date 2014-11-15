using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using ZD.Gui.Zen;

namespace ZD.Gui
{
    internal class SettingsControl : ZenControl
    {
        private SettingsControlWin ctrlWin;

        public SettingsControl(ZenControlBase owner)
            : base(owner)
        {
            ctrlWin = new SettingsControlWin();
            ctrlWin.Size = Size;
            ctrlWin.Location = AbsLocation;
            RegisterWinFormsControl(ctrlWin);
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            if (ctrlWin.InvokeRequired)
            {
                InvokeOnForm((MethodInvoker)delegate
                {
                    ctrlWin.Location = AbsLocation;
                    ctrlWin.Size = Size;
                });
            }
            else
            {
                ctrlWin.Location = AbsLocation;
                ctrlWin.Size = Size;
            }
        }

        public override void DoPaint(Graphics g)
        {
            using (Brush b = new SolidBrush(ZenParams.WindowColor))
            {
                g.FillRectangle(b, 0, 0, Width, Height);
            }
        }
    }
}
