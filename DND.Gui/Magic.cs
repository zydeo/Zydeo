using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

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

        /// <summary>
        /// Character used to measure genuine visual rectangle of Hanzi characters in a font.
        /// </summary>
        /// <remarks>This character is very tall and pretty wide. Oh beautiful life.</remarks>
        public readonly static string HanziMeasureTestChar = "蠹";

        /// <summary>
        /// Font family for hard-wired texts on buttons next to search input field.
        /// </summary>
        /// <remars>
        /// Other fonts tested:
        /// DFKai-SB
        /// 䡡湄楮札䍓ⵆ潮瑳
        /// Noto Sans S Chinese Regular
        /// </remars>
        public static readonly string ZhoButtonFontFamily = "Segoe UI";

        /// <summary>
        /// Font size for hard-wired texts on buttons next to search input field.
        /// </summary>
        public static readonly float ZhoButtonFontSize = 18.0F;

        /// <summary>
        /// Font family for Hanzi in lookup results and char picker: simplified.
        /// </summary>
        public static readonly string ZhoSimpContentFontFamily = "AR PL UKai CN";

        /// <summary>
        /// Font family for Hanzi in lookup results and char picker: traditional.
        /// </summary>
        public static readonly string ZhoTradContentFontFamily = "AR PL UKai CN";

        /// <summary>
        /// Font size for Hanzi heading in each displayed dictionary entry. Also in character picker.
        /// </summary>
        public static readonly float ZhoResultFontSize = 22.0F;

        /// <summary>
        /// Font family for lemma (domain, equiv, note) in displayed entries. Also for Hanzi ranges.
        /// </summary>
        public static readonly string LemmaFontFamily = "Tahoma";

        /// <summary>
        /// Font size for lemma (domain, equiv, note) in displayed entries. Hanzi size derived from this.
        /// </summary>
        public static readonly float LemmaFontSize = 10.0F;

        /// <summary>
        /// Font family for Pinyin in heading of displayed entries.
        /// </summary>
        public static readonly string PinyinFontFamily = "Tahoma";

        /// <summary>
        /// Font size for Pinyin in heading of displayed entries.
        /// </summary>
        public static readonly float PinyinFontSize = 11.0F;

        /// <summary>
        /// Base color of highlights in a result (auto-faded gradient at ends).
        /// </summary>
        public static readonly Color HiliteColor = Color.FromArgb(255, 232, 189);

        /// <summary>
        /// Hover color of text in results that acts as a lookup link.
        /// </summary>
        public static readonly Color LinkHoverColor = Color.Maroon;

        /// <summary>
        /// Font size of results count box in bottom right.
        /// </summary>
        public static readonly float ResultsCountFontSize = 9.0F;
    }
}
