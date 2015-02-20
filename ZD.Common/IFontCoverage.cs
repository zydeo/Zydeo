using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// Specifies a code point's coverage in the simplified/traditional pair of fonts queried.
    /// </summary>
    [Flags]
    public enum FontCoverageFlags
    {
        None = 0,
        Simp = 1,
        Trad = 2,
    }

    /// <summary>
    /// Provides information about a font's coverage of specific code points.
    /// </summary>
    public interface IFontCoverage
    {
        /// <summary>
        /// Returns information about a code point's coverage in the simp/trad font pair represented by the implementor.
        /// </summary>
        FontCoverageFlags GetCoverage(char c);
    }
}
