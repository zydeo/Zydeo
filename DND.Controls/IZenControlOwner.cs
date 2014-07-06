using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.Controls
{
    internal interface IZenControlOwner
    {
        void Invalidate(ZenControl ctrl);
    }
}
