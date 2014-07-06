using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.Common
{
    public class CedictResult
    {
        public readonly CedictEntry Entry;

        // TO-DO: highlights

        public CedictResult(CedictEntry entry)
        {
            Entry = entry;
        }
    }
}
