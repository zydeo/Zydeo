using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ZD.Gui
{
    /// <summary>
    /// All kinds of magic constants.
    /// </summary>
    internal class Magic
    {
        /// <summary>
        /// The "looseness" of lookup, 0-1, higher == looser, looser more computationally intensive.
        /// </summary>
        public readonly static double HanziLookupLooseness = 0.25;

        /// <summary>
        /// Maximum number of results to return with each lookup.
        /// </summary>
        public readonly static int HanziLookupNumResults = 15;

        /// <summary>
        /// Name of compiled, binary dictionary file.
        /// </summary>
        public readonly static string DictFileName = "cedict-zydeo.bin";

        /// <summary>
        /// Name of compiled, binary stroke recognition data.
        /// </summary>
        public readonly static string StrokesFileName = "strokes-zydeo.bin";

        /// <summary>
        /// Subfolder in user's appdata where Zydeo stores its persistent user-specific data.
        /// </summary>
        public readonly static string ZydeoUserFolder = "Zydeo";

        /// <summary>
        /// Name of file where Zydeo's settings are stored in its user data folder.
        /// </summary>
        public readonly static string ZydeoSettingsFile = "Settings.xml";

        /// <summary>
        /// Window's default logical (unscaled) size.
        /// </summary>
        public readonly static Size WinDefaultLogicalSize = new Size(800, 500);

        /// <summary>
        /// Window's minimum logical (unscaled) size.
        /// </summary>
        public readonly static Size WinMinimumLogicalSize = new Size(600, 450);

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
        public static readonly float ZhoResultFontSize = 26.0F;

        /// <summary>
        /// Font family for lemma (domain, equiv, note) in displayed entries. Also for Hanzi ranges.
        /// </summary>
        public static readonly string LemmaFontFamily = "Segoe UI";

        /// <summary>
        /// Font size for lemma (domain, equiv, note) in displayed entries. Hanzi size derived from this.
        /// </summary>
        public static readonly float LemmaFontSize = 12.0F;

        /// <summary>
        /// Hanzi in lemma is displayed in font this much bigger than <see cref="LemmaFontSize"/>.
        /// </summary>
        /// <remarks>
        /// <para>Depends on specific Latin and Hanzi font families used.</para>
        /// <para>Also double-check <see cref="OneResultControl.getTargetHanziOfs"/> when changing.</para>
        /// </remarks>
        public static readonly float LemmaHanziScale = 1.3F;

        /// <summary>
        /// Line height is font size (in pixels) times this.
        /// </summary>
        /// <remarks>Depends on specific font family used.</remarks>
        public static readonly float LemmaLineHeightScale = 1.2F;

        /// <summary>
        /// Font family for Pinyin in heading of displayed entries.
        /// </summary>
        public static readonly string PinyinFontFamily = "Tahoma";

        /// <summary>
        /// Font size for Pinyin in heading of displayed entries.
        /// </summary>
        public static readonly float PinyinFontSize = 12.0F;

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

        /// <summary>
        /// Alternating BG color of entries in results list.
        /// </summary>
        public static readonly Color ResultsAltBackColor = Color.FromArgb(248, 248, 255);

        /// <summary>
        /// Opacity of text shown as hint in search input field
        /// </summary>
        public static readonly int SearchInputHintOpacity = 80;

        /// <summary>
        /// Opacity of hint text in writing pad.
        /// </summary>
        public static readonly int WritingPadHintOpacity = 128;

        /// <summary>
        /// Font size of error report in character picker.
        /// </summary>
        public static readonly float CharPickerErrorFontSize = 10F;

        /// <summary>
        /// Opacity of results count overlay in bottom right corner.
        /// </summary>
        public static readonly int ResultCountOverlayOpacity = 196;

        /// <summary>
        /// Background color of results count overlay in bottom righ corner.
        /// </summary>
        public static readonly Color ResultCountOverlayBackColor = Color.FromArgb(0, 0, 0);

        /// <summary>
        /// Text color of results count overlay in bottom right corner.
        /// </summary>
        public static readonly Color ResultCountOverlayTextColor = Color.FromArgb(240, 240, 240);
    }
}
