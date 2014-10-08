using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.Common
{
    /// <summary>
    /// One target-text highlight to show in lookup result's entry
    /// </summary>
    public class CedictTargetHighlight
    {
        /// <summary>
        /// Index of sense in entry that contains the highlight in its equiv.
        /// </summary>
        public readonly int SenseIx;

        /// <summary>
        /// Index of (Latin) text run in equiv of sense that contains the highlight.
        /// </summary>
        public readonly int RunIx;

        /// <summary>
        /// Start of highlight in Latin text run.
        /// </summary>
        public readonly int HiliteStart;

        /// <summary>
        /// Length of highlight in Latin text run.
        /// </summary>
        public readonly int HiliteLength;

        /// <summary>
        /// Ctor: init immutable instance.
        /// </summary>
        public CedictTargetHighlight(int senseIx, int runIx, int hlStart, int hlLength)
        {
            SenseIx = senseIx;
            RunIx = runIx;
            HiliteStart = hlStart;
            HiliteLength = hlLength;
        }
    }
}
