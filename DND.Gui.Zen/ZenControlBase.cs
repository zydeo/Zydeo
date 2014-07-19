using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace DND.Gui.Zen
{
    public abstract class ZenControlBase
    {
        protected readonly ZenControlBase Parent;

        protected ZenControlBase(ZenControlBase parent)
        {
            Parent = parent;
        }

        protected virtual void MakeCtrlPaint(ZenControl ctrl, bool needBackground, RenderMode rm)
        {
            Parent.MakeCtrlPaint(ctrl, needBackground, rm);
        }

        internal abstract void ControlAdded(ZenControl ctrl);
        public abstract Rectangle AbsRect { get; }

        protected virtual Point MousePositionAbs
        {
            get { return Parent.MousePositionAbs; }
        }

        protected virtual void AddWinFormsControlToForm(Control c)
        {
            Parent.AddWinFormsControlToForm(c);
        }

        protected virtual void InvokeOnForm(Delegate method)
        {
            Parent.InvokeOnForm(method);
        }
    }
}
