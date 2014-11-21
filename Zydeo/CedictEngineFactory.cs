using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZD.Common;
using ZD.CedictEngine;

namespace ZD
{
    /// <summary>
    /// Implements <see cref="ZD.Common.ICedictEngineFactory"/>.
    /// </summary>
    internal class CedictEngineFactory : ICedictEngineFactory
    {
        /// <summary>
        /// See <see cref="ZD.Common.ICedictEngineFactory.Create"/>.
        /// </summary>
        public ICedictEngine Create(string dictFileName)
        {
            return new DictEngine(dictFileName);
        }

        /// <summary>
        /// See <see cref="ZD.Common.ICedictEngineFactory.GetInfo"/>.
        /// </summary>
        public ICedictInfo GetInfo(string dictFileName)
        {
            return DictEngine.GetInfo(dictFileName);
        }
    }
}
