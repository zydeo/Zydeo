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
        public OneResultControl(ZenControl owner, CedictResult cr, int maxHeadLength,
            SearchScript script, bool odd)
            : base(owner)
        {
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

        /// <summary>
        /// Breaks down body content into typographic blocks and caches the size of these.
        /// </summary>
        /// <param name="g">A Graphics object used for measurements.</param>
        private void doMeasureBlocks(Graphics g)
        {
            // Once measured, blocks don't change. Nothing to do then.
            if (measuredBlocks != null) return;

            // This is how we measure
            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // Decide about size of sense ID up front: that's always a square, letter-height
            SizeF xSize = g.MeasureString("x", fntSenseLatin, 65535, sf);
            SizeF senseIdxSize = new SizeF(xSize.Height, xSize.Height);

            // Create array with as many items as senses
            // Each item is null, or highlight in sense's equiv
            CedictTargetHighlight[] hlArr = new CedictTargetHighlight[Res.Entry.SenseCount];
            foreach (CedictTargetHighlight hl in Res.TargetHilites) hlArr[hl.SenseIx] = hl;

            // Recreate list of blocks
            measuredBlocks = new List<Block>();
            int senseIdx = -1;
            foreach (CedictSense cm in Res.Entry.Senses)
            {
                ++senseIdx;
                // Add one block for sense ID
                SenseIdBlock sidBlock = new SenseIdBlock
                {
                    Size = senseIdxSize,
                    StickRight = true,
                    Idx = senseIdx
                };
                measuredBlocks.Add(sidBlock);
                // Split domain, equiv and note into typographic parts
                // Splits along spaces and dashes
                // Unpacks Chinese ranges
                List<TextBlock> senseBlocks = new List<TextBlock>();
                getBlocks(cm.Domain, true, null, senseBlocks);
                getBlocks(cm.Equiv, false, hlArr[senseIdx], senseBlocks);
                getBlocks(cm.Note, true, null, senseBlocks);
                // Measure each block, and add to full list of blocks
                measuredBlocks.Capacity += senseBlocks.Count;
                foreach (TextBlock tb in senseBlocks)
                {
                    tb.Size = g.MeasureString(tb.Text, tb.Font, 65535, sf);
                    measuredBlocks.Add(tb);
                }
            }
        }

        /// <summary>
        /// <para>Produces unmeasured display blocks from a single hybrid text. Marks highlights, if any.</para>
        /// <para>Does not fill in blocks' size, but fills in everything else.</para>
        /// </summary>
        /// <param name="htxt">Hybrid text to break down into blocks and measure.</param>
        /// <param name="isMeta">True if this is a domain or note (displayed in italics).</param>
        /// <param name="hl">Highlight to show in hybrid text, or null.</param>
        /// <param name="blocks">List of blocks to append to.</param>
        private void getBlocks(HybridText htxt, bool isMeta, CedictTargetHighlight hl, List<TextBlock> blocks)
        {
            Font fntLatin = isMeta ? fntMetaLatin : fntSenseLatin;
            Font fntZho = isMeta ? fntMetaHanzi : fntSenseHanzi;
            // Go run by run
            for (int runIX = 0; runIX != htxt.RunCount; ++runIX)
            {
                TextRun run = htxt.GetRunAt(runIX);
                // Latin run: split by spaces first
                if (run is TextRunLatin)
                {
                    string[] bySpaces = run.GetPlainText().Split(new char[] { ' ' });
                    // Each word: also by dash
                    foreach (string str in bySpaces)
                    {
                        string[] byDashes = splitByDash(str);
                        // Add block for each
                        foreach (string blockStr in byDashes)
                        {
                            TextBlock tb = new TextBlock
                            {
                                StickRight = false,
                                Text = blockStr,
                                Font = fntLatin,
                                SpaceAfter = false, // will set this true for last block in "byDashes"
                            };
                            blocks.Add(tb);
                        }
                        // Make sure last one is followed by space
                        blocks[blocks.Count - 1].SpaceAfter = true;
                    }
                }
                // Chinese: depends on T/S/Both display mode, and on available info
                else
                {
                    TextRunZho zhoRun = run as TextRunZho;
                    // Chinese range is made up of:
                    // Simplified (empty string if only traditional requested)
                    // Separator (if both simplified and traditional are requested)
                    // Traditional (empty string if only simplified requested)
                    // Pinyin with accents as tone marks, in brackets (if present)
                    string strSimp = string.Empty;
                    if (analyzedScript != SearchScript.Traditional) strSimp = zhoRun.Simp;
                    string strTrad = string.Empty;
                    if (analyzedScript != SearchScript.Simplified) strTrad = zhoRun.Trad;
                    // Remember if we have any target Hanzi
                    if (strSimp != string.Empty || strTrad != string.Empty) anyTargetHanzi = true;
                    string strPy = string.Empty;
                    // Convert pinyin to display format (tone marks as diacritics; r5 glued)
                    if (zhoRun.Pinyin != null) strPy = "[" + zhoRun.GetPinyinInOne(true) + "]";
                    // Block for simplified, if present
                    if (strSimp != string.Empty)
                    {
                        TextBlock tb = new TextBlock
                        {
                            StickRight = false,
                            Text = strSimp,
                            Font = fntZho,
                            SpaceAfter = true,
                        };
                        blocks.Add(tb);
                    }
                    // Separator if both simplified and traditional are there
                    // AND they are different...
                    if (strSimp != string.Empty && strTrad != string.Empty && strSimp != strTrad)
                    {
                        blocks[blocks.Count - 1].StickRight = true;
                        TextBlock tb = new TextBlock
                        {
                            StickRight = false,
                            Text = "•",
                            Font = fntLatin,
                            SpaceAfter = true,
                        };
                        blocks.Add(tb);
                    }
                    // Traditional, if present
                    if (strTrad != string.Empty && strTrad != strSimp)
                    {
                        TextBlock tb = new TextBlock
                        {
                            StickRight = false,
                            Text = strTrad,
                            Font = fntZho,
                            SpaceAfter = true,
                        };
                        blocks.Add(tb);
                    }
                    // Pinyin, if present
                    if (strPy != string.Empty)
                    {
                        // Split by spaces
                        string[] pyParts = strPy.Split(new char[] { ' ' });
                        foreach (string pyPart in pyParts)
                        {
                            TextBlock tb = new TextBlock
                            {
                                StickRight = false,
                                Text = pyPart,
                                Font = fntLatin,
                                SpaceAfter = true,
                            };
                            blocks.Add(tb);
                        }
                    }
                    // Last part will have requested a space after.
                    // Look ahead and if next text run is Latin and starts with punctuation, make it stick
                    TextRunLatin nextLatinRun = null;
                    if (runIX + 1 < htxt.RunCount) nextLatinRun = htxt.GetRunAt(runIX + 1) as TextRunLatin;
                    if (nextLatinRun != null && char.IsPunctuation(nextLatinRun.GetPlainText()[0]))
                        blocks[blocks.Count - 1].SpaceAfter = false;
                }
            }
        }

        /// <summary>
        /// Splits text along dashes (words end in dashes).
        /// </summary>
        private string[] splitByDash(string str)
        {
            // No dash at all inside word: return in one
            bool dashInside = false;
            for (int i = 1; i < str.Length - 2; ++i)
            { if (str[i] == '-') { dashInside = true; break; } }
            if (!dashInside)
            {
                string[] res = new string[1];
                res[0] = str;
                return res;
            }
            List<string> wdList = new List<string>();
            StringBuilder sbWord = new StringBuilder();
            sbWord.Append(str[0]);
            for (int i = 1; i != str.Length; ++i)
            {
                char c = str[i];
                sbWord.Append(c);
                if (c != '-') continue;
                wdList.Add(sbWord.ToString());
                sbWord.Clear();
            }
            if (sbWord.Length != 0) wdList.Add(sbWord.ToString());
            return wdList.ToArray();
        }

        /// <summary>
        /// Calculates layout of content in entry body, taking current width into account for line breaks.
        /// </summary>
        /// <param name="lemmaL">Left position of body content area.</param>
        /// <param name="lemmaW">Width of body content area.</param>
        /// <returns>Bottom of content area.</returns>
        private float doArrangeBlocks(float lemmaL, float lemmaW)
        {
            float lemmaTop = (float)padTop + pinyinInfo.PinyinHeight * 1.3F;

            // Will not work reduntantly
            if (positionedBlocks != null)
            {
                if (positionedBlocks.Count == 0) return lemmaTop;
                else return positionedBlocks[positionedBlocks.Count - 1].Loc.Y + lemmaLineHeight;
            }

            // This is always re-done when function is called
            // We only get here when width has changed, so we do need to rearrange
            positionedBlocks = new List<PositionedBlock>();
            float blockX = lemmaL;
            float blockY = lemmaTop;
            PositionedBlock lastPB = null;
            foreach (Block block in measuredBlocks)
            {
                // If current block is a sense ID, and we've had block before:
                // Add extra space on left
                if (block is SenseIdBlock && lastPB != null)
                    blockX += spaceWidth;

                // Use current position
                PositionedBlock pb = new PositionedBlock
                {
                    Block = block,
                    Loc = new PointF(blockX, blockY),
                };
                // New block extends beyond available width: break to next line
                // But, if last block is "stick right", break together
                if (pb.Loc.X + block.Size.Width - lemmaL > lemmaW)
                {
                    blockY += lemmaLineHeight;
                    blockX = lemmaL;
                    // No stick
                    if (lastPB == null || !lastPB.Block.StickRight) pb.Loc = new PointF(blockX, blockY);
                    // We break together
                    else
                    {
                        // Last block breaks onto this line
                        lastPB.Loc = new PointF(blockX, blockY);
                        // We move on by last block's width plus (optional) space
                        blockX += lastPB.Block.Size.Width;
                        if (lastPB.Block is TextBlock && (lastPB.Block as TextBlock).SpaceAfter)
                            blockX += spaceWidth;
                        // So.
                        pb.Loc = new PointF(blockX, blockY);
                    }
                }
                positionedBlocks.Add(pb);
                // Move right by block's width; space optional
                blockX += block.Size.Width;
                if (block is TextBlock && (block as TextBlock).SpaceAfter)
                    blockX += spaceWidth;
                // This is last block
                lastPB = pb;
            }
            return measuredBlocks.Count == 0 ? blockY : blockY + lemmaLineHeight;
        }

        /// <summary>
        /// Right-align a range of headword blocks.
        /// </summary>
        /// <param name="blocks">The full headword blocks list.</param>
        /// <param name="start">Index of the first character to right-align.</param>
        /// <param name="length">Lengt of range to right-align.</param>
        /// <param name="right">The right edge.</param>
        private static void doRightAlign(List<HeadBlock> blocks,
            int start, int length, float right)
        {
            float x = right;
            for (int i = start + length - 1; i >= start; --i)
            {
                HeadBlock block = blocks[i];
                block.Loc = new PointF(x - block.Size.Width, block.Loc.Y);
                x -= block.Size.Width;
            }
        }

        /// <summary>
        /// Calculates right-aligned layout in headword area.
        /// </summary>
        private static bool doAnalyzeHanzi(Graphics g, string str, StringFormat sf,
            List<HeadBlock> blocks, ref PointF loc, float right)
        {
            float left = loc.X;
            bool lineBreak = false;
            int firstCharOfLine = 0;
            int charsOnLine = 0;
            // Measure and position each character
            for (int i = 0; i != str.Length; ++i)
            {
                ++charsOnLine;
                string cstr = str[i].ToString();
                // Measure each character. They may not all be hanzi: there are latin letters in some HWS
                HeadBlock hb = new HeadBlock
                {
                    Char = cstr,
                    Size = g.MeasureString(cstr, fntZhoHead, 65535, sf),
                    Loc = loc,
                    Faded = false,
                };
                blocks.Add(hb);
                // Location moves right
                loc.X += hb.Size.Width;
                // If location is beyond headword's right edge, break line now.
                // This involves
                // - moving last added block to next line
                // - right-aligning blocks added so far
                if (loc.X > right)
                {
                    lineBreak = true;
                    loc.X = left;
                    loc.Y += ideoLineHeight;
                    doRightAlign(blocks, firstCharOfLine, charsOnLine - 1, right);
                    charsOnLine = 1;
                    firstCharOfLine = blocks.Count - 1;
                    hb.Loc = loc;
                    loc.X += hb.Size.Width;
                }
            }
            // Right-align the final bit
            doRightAlign(blocks, firstCharOfLine, charsOnLine, right);
            // Done - tell call if we had line break or not
            return lineBreak;
        }

        /// <summary>
        /// Analyzes layout of headword.
        /// </summary>
        /// <param name="g">A Graphics object used for measurements.</param>
        private void doAnalyzeHeadword(Graphics g)
        {
            // If already analyzed, nothing to do
            if (headInfo != null) return;

            // This is how we measure
            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // On-demand: measure a single ideograph's dimensions
            if (ideoSize.Width == 0)
            {
                ideoSize = g.MeasureString(ideoTestStr, fntZhoHead, 65535, sf);
                var si = HanziMeasure.Instance.GetMeasures(ZenParams.ZhoFontFamily, ZenParams.ZhoFontSize);
                float hanziLinePad = 6.0F;
                hanziLinePad *= Scale;
                ideoLineHeight = si.RealRect.Height + hanziLinePad;
            }

            headInfo = new HeadInfo();
            if (analyzedScript == SearchScript.Simplified) headInfo.HeadMode = HeadMode.OnlySimp;
            else if (analyzedScript == SearchScript.Traditional) headInfo.HeadMode = HeadMode.OnlyTrad;
            else headInfo.HeadMode = HeadMode.BothSingleLine;
            //// For width of headword, use padLeft from border, plus 4 to 6 ideographs' worth of space
            //// Depending on longest headword in entire list
            //int hwChars = maxHeadLength;
            //if (hwChars < 4) hwChars = 4;
            //if (hwChars > 6) hwChars = 6;
            //float hwidth = ((float)hwChars) * ideoSize.Width;
            // Revised
            // For width of headword, always take 5 characters' width of space
            float hwidth = 5.0F * ideoSize.Width;
            headInfo.HeadwordRight = padLeft + hwidth;
            // Measure simplified chars from start; break when needed
            PointF loc = new PointF(((float)padLeft) + ideoSize.Width, padTop / 2.0F); // Less padding above hanzi - font leaves enough space
            //PointF loc = new PointF(padLeft, padTop / 2.0F); // Less padding above hanzi - font leaves enough space
            bool lbrk = false;
            if (analyzedScript == SearchScript.Simplified || analyzedScript == SearchScript.Both)
            {
                lbrk |= doAnalyzeHanzi(g, Res.Entry.ChSimpl, sf, headInfo.SimpBlocks, ref loc, headInfo.HeadwordRight);
            }
            if (analyzedScript == SearchScript.Traditional || analyzedScript == SearchScript.Both)
            {
                //loc.X = padLeft;
                loc.X = ((float)padLeft) + ideoSize.Width;
                if (analyzedScript == SearchScript.Both) loc.Y += ideoLineHeight;
                lbrk |= doAnalyzeHanzi(g, Res.Entry.ChTrad, sf, headInfo.TradBlocks, ref loc, headInfo.HeadwordRight);
            }
            // If we're displaying both simplified and traditional, fade out
            // traditional chars that are same as simplified, right above them
            if (analyzedScript == SearchScript.Both)
            {
                for (int i = 0; i != headInfo.SimpBlocks.Count; ++i)
                {
                    if (headInfo.SimpBlocks[i].Char == headInfo.TradBlocks[i].Char)
                        headInfo.TradBlocks[i].Faded = true;
                }
            }
            // Bottom of headword area
            headInfo.HeadwordBottom = loc.Y + ideoLineHeight;
            // If we had a line break and we're showing both scripts, update info
            if (analyzedScript == SearchScript.Both && lbrk)
                headInfo.HeadMode = HeadMode.BothMultiLine;
        }

        /// <summary>
        /// Calculates pinyin layout.
        /// </summary>
        private void doAnalyzePinyin(Graphics g)
        {
            // If already measured, nothing to do
            if (pinyinInfo != null) return;

            // This is how we measure
            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            pinyinInfo = new PinyinInfo();
            // Measure each pinyin syllable
            bool diacritics = true;
            var pcoll = Res.Entry.GetPinyinForDisplay(diacritics,
                Res.PinyinHiliteStart, Res.PinyinHiliteLength,
                out pinyinInfo.HiliteStart, out pinyinInfo.HiliteLength);
            float cx = (float)AbsLeft + headInfo.HeadwordRight + (float)padMid;
            float ctop = padTop;
            for (int i = 0; i != pcoll.Count; ++i)
            {
                PinyinSyllable ps = pcoll[i];
                // New pinyin block
                PinyinBlock pb = new PinyinBlock();
                // Text: syllable's display text
                pb.Text = ps.GetDisplayString(true);
                // Block's size and relative location
                SizeF sz = g.MeasureString(pb.Text, fntPinyinHead, 65535, sf);
                pb.Rect = new RectangleF(cx, ctop, sz.Width, sz.Height);
                cx += sz.Width + pinyinSpaceWidth;
                // Add block
                pinyinInfo.Blocks.Add(pb);
            }
            // Height of whole pinyin area
            pinyinInfo.PinyinHeight = pinyinInfo.Blocks[0].Rect.Height;
        }

        /// <summary>
        /// Analyzes UI for display. Assumes ideal height for provided width wihtout invalidating or painting.
        /// </summary>
        /// <param name="g">A Graphics object used for measurements.</param>
        /// <param name="width">Control's width.</param>
        public void Analyze(Graphics g, int width, SearchScript script)
        {
            // If width or script has not changed, nothing to do.
            if (analyzedWidth == width && script == analyzedScript) return;
            if (analyzedWidth != width)
            {
                analyzedWidth = Width;
                positionedBlocks = null;
            }
            if (analyzedScript != script)
            {
                analyzedScript = script;
                headInfo = null;
                // Make me re-render completely if target contains Hanzi and script has changed.
                if (anyTargetHanzi)
                {
                    measuredBlocks = null;
                    positionedBlocks = null;
                }
            }

            // This is how we measure
            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // On-demand: measure a space's width - for entry text flow
            // Also line height in entry text
            if (spaceWidth == 0)
            {
                SizeF sz = g.MeasureString(spaceTestStr, fntSenseLatin, 65535, sf);
                spaceWidth = (int)sz.Width;
                lemmaLineHeight = sz.Height * 1.1F;
                sz = g.MeasureString(spaceTestStr, fntPinyinHead, 65535, sf);
                pinyinSpaceWidth = sz.Width;
            }

            // Headword and pinyin
            // Will not measure redundantly
            doAnalyzeHeadword(g);
            doAnalyzePinyin(g);

            // OK, now onto body
            // Measure blocks in themselves on demand
            // Will not measure redundantly
            doMeasureBlocks(g);

            // Arrange blocks
            float lemmaW = ((float)width) - headInfo.HeadwordRight - padMid - padRight;
            float lemmaL = headInfo.HeadwordRight + padMid;
            float lastTop = doArrangeBlocks(lemmaL, lemmaW);

            // My height: bottom of headword or bottom of entry, whichever is lower
            float entryHeight = lastTop + padBottom;
            float zhoHeight = headInfo.HeadwordBottom + padBottom;
            float trueHeight = Math.Max(entryHeight, zhoHeight);

            // Assume this height, and also provided width
            Size = new Size(width, (int)trueHeight);
        }
    }
}
