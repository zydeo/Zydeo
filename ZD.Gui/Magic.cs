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
    public class Magic
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
        public readonly static string DictFileName = @"Data\cedict-zydeo.bin";

        /// <summary>
        /// Name of compiled, binary stroke recognition data.
        /// </summary>
        public readonly static string StrokesFileName = @"Data\strokes-zydeo.bin";

        /// <summary>
        /// Name of the license file, deployed in binary folder, that opens when user clicks on link Zydeo tab.
        /// </summary>
        public readonly static string LicenseFileName = "License.html";

        /// <summary>
        /// Subfolder in user's appdata where Zydeo stores its persistent user-specific data.
        /// </summary>
        /// <remarks>Keep in sync with <see cref="ZD.AU.Magic.ZydeoUserFolder"/>.</remarks>
        public readonly static string ZydeoUserFolder = "Zydeo";

        /// <summary>
        /// Name of file where Zydeo's settings are stored in its user data folder.
        /// </summary>
        public readonly static string ZydeoSettingsFile = "Settings.xml";

        /// <summary>
        /// Name of file where Zydeo logs gracefully handled as well as fateful errors.
        /// </summary>
        public readonly static string ZydeoErrorFile = "Errors.log";

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
        public readonly static string SearchSimp = "简";

        /// <summary>
        /// Non-localized text of script selector button: traditional.
        /// </summary>
        public readonly static string SearchTrad = "繁";

        /// <summary>
        /// Non-localized text of script selector button: S+T.
        /// </summary>
        public readonly static string SearchBoth = "简+繁";

        /// <summary>
        /// Non-localized text of search language button: English.
        /// </summary>
        public readonly static string SearchLangEng = "英";

        /// <summary>
        /// Non-localized text of search language button: Chinese.
        /// </summary>
        public readonly static string SearchLangZho = "中";

        /// <summary>
        /// Font size for hard-wired texts on buttons next to search input field.
        /// </summary>
        public static readonly float ZhoButtonFontSize = 18.0F;

        /// <summary>
        /// See <see cref="ZhoContentFontFamily"/>.
        /// </summary>
        private static IdeoFamily zhoContentFontFamily = IdeoFamily.WinKai;

        /// <summary>
        /// Font family for Hanzi in lookup results and char picker.
        /// </summary>
        public static IdeoFamily ZhoContentFontFamily
        {
            get { return zhoContentFontFamily; }
        }

        /// <summary>
        /// Sets the font family for Hanzi characters in lookup results and char picker.
        /// </summary>
        public static void SetZhoContentFontFamily(IdeoFamily fam)
        {
            zhoContentFontFamily = fam;
        }

        /// <summary>
        /// Font size for Hanzi heading in each displayed dictionary entry. Also in character picker.
        /// </summary>
        public static readonly float ZhoResultFontSize = 26.0F;

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
        public static readonly float LemmaHanziFontSize = 15.6F;

        /// <summary>
        /// Line height is font size (in pixels) times this.
        /// </summary>
        /// <remarks>Depends on specific font family used.</remarks>
        public static readonly float LemmaLineHeightScale = 1.2F;

        /// <summary>
        /// Font family for Pinyin in heading of displayed entries.
        /// </summary>
        public static readonly string PinyinFontFamily = "Ubuntu";

        /// <summary>
        /// Font style for Pinyin in heading of displayed entries.
        /// </summary>
        public static readonly FontStyle PinyinFontStyle = FontStyle.Bold;

        /// <summary>
        /// Font size for Pinyin in heading of displayed entries.
        /// </summary>
        public static readonly float PinyinFontSize = 12.0F;

        /// <summary>
        /// Text color of Pinyin in heading of entries.
        /// </summary>
        public static readonly Color PinyinColor = Color.FromArgb(32, 32, 32);

        /// <summary>
        /// Text measuredto get width of space. Varies with fonts.
        /// </summary>
        public static readonly string PinyinSpaceTestString = "i";

        /// <summary>
        /// Base color of highlights in a result (auto-faded gradient at ends).
        /// </summary>
        public static readonly Color HiliteColor = Color.FromArgb(0xff, 0xe4, 0xcc);
        //public static readonly Color HiliteColor = Color.FromArgb(255, 232, 189);

        /// <summary>
        /// Hover color of text in results that acts as a lookup link.
        /// </summary>
        public static readonly Color LinkHoverColor = Color.FromArgb(0x74, 0x35, 0x00);

        /// <summary>
        /// Background color if "sense ID" circle's inside when sense is hovered over.
        /// </summary>
        public static readonly Color SenseHoverColor = Color.FromArgb(0xe0, 0xdb, 0xd7);

        /// <summary>
        /// Font size of results count box in bottom right.
        /// </summary>
        public static readonly float ResultsCountFontSize = 9.0F;

        /// <summary>
        /// Color of separator line at the bottom of a single result control.
        /// </summary>
        public static readonly Color ResultsSeparator = Color.DarkGray;

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
        public static readonly int ResultCountOverlayOpacity = 224;

        /// <summary>
        /// Background color of results count overlay in bottom righ corner.
        /// </summary>
        public static readonly Color ResultCountOverlayBackColor = Color.FromArgb(76, 76, 76);

        /// <summary>
        /// Text color of results count overlay in bottom right corner.
        /// </summary>
        public static readonly Color ResultCountOverlayTextColor = Color.FromArgb(240, 240, 240);

        /// <summary>
        /// Maximum length of sense string in context menu before it is ellipsed.
        /// </summary>
        public static readonly int CtxtMenuMaxSenseLength = 64;

        /// <summary>
        /// Maximum number of syllables in context menu (Hanzi and Pinyin) before strings are ellipsed.
        /// </summary>
        public static readonly int CtxtMenuMaxSyllableLength = 16;

        /// <summary>
        /// Zydeo's Github URL shown on the Zydeo tab page.
        /// </summary>
        public static readonly string GithubUrl = "github.com/Zydeo";

        /// <summary>
        /// Zydeo's website URL shown on the Zydeo tab page.
        /// </summary>
        public static readonly string WebUrl = "zydeo.net";

        public static readonly string WhiteUpdFntTitle = "Neuton";
        public static readonly float WhiteUpFntTitleSz = 24F;
        public static readonly float WhiteUpdFntNormSz = 10F;
        public static readonly Color WhiteUpdClrTitle = Color.FromArgb(0xa8, 0x80, 0x5f);
        public static readonly Color WhiteUpdClrBody = Color.FromArgb(0x26, 0x26, 0x26);
        public static readonly Color WhiteUpdClrLink = Color.FromArgb(0, 0, 192);
        public static readonly Color WhiteUpdClrDetailsSep = Color.FromArgb(0xa9, 0xa9, 0xa9);
    }
}
