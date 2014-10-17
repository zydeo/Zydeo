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
        /// Delegate for handling lookup requests through clicking on target link.
        /// </summary>
        /// <param name="queryString"></param>
        public delegate void LookupThroughLinkDelegate(string queryString);

        /// <summary>
        /// Source of localized texts.
        /// </summary>
        private readonly ITextProvider tprov;

        /// <summary>
        /// Called when user clicks a link (Chinese text) in an entry target;
        /// </summary>
        private LookupThroughLinkDelegate lookupThroughLink;

        /// <summary>
        /// Actual dictionary entry. Retrieved in ctor; nulled out once analyzed for rendering.
        /// </summary>
        private CedictEntry entry;
        /// <summary>
        /// The result this control displays.
        /// </summary>
        private readonly CedictResult res;
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
        /// Character height in entry body.
        /// </summary>
        private static float lemmaCharHeight = 0;
        /// <summary>
        /// Line height in the entry body: greater than character height for line spacing.
        /// </summary>
        private static float lemmaLineHeight = 0;
        
        /// <summary>
        /// The width I last analyzed my layout for.
        /// </summary>
        private int analyzedWidth = int.MinValue;
        /// <summary>
        /// Scripts to display (simp/trad/both).
        /// </summary>
        private readonly SearchScript analyzedScript;
        /// <summary>
        /// Text pool for optimal storage of all text chunks displayed typographically in this control.
        /// </summary>
        private TextPool textPool = null;
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
        private Block[] measuredBlocks = null;
        /// <summary>
        /// True if target contains Hanzi > need to re-measure when script changes.
        /// </summary>
        private bool anyTargetHanzi = false;
        /// <summary>
        /// Body text laid out for current width.
        /// </summary>
        private PositionedBlock[] positionedBlocks = null;
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
        /// Link areas in the entry body, or null.
        /// </summary>
        private List<LinkArea> targetLinks = null;

        /// <summary>
        /// Link area above which the cursor currently hovers, or null.
        /// </summary>
        private LinkArea hoverLink = null;

        /// <summary>
        /// Ctor: takes data to display.
        /// </summary>
        /// <param name="owner">Zen control that owns me.</param>
        /// <param name="tprov">Localized display text provider.</param>
        /// <param name="lookupThroughLink">Delegate to call when user initiates lookup by clicking on a link.</param>
        /// <param name="entryProvider">Dictionary entry provider.</param>
        /// <param name="cr">The lookup result this control will show.</param>
        /// <param name="maxHeadLength">Longest headword in full results list.</param>
        /// <param name="script">Scripts to show in headword.</param>
        /// <param name="odd">Odd/even position in list, for alternating BG color.</param>
        public OneResultControl(ZenControl owner, ITextProvider tprov,
            LookupThroughLinkDelegate lookupThroughLink,
            ICedictEntryProvider entryProvider, CedictResult cr,
            SearchScript script, bool odd)
            : base(owner)
        {
            this.tprov = tprov;
            this.lookupThroughLink = lookupThroughLink;
            this.entry = entryProvider.GetEntry(cr.EntryId);
            this.res = cr;
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
        private static Font[] fntArr = new Font[7];
        private const byte fntZhoHead = 0;
        private const byte fntPinyinHead = 1;
        private const byte fntSenseLatin = 2;
        private const byte fntSenseHanzi = 3;
        private const byte fntMetaLatin = 4;
        private const byte fntMetaHanzi = 5;
        private const byte fntSenseId = 6;

        private static string[] senseIdxStrings = new string[36];

        /// <summary>
        /// Static ctor: initializes static graphics resources.
        /// </summary>
        static OneResultControl()
        {
            fntArr[fntZhoHead] = FontPool.GetFont(ZenParams.ZhoContentFontFamily, ZenParams.ZhoResultFontSize, FontStyle.Regular);
            fntArr[fntPinyinHead] = new Font(ZenParams.PinyinFontFamily, ZenParams.PinyinFontSize, FontStyle.Bold);
            fntArr[fntSenseLatin] = new Font(ZenParams.LemmaFontFamily, ZenParams.LemmaFontSize);
            fntArr[fntSenseHanzi] = FontPool.GetFont(ZenParams.ZhoContentFontFamily, ZenParams.LemmaFontSize * 1.2F, FontStyle.Regular);
            fntArr[fntMetaLatin] = new Font(ZenParams.LemmaFontFamily, ZenParams.LemmaFontSize, FontStyle.Italic);
            fntArr[fntMetaHanzi] = FontPool.GetFont(ZenParams.ZhoContentFontFamily, ZenParams.LemmaFontSize * 1.2F, FontStyle.Italic);
            fntArr[fntSenseId] = new Font(ZenParams.LemmaFontFamily, ZenParams.LemmaFontSize * 0.8F);

            // Sense ID strings
            for (int i = 0; i != senseIdxStrings.Length; ++i)
            {
                string str = "?";
                if (i >= 0 && i < 9) str = (i + 1).ToString();
                else
                {
                    int resInt = (int)'a';
                    resInt += i - 9;
                    str = "";
                    str += (char)resInt;
                }
                senseIdxStrings[i] = str;
            }
        }

        /// <summary>
        /// Gets a static display font by its index.
        /// </summary>
        private static Font getFont(byte idx)
        {
            return fntArr[idx];
        }

        /// <summary>
        /// Gets the one-character display string representing a zero-based sense index.
        /// </summary>
        private static string getSenseIdString(int idx)
        {
            return senseIdxStrings[idx % senseIdxStrings.Length];
        }

        public override void DoMouseLeave()
        {
            if (Parent == null) return;
            Cursor = Cursors.Arrow;
            // Cursor hovered over a link: request a repaint
            if (hoverLink != null)
            {
                hoverLink = null;
                MakeMePaint(false, RenderMode.Invalidate);
            }
        }

        public override bool DoMouseMove(Point p, MouseButtons button)
        {
            // If we have no links, nothing to do
            if (targetLinks == null) return true;
            // Are we over a link area?
            LinkArea overWhat = null;
            foreach (LinkArea link in targetLinks)
            {
                foreach (Rectangle rect in link.ActiveAreas)
                {
                    if (rect.Contains(p))
                    {
                        overWhat = link;
                        break;
                    }
                }
            }
            // We're over a link
            if (overWhat != null) Cursor = Cursors.Hand;
            // Nop, not over a link
            else Cursor = Cursors.Arrow;
            // Hover state changed: request a repaint
            if (overWhat != hoverLink)
            {
                hoverLink = overWhat;
                MakeMePaint(false, RenderMode.Invalidate);
            }
            // We're done. No child controls, just return true.
            return true;
        }

        public override bool DoMouseClick(Point p, MouseButtons button)
        {
            // Are we over a link area?
            LinkArea overWhat = null;
            foreach (LinkArea link in targetLinks)
            {
                foreach (Rectangle rect in link.ActiveAreas)
                {
                    if (rect.Contains(p))
                    {
                        overWhat = link;
                        break;
                    }
                }
            }
            // Yes: trigger lookup
            if (overWhat != null)
                lookupThroughLink(overWhat.QueryString);
            return true;
        }
    }
}
