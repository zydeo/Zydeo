using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// Provides Unihan information about Hanzi for CHDICT's interactive entry editor.
    /// </summary>
    public interface IUniHanziRepo
    {
        /// <summary>
        /// <para>Returns information about a bunch of Hanzi.</para>
        /// <para>Accepts A-Z, 0-9. Returns null in array for other non-ideographs, and for unknown Hanzi.</para>
        /// </summary>
        UniHanziInfo[] GetInfo(char[] c);
    }
}
