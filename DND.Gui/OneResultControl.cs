using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

using DND.Common;
using DND.Gui.Zen;

namespace DND.Gui
{
    /// <summary>
    /// Displays a single lookup result, with one dictionary entry, within the results list.
    /// </summary>
    internal partial class OneResultControl : ZenControl
    {
        /// <summary>
        /// Source of localized texts.
        /// </summary>
        private readonly ITextProvider tprov;

        /// <summary>
        /// The entry this control displays.
        /// </summary>
        public readonly CedictResult Res;
        /// <summary>
        /// The maximum hanzi length in the entire result set. Affects width of my headword area.
        /// </summary>
        private readonly int maxHeadLength;
        /// <summary>
        /// True if my position in the results list is odd; false for even. Drives alternating BG color.
        /// </summary>
        private readonly bool odd;

        // Paddings internal and external; calculated for current scale in ctor.
        private readonly int padLeft;
        private readonly int padTop;
        private readonly int padBottom;
        private readonly int padMid;
        private readonly int padRight;

        /// <summary>
        /// Used to measure size of ideographic characters.
        /// </summary>
        private const string ideoTestStr = "中";
        /// <summary>
        /// Used to measure width of space in entry body.
        /// </summary>
        private const string spaceTestStr = "f";
        /// <summary>
        /// Size of an ideographic character.
        /// </summary>
        private static SizeF ideoSize = new SizeF(0, 0);
        /// <summary>
        /// Line height of ideographic characters. Less than ideoSize's height due to weird rendering.
        /// </summary>
        private static float ideoLineHeight = 0;
        /// <summary>
        /// Measured width of a space.
        /// </summary>
        private static float spaceWidth = 0;
        /// <summary>
        /// Measured width of a pinyin space.
        /// </summary>
        private static float pinyinSpaceWidth = 0;
        /// <summary>
        /// Line height in the entry body.
        /// </summary>
        private static float lemmaLineHeight = 0;
        
        /// <summary>
        /// The width I last analyzed my layout for.
        /// </summary>
        private int analyzedWidth = int.MinValue;
        /// <summary>
        /// Scripts to display (simp/trad/both). If this changes, I must re-analyze: height may grow or shrink.
        /// </summary>
        private SearchScript analyzedScript;
        /// <summary>
        /// Typographically analyzed headword.
        /// </summary>
        private HeadInfo headInfo = null;
        /// <summary>
        /// Typographically analyzed pinyin.
        /// </summary>
        private PinyinInfo pinyinInfo = null;
        /// <summary>
        /// Typographically analyzed body text.
        /// </summary>
        private List<Block> measuredBlocks = null;
        /// <summary>
        /// True if target contains Hanzi > need to re-measure when script changes.
        /// </summary>
        private bool anyTargetHanzi = false;
        /// <summary>
        /// Body text laid out for current width.
        /// </summary>
        private List<PositionedBlock> positionedBlocks = null;
        /// <summary>
        /// <para>Indexes in <see cref="positionedBlocks"/> that have a match highlight.</para>
        /// <para>Each inner list contains adjacent blocks (range from a single sense).</para>
        /// </summary>
        private List<List<int>> targetHiliteIndexes = null;
        /// <summary>
        /// Vertical offset of Hanzi in a target line, to make it look better aligned in Latin text.
        /// </summary>
        private float targetHanziOfs = 0;

        /// <summary>
        /// Ctor: takes data to display.
        /// </summary>
        /// <param name="owner">Zen control that owns me.</param>
        /// <param name="cr">The lookup result this control will show.</param>
        /// <param name="maxHeadLength">Longest headword in full results list.</param>
        /// <param name="script">Scripts to show in headword.</param>
        /// <param name="odd">Odd/even position in list, for alternating BG color.</param>
        public OneResultControl(ZenControl owner, ITextProvider tprov, CedictResult cr, int maxHeadLength,
            SearchScript script, bool odd)
            : base(owner)
        {
            this.tprov = tprov;
            this.Res = cr;
            this.maxHeadLength = maxHeadLength;
            this.analyzedScript = script;
            this.odd = odd;

            padLeft = (int)(5.0F * Scale);
            padTop = (int)(5.0F * Scale);
            padBottom = (int)(8.0F * Scale);
            padMid = (int)(20.0F * Scale);
            padRight = (int)(10.0F * Scale);
        }

        // Graphics resources: static, singleton, never disposed.
        // When we're quitting it doesn't matter anymore, anyway.
        // TO-DO: double-check for thread safety when control starts drawing animations in worker thread!
        private static Font fntZhoHead;
        private static Font fntPinyinHead;
        private static Font fntSenseLatin;
        private static Font fntSenseHanzi;
        private static Font fntMetaLatin;
        private static Font fntMetaHanzi;
        private static Font fntSenseId;

        /// <summary>
        /// Static ctor: initializes static graphics resources.
        /// </summary>
        static OneResultControl()
        {
            fntZhoHead = new Font(ZenParams.ZhoFontFamily, ZenParams.ZhoFontSize);
            fntPinyinHead = new Font(ZenParams.PinyinFontFamily, ZenParams.PinyinFontSize, FontStyle.Bold);
            fntSenseLatin = new Font(ZenParams.LemmaFontFamily, ZenParams.LemmaFontSize);
            fntSenseHanzi = new Font(ZenParams.ZhoFontFamily, ZenParams.LemmaFontSize * 1.2F);
            fntMetaLatin = new Font(ZenParams.LemmaFontFamily, ZenParams.LemmaFontSize, FontStyle.Italic);
            fntMetaHanzi = new Font(ZenParams.ZhoFontFamily, ZenParams.LemmaFontSize * 1.2F, FontStyle.Italic);
            fntSenseId = new Font(ZenParams.LemmaFontFamily, ZenParams.LemmaFontSize * 0.8F);
        }
    }
}
