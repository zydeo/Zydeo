using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace DND.Controls
{
    public enum RenderMode
    {
        Update,
        Invalidate,
        None
    }

    public abstract class ZenControlBase
    {
        internal abstract void MakeCtrlPaint(ZenControl ctrl, bool needBackground, RenderMode rm);
        internal abstract void ControlAdded(ZenControl ctrl);
        public abstract Rectangle AbsRect { get; }
        internal abstract Point MousePositionAbs { get; }
        internal abstract void AddWinFormsControlToForm(Control c);
        internal abstract void InvokeOnForm(Delegate method);
    }
}
