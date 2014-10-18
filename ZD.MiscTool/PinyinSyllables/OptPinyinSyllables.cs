using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.MiscTool
{
    internal class OptPinyinSyllables
    {
        public readonly string CedictFileName;
        public readonly string OutFileName;

        public OptPinyinSyllables(string cedictFileName, string outFileName)
        {
            CedictFileName = cedictFileName;
            OutFileName = outFileName;
        }
    }
}
