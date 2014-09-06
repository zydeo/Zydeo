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
        public ZenControl(ZenControlBase parent)
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
                g.FillRectangle(b, 0, 0, Width, Height);
            }
            using (Pen p = new Pen(Color.DarkGray))
            {
                g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
            }
            // Paint children
            DoPaintChildren(g);
        }

        protected Point MousePosition
        {
            get
            {
                Point pAbs = MousePositionAbs;
                Point pRel = new Point(pAbs.X - AbsLeft, pAbs.Y - AbsTop);
                return pRel;
            }
        }

        /// <summary>
        /// Gets or sets the form's cursor.
        /// </summary>
        public override sealed Cursor Cursor
        {
            get { return Parent.Cursor; }
            set { Parent.Cursor = value; }
        }
    }
}
