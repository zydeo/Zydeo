using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZD.Common;
using ZD.CedictEngine;

namespace ZD
{
    internal class CedictEngineFactory : ICedictEngineFactory
    {
        public ICedictEngine Create(string dictFileName)
        {
            return new DictEngine(dictFileName);
        }
    }
}
