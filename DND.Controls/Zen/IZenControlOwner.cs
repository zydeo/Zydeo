using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DND.Controls
{
    public enum RenderMode
    {
        Update,
        Invalidate,
        None
    }

    public interface IZenControlOwner
    {
        void MakeCtrlPaint(ZenControl ctrl, bool needBackground, RenderMode rm);
        void ControlAdded(ZenControl ctrl);
        Rectangle AbsRect { get; }
        Point MousePositionAbs { get; }
    }
}
