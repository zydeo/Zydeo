using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DND.Common;
using DND.CedictEngine;

namespace Sandbox
{
    internal class CedictEngineFactory : ICedictEngineFactory
    {
        public ICedictEngine Create(string dictFileName)
        {
            return new DictEngine(dictFileName);
        }
    }
}
