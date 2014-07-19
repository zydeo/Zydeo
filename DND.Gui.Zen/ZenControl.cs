using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DND.Gui.Zen
{
    public class ZenControl : ZenControlBase, IDisposable
    {
        public ZenControl(float scale, ZenControlBase parent)
            : base(parent)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override sealed void RegisterWinFormsControl(Control c)
        {
            base.RegisterWinFormsControl(c);
        }

        internal sealed override void AddWinFormsControl(Control c)
        {
            base.AddWinFormsControl(c);
        }

        internal sealed override void RemoveWinFormsControl(Control c)
        {
            base.RemoveWinFormsControl(c);
        }

        public sealed override Size LogicalSize
        {
            get { return base.LogicalSize; }
            set { base.LogicalSize = value; }
        }

        public sealed override Rectangle AbsRect
        {
            get { return base.AbsRect; }
        }

        internal override sealed void MakeCtrlPaint(ZenControlBase ctrl, bool needBackground, RenderMode rm)
        {
            base.MakeCtrlPaint(ctrl, needBackground, rm);
        }

        public override void DoPaint(Graphics g)
        {
            using (Brush b = new SolidBrush(SystemColors.Control))
            {
                g.FillRectangle(b, AbsLocation.X, AbsLocation.Y, Size.Width, Size.Height);
            }
            using (Pen p = new Pen(Color.DarkGray))
            {
                g.DrawRectangle(p, AbsLocation.X, AbsLocation.Y, Size.Width, Size.Height);
            }
            // Paint children
            DoPaintChildren(g);
        }
    }
}
