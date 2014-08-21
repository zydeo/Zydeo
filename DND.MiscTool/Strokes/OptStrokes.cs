using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.MiscTool
{
    internal class OptStrokes
    {
        public readonly string CharStatsFileName;
        public readonly string OrigStrokesFileName;
        public readonly string OutFileName;

        public OptStrokes(string charStatsFileName, string origStrokesFileName, string outFileName)
        {
            CharStatsFileName = charStatsFileName;
            OrigStrokesFileName = origStrokesFileName;
            OutFileName = outFileName;
        }
    }
}
