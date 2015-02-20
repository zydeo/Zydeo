using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// Yes-man: always returns true. Used for the Noto fonts.
    /// </summary>
    public class FontCoverageFull : IFontCoverage
    {
        /// <summary>
        /// See <see cref="IFontCoverage.GetCoverage"/>.
        /// </summary>
        public FontCoverageFlags GetCoverage(char c)
        {
            return FontCoverageFlags.Simp | FontCoverageFlags.Trad;
        }
    }
}
