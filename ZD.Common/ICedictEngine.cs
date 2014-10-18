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
    }
}
