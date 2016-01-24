using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// The result of a dictionary lookup.
    /// </summary>
    public class CedictLookupResult
    {
        /// <summary>
        /// The query string, repeated, and possibly normalized.
        /// </summary>
        public readonly string Query;

        /// <summary>
        /// Entry provider for retrieving actual dictionary entries. Caller owns it (must dispose it).
        /// </summary>
        public readonly ICedictEntryProvider EntryProvider;

        /// <summary>
        /// Results of the dictionary query.
        /// </summary>
        public readonly ReadOnlyCollection<CedictResult> Results;

        /// <summary>
        /// Annotation results (if input was Hanzi and yielded no results as a whole).
        /// </summary>
        public readonly ReadOnlyCollection<CedictAnnotation> Annotations;

        /// <summary>
        /// <para>Actual search language. If search yields no results based on user's input, but there *are*</para>
        /// <para>results in the other language, engine overrides user's wish.</para>
        /// </summary>
        public readonly SearchLang ActualSearchLang;

        /// <summary>
        /// Ctor: intialize immutable object.
        /// </summary>
        public CedictLookupResult(ICedictEntryProvider entryProvider, string query,
            List<CedictResult> results, List<CedictAnnotation> annotations, SearchLang actualSearchLang)
        {
            Query = query;
            EntryProvider = entryProvider;
            Results = new ReadOnlyCollection<CedictResult>(results);
            Annotations = new ReadOnlyCollection<CedictAnnotation>(annotations);
            ActualSearchLang = actualSearchLang;
        }
    }
}
