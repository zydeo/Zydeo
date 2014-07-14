using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DND.Controls
{
    public interface IZenControlOwner
    {
        void Invalidate(ZenControl ctrl);
        void ControlAdded(ZenControl ctrl);
        Rectangle AbsRect { get; }
        Point MousePositionAbs { get; }
    }
}
