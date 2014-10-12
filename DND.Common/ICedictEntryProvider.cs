using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.Common
{
    /// <summary>
    /// <para>Provides a means to retrieve actual Cedict entries based on their ID.</para>
    /// <para>Dictionary lookup only returns IDs, not actual entries, in results list.</para>
    /// <para>Returned by dictionary engine after lookup. Must be disposed by caller.</para>
    /// </summary>
    public interface ICedictEntryProvider : IDisposable
    {
        /// <summary>
        /// <para>Retrieves a dictionary entry identified by the provided ID.</para>
        /// <para>Not thread-safe!</para>
        /// </summary>
        /// <param name="entryId">The ID, as returned in <see cref="CedictResult"/>.</param>
        /// <returns>The retrieved dictionary entry.</returns>
        CedictEntry GetEntry(int entryId);
    }
}
