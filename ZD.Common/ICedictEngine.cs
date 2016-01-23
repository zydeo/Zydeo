using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// Dictionary engine, exposed for lookup.
    /// </summary>
    public interface ICedictEngine
    {
        /// <summary>
        /// Performs a dictionary lookup.
        /// </summary>
        /// <param name="what">The query string.</param>
        CedictLookupResult Lookup(string what, SearchScript script, SearchLang lang);

        /// <summary>
        /// Retrieves a dictionary entry.
        /// </summary>
        /// <param name="entryId">The ID of the requested entry.</param>
        /// <returns>The retrieved entry.</returns>
        CedictEntry GetEntry(int entryId);

        /// <summary>
        /// Retrieves hanzi information (stroke order etc.) for the provided Unicode code point, or null if no info available.
        /// </summary>
        /// <param name="c">The Hanzi as a Unicode character.</param>
        /// <returns>Information about the Hanzi, or null.</returns>
        HanziInfo GetHanziInfo(char c);

        void GetFirstWords(out string hanzi, out string target);        /// <summary>
        /// Gets the first Hanzi headword and the first target-language word in the dictionary, to start a complete walkthrough.
        /// </summary>

        /// <summary>
        /// Retrieves preceding and following word for full walkthrough.
        /// </summary>
        void GetPrevNextWords(string curr, bool isTarget, out string prev, out string next);
    }

    /// <summary>
    /// Information about this compiled Cedict dictionary.
    /// </summary>
    public interface ICedictInfo
    {
        /// <summary>
        /// Compilation date (only year/month/day are relevant).
        /// </summary>
        DateTime Date { get; }
        /// <summary>
        /// Number of entries in actual, compiled dictionary.
        /// </summary>
        int EntryCount { get; }
    }

    /// <summary>
    /// A factory for creating dictionary engines.
    /// </summary>
    public interface ICedictEngineFactory
    {
        /// <summary>
        /// Creates a dictionary engine for the dictionary in the provided file.
        /// </summary>
        /// <param name="dictFileName">Name of the compiled binary dictionary.</param>
        /// <param name="cvr">Font coverage info provider for lookup filtering.</param>
        ICedictEngine Create(string dictFileName, IFontCoverage cvr);

        /// <summary>
        /// Gest information about dictionary, without loading indexes.
        /// </summary>
        ICedictInfo GetInfo(string dictFileName);
    }
}
