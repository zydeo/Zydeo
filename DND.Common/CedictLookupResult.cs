using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DND.Common
{
    /// <summary>
    /// The result of a dictionary lookup.
    /// </summary>
    public class CedictLookupResult
    {
        /// <summary>
        /// Matching entries retrieved from the dictionary.
        /// </summary>
        public readonly ReadOnlyCollection<CedictResult> Results;

        /// <summary>
        /// <para>Actual search language. If search yields no results based on user's input, but there *are*</para>
        /// <para>results in the other language, engine overrides user's wish.</para>
        /// </summary>
        public readonly SearchLang ActualSearchLang;

        /// <summary>
        /// Ctor: intialize immutable object.
        /// </summary>
        public CedictLookupResult(ReadOnlyCollection<CedictResult> results,
            SearchLang actualSearchLang)
        {
            Results = results;
            ActualSearchLang = actualSearchLang;
        }
    }
}
