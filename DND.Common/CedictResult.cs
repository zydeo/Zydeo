using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.Common
{
    public class CedictResult
    {
        public readonly CedictEntry Entry;
        public readonly int HanziHiliteStart;
        public readonly int HanziHiliteLength;

        public CedictResult(CedictEntry entry,
            int hanziHiliteStart, int hanziHiliteLength)
        {
            Entry = entry;
            HanziHiliteStart = hanziHiliteStart;
            HanziHiliteLength = hanziHiliteLength;
        }
    }
}
