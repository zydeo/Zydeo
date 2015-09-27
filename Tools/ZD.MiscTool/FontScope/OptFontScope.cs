using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.MiscTool
{
    internal class OptFontScope
    {
        public readonly string CedictFileName;
        public readonly string GBFileName;
        public readonly string Big5FileName;
        public readonly string UniFileName;
        public readonly string ZFileName;
        public readonly string OutCharFileName;
        public readonly string OutS2TFileName;

        public OptFontScope(string cedictFileName,
            string gbFileName, string big5FileName, string uniFileName, string zFileName,
            string outCharFileName, string outS2TFileName)
        {
            CedictFileName = cedictFileName;
            GBFileName = gbFileName;
            Big5FileName = big5FileName;
            UniFileName = uniFileName;
            ZFileName = zFileName;
            OutCharFileName = outCharFileName;
            OutS2TFileName = outS2TFileName;
        }
    }
}
