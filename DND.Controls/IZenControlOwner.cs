using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DND.Controls
{
    public interface IZenControlOwner
    {
        void Invalidate(ZenControl ctrl);
    }
}
