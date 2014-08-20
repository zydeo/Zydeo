using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.MiscTool
{
    internal interface IWorker : IDisposable
    {
        void Init();
        void Work();
        void Finish();
    }
}
