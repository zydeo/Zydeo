using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.Gui
{
    /// <summary>
    /// All kinds of magic constants.
    /// </summary>
    internal class Magic
    {
        /// <summary>
        /// The "looseness" of lookup, 0-1, higher == looser, looser more computationally intensive.
        /// </summary>
        public const double HanziLookupLooseness = 0.25;

        /// <summary>
        /// Maximum number of results to return with each lookup.
        /// </summary>
        public const int HanziLookupNumResults = 15;

        /// <summary>
        /// Name of compiled, binary dictionary file.
        /// </summary>
        public const string DictFileName = "cedict-zydeo.bin";

        /// <summary>
        /// Non-localized text of script selector button: simplified.
        /// </summary>
        public readonly static string SearchSimp = "语";

        /// <summary>
        /// Non-localized text of script selector button: traditional.
        /// </summary>
        public readonly static string SearchTrad = "語";

        /// <summary>
        /// Non-localized text of script selector button: S+T.
        /// </summary>
        public readonly static string SearchBoth = "语+語";

        /// <summary>
        /// Non-localized text of search language button: English.
        /// </summary>
        public readonly static string SearchLangEng = "英";

        /// <summary>
        /// Non-localized text of search language button: Chinese.
        /// </summary>
        public readonly static string SearchLangZho = "中";
    }
}
