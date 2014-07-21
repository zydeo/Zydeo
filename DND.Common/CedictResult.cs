using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.Common
{
    public class CedictResult
    {
        public readonly CedictEntry Entry;
        public readonly int SimpHiliteStart;
        public readonly int SimpHiliteLength;
        public readonly int TradHiliteStart;
        public readonly int TradHiliteLength;

        public CedictResult(CedictEntry entry,
            int simpHiliteStart, int simpHiliteLength,
            int tradHiliteStart, int tradHiliteLength)
        {
            Entry = entry;
            SimpHiliteStart = simpHiliteStart;
            SimpHiliteLength = simpHiliteLength;
            TradHiliteStart = tradHiliteStart;
            TradHiliteLength = tradHiliteLength;
        }
    }
}
