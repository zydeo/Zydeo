using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

using ZD.Common;
using ZD.Gui.Zen;

namespace ZD.Gui
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
        /// Delegate for handling a single result control's paint request.
        /// </summary>
        public delegate void ParentPaintDelegate();

        /// <summary>
        /// Delegate for handling a control's request for retrieving a dictionary entry.
        /// </summary>
        public delegate CedictEntry GetEntryDelegate(int entryId);

        /// <summary>
        /// Scale, passed to me in ctor, so I don't have to ask parent (which I may not yet have during analysis).
        /// </summary>
        private readonly float scale;

        /// <summary>
        /// Source of localized texts.
        /// </summary>
        private readonly ITextProvider tprov;

        /// <summary>
        /// Called when user clicks a link (Chinese text) in an entry target;
        /// </summary>
        private LookupThroughLinkDelegate lookupThroughLink;

        /// <summary>
        /// Called when this control requests a repaint.
        /// </summary>
        private ParentPaintDelegate parentPaint;

        /// <summary>
        /// Actual dictionary entry. Retrieved in ctor; nulled out once analyzed for rendering.
        /// </summary>
        private CedictEntry entry;

        /// <summary>
        /// The result this control displays.
        /// </summary>
        private readonly CedictResult res;

        /// <summary>
        /// True if this is the last control on screen.
        /// </summary>
        private readonly bool last;

        /// <summary>
        /// Called when this control needs to retrieve its dictionary entry later on in its life.
        /// </summary>
        private readonly GetEntryDelegate getEntry;

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
        /// Hot areas of senses, with current arrangement of blocks.
        /// </summary>
        private SenseArea[] senseAreas = null;

        /// <summary>
        /// <para>Index of sense currently hovered over, or -1.</para>
        /// <para>Means ense index in original Cedict entry, not our display circles - see "Classifier".</para>
        /// </summary>
        private short hoverSenseIx = -1;

        /// <summary>
        /// Ctor: takes data to display.
        /// </summary>
        /// <param name="owner">Zen control that owns me.</param>
        /// <param name="tprov">Localized display text provider.</param>
        /// <param name="lookupThroughLink">Delegate to call when user initiates lookup by clicking on a link.</param>
        /// <param name="getEntry">Delegate to call when an entry must be retrieved (for "copy" context menu).</param>
        /// <param name="entryProvider">Dictionary entry provider.</param>
        /// <param name="cr">The lookup result this control will show.</param>
        /// <param name="maxHeadLength">Longest headword in full results list.</param>
        /// <param name="script">Scripts to show in headword.</param>
        /// <param name="odd">Odd/even position in list, for alternating BG color.</param>
        public OneResultControl(ZenControlBase owner, float scale, ITextProvider tprov,
            LookupThroughLinkDelegate lookupThroughLink,
            ParentPaintDelegate parentPaint, GetEntryDelegate getEntry,
            ICedictEntryProvider entryProvider, CedictResult cr,
            SearchScript script, bool last)
            : base(owner)
        {
            this.scale = scale;
            this.tprov = tprov;
            this.lookupThroughLink = lookupThroughLink;
            this.parentPaint = parentPaint;
            this.getEntry = getEntry;
            this.entry = entryProvider.GetEntry(cr.EntryId);
            this.res = cr;
            this.analyzedScript = script;
            this.last = last;

            padLeft = (int)(5.0F * scale);
            padTop = (int)(4.0F * scale);
            padBottom = (int)(8.0F * scale);
            padMid = (int)(20.0F * scale);
            padRight = (int)(10.0F * scale);
        }

        // Graphics resources: static, singleton, never disposed.
        // When we're quitting it doesn't matter anymore, anyway.
        private static Font[] fntArr = new Font[10];
        private const byte fntZhoHeadSimp = 0;
        private const byte fntZhoHeadTrad = 1;
        private const byte fntPinyinHead = 2;
        private const byte fntSenseLatin = 3;
        private const byte fntSenseHanziSimp = 4;
        private const byte fntSenseHanziTrad = 5;
        private const byte fntMetaLatin = 6;
        private const byte fntMetaHanziSimp = 7;
        private const byte fntMetaHanziTrad = 8;
        private const byte fntSenseId = 9;

        /// <summary>
        /// Cached display strings for sense IDs, going from 0 to the unimaginable 35 (0-9, a-z)
        /// </summary>
        private static string[] senseIdxStrings = new string[36];

        /// <summary>
        /// Static ctor: initializes static graphics resources.
        /// </summary>
        static OneResultControl()
        {
            fntArr[fntPinyinHead] = FontCollection.CreateFont(Magic.PinyinFontFamily, Magic.PinyinFontSize, Magic.PinyinFontStyle);
            fntArr[fntSenseLatin] = new Font(Magic.LemmaFontFamily, Magic.LemmaFontSize);
            fntArr[fntMetaLatin] = new Font(Magic.LemmaFontFamily, Magic.LemmaFontSize, FontStyle.Italic);
            fntArr[fntSenseId] = new Font(Magic.LemmaFontFamily, Magic.LemmaFontSize * 0.8F);

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
            // Cursor hovered over a link, or a sense: request a repaint
            if (hoverLink != null || hoverSenseIx != -1)
            {
                hoverLink = null;
                hoverSenseIx = -1;
                // Cannot request paint for myself directly: entire results control must be repainted in one
                // - Cropping when I'm outside parent's rectangle
                // - Stuff in overlays on top of me
                parentPaint();
            }
        }

        /// <summary>
        /// Updates state for hover behavior above links.
        /// </summary>
        /// <param name="p">Mouse coordinate.</param>
        /// <returns>True if control must request a repaint.</returns>
        private bool doCheckLinkHover(Point p)
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
            // We're over a link
            //if (overWhat != null) Cursor = Cursors.Hand;
            if (overWhat != null) Cursor = CustomCursor.GetHand(Scale);
            // Nop, not over a link
            else Cursor = Cursors.Arrow;
            // Hover state changed: request a repaint
            if (overWhat != hoverLink)
            {
                hoverLink = overWhat;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Finds index of sense (in original Cedict entry) under provided coordinate.
        /// </summary>
        /// <param name="p">Coordinate to check.</param>
        /// <returns>Index of sense, or -1.</returns>
        private short getSenseIxFromPoint(Point p)
        {
            if (senseAreas == null || senseAreas.Length == 0) return -1;

            // Find which line point is over
            PointF pf = p;
            int lineIx = -1;
            int ix = 0;
            float lemmaTop = (float)padTop + pinyinInfo.PinyinHeight * 1.3F;
            if (p.Y < lemmaTop) return -1; // No such line
            for (float y = lemmaTop; y < Height; y += lemmaLineHeight, ++ix)
            {
                if (p.Y >= y && p.Y < y + lemmaLineHeight)
                {
                    lineIx = ix;
                    break;
                }
            }
            // No such line
            if (lineIx == -1) return -1;
            // Check every sense area
            foreach (SenseArea area in senseAreas)
            {
                if (area.LineIx == lineIx && p.X >= area.Left && p.X < area.Right)
                    return area.SenseIx;
            }
            // We did out best, but found no area.
            return -1;
        }

        public override bool DoMouseMove(Point p, MouseButtons button)
        {
            // If we have no links and no hot senses, nothing to do
            if (targetLinks == null && senseAreas == null) return true;

            bool needPaint = false;
            if (targetLinks != null) needPaint |= doCheckLinkHover(p);
            
            // Hover over sense?
            short senseIx = getSenseIxFromPoint(p);
            if (senseIx != hoverSenseIx) { hoverSenseIx = senseIx; needPaint = true; }

            if (needPaint)
            {
                // Cannot request paint for myself directly: entire results control must be repainted in one
                // - Cropping when I'm outside parent's rectangle
                // - Stuff in overlays on top of me
                parentPaint();
            }

            // We're done. No child controls, just return true.
            return true;
        }

        public override bool DoMouseClick(Point p, MouseButtons button)
        {
            // Right-click? Show context menu.
            if (button == MouseButtons.Right)
            {
                short senseIx = getSenseIxFromPoint(p);
                CedictEntry entry = getEntry(res.EntryId);
                ResultsCtxtControl ctxt = new ResultsCtxtControl(onCtxtMenuCommand, tprov, entry, senseIx, analyzedScript);
                ShowContextMenu(p, ctxt);
                return true;
            }

            // So, it's a left-click.
            // Are we over a link area?
            if (targetLinks == null) return true;
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

        /// <summary>
        /// Closes context menu if it's fired a command.
        /// </summary>
        private void onCtxtMenuCommand(ResultsCtxtControl sender)
        {
            CloseContextMenu(sender);
        }
    }
}
