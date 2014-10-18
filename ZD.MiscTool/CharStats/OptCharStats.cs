using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.MiscTool
{
    internal class OptCharStats
    {
        public readonly string StrokesFileName;
        public readonly string StrokesTypesFileName;
        public readonly string CedictFileName;
        public readonly string OutFileName;

        public OptCharStats(string strokesFileName,
            string strokesTypesFileName,
            string cedictFileName,
            string outFileName)
        {
            StrokesFileName = strokesFileName;
            StrokesTypesFileName = strokesTypesFileName;
            CedictFileName = cedictFileName;
            OutFileName = outFileName;
        }
    }
}
