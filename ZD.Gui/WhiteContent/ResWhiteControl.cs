using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using ZD.Gui.Zen;
using ZD.Common;

namespace ZD.Gui.WhiteContent
{
    internal class ResWhiteControl : ZenControl
    {
        private readonly Control winCtrl;
        private readonly ITextProvider tprov;
        private readonly ZenGradientButton btnUpdate;

        public ResWhiteControl(ZenControlBase owner, Control winCtrl, ITextProvider tprov)
            : base(owner)
        {
            this.winCtrl = winCtrl;
            
            btnUpdate = new ZenGradientButton(this);
            btnUpdate.Height = (int)(Scale * 24F);
            btnUpdate.Width = (int)(Scale * 200F);
            btnUpdate.SetFont(ZenParams.GenericFontFamily, 10F);
            btnUpdate.Text = "Update now";

            doArrange();
            RegisterWinFormsControl(winCtrl);
        }

        public override void Dispose()
        {
            base.Dispose();
            winCtrl.Dispose();
        }

        protected override void OnSizeChanged()
        {
            doArrange();
        }

        private void doArrange()
        {
            int padHoriz = (int)(Scale * 25F);
            Point wcLoc = new Point(AbsLeft + padHoriz, AbsTop + padHoriz);
            int wcWidth = Width - 2 * padHoriz;
            if (winCtrl.InvokeRequired)
            {
                InvokeOnForm((MethodInvoker)delegate
                {
                    winCtrl.Location = wcLoc;
                    winCtrl.Width = wcWidth;
                });
            }
            else
            {
                winCtrl.Location = wcLoc;
                winCtrl.Width = wcWidth;
            }

            btnUpdate.RelLeft = Width - padHoriz - btnUpdate.Width;
            btnUpdate.AbsTop = winCtrl.Bottom;
        }

        public override void DoPaint(Graphics g)
        {
            // Background
            using (Brush b = new SolidBrush(ZenParams.WindowColor))
            {
                g.FillRectangle(b, new Rectangle(0, 0, Width, Height));
            }
            // Border
            using (Pen p = new Pen(ZenParams.BorderColor))
            {
                g.DrawLine(p, 0, 0, Width, 0);
                g.DrawLine(p, Width - 1, 0, Width - 1, Height);
                g.DrawLine(p, Width - 1, Height - 1, 0, Height - 1);
                g.DrawLine(p, 0, Height - 1, 0, 0);
            }
            DoPaintChildren(g);
        }
    }
}
