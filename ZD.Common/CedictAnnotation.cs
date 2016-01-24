using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// One annotated entry in long Hanzi lookup input.
    /// </summary>
    public class CedictAnnotation
    {
        /// <summary>
        /// ID of the CEDICT entry.
        /// </summary>
        public readonly int EntryId;

        /// <summary>
        /// Indicates if annotation comes from entry's simplified or traditional HW.
        /// Never "Both" - then falls back to one or the other.
        /// </summary>
        public readonly SearchScript Script;

        /// <summary>
        /// Annotation's start position in query string.
        /// </summary>
        public readonly int StartInQuery;

        /// <summary>
        /// Annotation's length in query string.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public readonly int LengthInQuery;

        /// <summary>
        /// Ctor: init immutable instance.
        /// </summary>
        public CedictAnnotation(int entryId, SearchScript script, int start, int length)
        {
            EntryId = entryId;
            Script = script;
            StartInQuery = start;
            LengthInQuery = length;
        }
    }
}
