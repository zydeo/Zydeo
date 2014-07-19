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
        public delegate void ClickDelegate(ZenControl sender);
        public event ClickDelegate MouseClick;

        private ZenControl zenCtrlWithMouse = null;

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

        public virtual bool DoMouseClick(Point p, MouseButtons button)
        {
            ZenControl ctrl = GetControl(p);
            if (ctrl != null)
            {
                if (ctrl.DoMouseClick(TranslateToControl(ctrl, p), button))
                    return true;
            }
            if (MouseClick != null)
            {
                MouseClick(this);
                return true;
            }
            return false;
        }

        public virtual bool DoMouseMove(Point p, MouseButtons button)
        {
            ZenControl ctrl = GetControl(p);
            if (ctrl != null)
            {
                if (zenCtrlWithMouse != ctrl)
                {
                    if (zenCtrlWithMouse != null) zenCtrlWithMouse.DoMouseLeave();
                    ctrl.DoMouseEnter();
                    zenCtrlWithMouse = ctrl;
                }
                if (ctrl.DoMouseMove(TranslateToControl(ctrl, p), button))
                    return true;
            }
            else if (zenCtrlWithMouse != null)
            {
                zenCtrlWithMouse.DoMouseLeave();
                zenCtrlWithMouse = null;
            }
            return false;
        }

        public virtual bool DoMouseDown(Point p, MouseButtons button)
        {
            ZenControl ctrl = GetControl(p);
            if (ctrl != null)
            {
                if (ctrl.DoMouseDown(TranslateToControl(ctrl, p), button))
                    return true;
            }
            return false;
        }

        public virtual bool DoMouseUp(Point p, MouseButtons button)
        {
            ZenControl ctrl = GetControl(p);
            if (ctrl != null)
            {
                if (ctrl.DoMouseUp(TranslateToControl(ctrl, p), button))
                    return true;
            }
            return false;
        }

        public virtual bool DoMouseEnter()
        {
            bool res = false;
            Point pAbs = MousePositionAbs;
            Point pRel = new Point(pAbs.X - AbsLeft, pAbs.Y - AbsTop);
            ZenControl ctrl = GetControl(pRel);
            if (ctrl != null)
            {
                if (zenCtrlWithMouse != ctrl)
                {
                    if (zenCtrlWithMouse != null) zenCtrlWithMouse.DoMouseLeave();
                    res = ctrl.DoMouseEnter();
                    zenCtrlWithMouse = ctrl;
                }
            }
            return res;
        }

        public virtual bool DoMouseLeave()
        {
            bool res = false;
            if (zenCtrlWithMouse != null)
            {
                res = zenCtrlWithMouse.DoMouseLeave();
                zenCtrlWithMouse = null;
            }
            return res;
        }

        public bool Contains(Point pParent)
        {
            return RelRect.Contains(pParent);
        }
    }
}
