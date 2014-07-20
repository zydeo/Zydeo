using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DND.Common
{
    public class CedictLookupResult
    {
        public readonly ReadOnlyCollection<CedictResult> Results;

        public readonly SearchLang ActualSearchLang;

        public CedictLookupResult(ReadOnlyCollection<CedictResult> results,
            SearchLang actualSearchLang)
        {
            Results = results;
            ActualSearchLang = actualSearchLang;
        }
    }
}
