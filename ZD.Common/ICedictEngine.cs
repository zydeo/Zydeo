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
        ICedictEngine Create(string dictFileName);

        /// <summary>
        /// Gest information about dictionary, without loading indexes.
        /// </summary>
        ICedictInfo GetInfo(string dictFileName);
    }
}
