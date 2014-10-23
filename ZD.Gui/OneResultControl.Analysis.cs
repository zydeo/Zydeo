using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

using ZD.Common;
using ZD.Gui.Zen;

namespace ZD.Gui
{
    partial class OneResultControl
    {
        /// <summary>
        /// Analyzes UI for display. Assumes ideal height for provided width wihtout invalidating or painting.
        /// </summary>
        /// <param name="g">A Graphics object used for measurements.</param>
        /// <param name="width">Control's width.</param>
        public void Analyze(Graphics g, int width)
        {
            bool isTextPoolNew = textPool == null;

            // If width or script has not changed, nothing to do.
            if (analyzedWidth == width) return;
            analyzedWidth = Width;
            positionedBlocks = null;
            targetHiliteIndexes = null;
            
            // This is how we measure
            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // On-demand: measure a space's width - for entry text flow
            // Also line height in entry text
            if (spaceWidth == 0)
            {
                SizeF sz = g.MeasureString(spaceTestStr, getFont(fntSenseLatin), 65535, sf);
                spaceWidth = (int)sz.Width;
                lemmaCharHeight = sz.Height;
                lemmaLineHeight = sz.Height * Magic.LemmaLineHeightScale;
                sz = g.MeasureString(spaceTestStr, getFont(fntPinyinHead), 65535, sf);
                pinyinSpaceWidth = sz.Width;
            }

            // Create text pool if needed
            if (textPool == null) textPool = new TextPool();

            // Headword and pinyin
            // Will not measure redundantly
            doAnalyzeHeadword(g);
            doAnalyzePinyin(g);

            // OK, now onto body
            // Measure blocks in themselves on demand
            // Will not measure redundantly
            doMeasureBlocks(g);

            // Finalize text pool - compact in memory
            if (isTextPoolNew) textPool.FinishBuilding();
            // Get rid of entry. Not keeping it in memory once we're done analyzing.
            entry = null;

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
            SizeF xSize = g.MeasureString("x", getFont(fntSenseLatin), 65535, sf);
            ushort senseIdxWidth = (ushort)Math.Ceiling(xSize.Height);

            // Create array with as many items as senses
            // Each item is null, or highlight in sense's equiv
            CedictTargetHighlight[] hlArr = new CedictTargetHighlight[entry.SenseCount];
            foreach (CedictTargetHighlight hl in res.TargetHilites) hlArr[hl.SenseIx] = hl;

            // Recreate list of blocks
            List<Block> newBlocks = new List<Block>();
            // Collect links here. Will only keep at end if not empty.
            List<LinkArea> newLinks = new List<LinkArea>();

            int senseIdx = -1;
            int displaySenseIdx = -1;
            bool lastWasClassifier = false;
            foreach (CedictSense cm in entry.Senses)
            {
                ++senseIdx;
                // Is this sense a classifier?
                bool classifier = cm.Domain.EqualsPlainText("CL:");
                if (!classifier) ++displaySenseIdx;
                // Add one block for sense ID, unless this is a classifier "sense"
                if (!classifier)
                {
                    Block sidBlock = new Block
                    {
                        Width = senseIdxWidth,
                        StickRight = true,
                        TextPos = textPool.PoolString(getSenseIdString(displaySenseIdx)),
                        NewLine = lastWasClassifier,
                        SenseId = true
                    };
                    newBlocks.Add(sidBlock);
                }
                // Split domain, equiv and note into typographic parts
                // Splits along spaces and dashes
                // Unpacks Chinese ranges
                // Domain is localized text for "Classifier:" if, well, this is a classifier sense
                int startIX = newBlocks.Count;
                if (!classifier) makeBlocks(cm.Domain, true, null, newBlocks, newLinks);
                else
                {
                    string strClassifier = tprov.GetString("ResultCtrlClassifier");
                    HybridText htClassifier = new HybridText(strClassifier);
                    int ix = newBlocks.Count;
                    makeBlocks(htClassifier, true, null, newBlocks, newLinks);
                    Block xb = newBlocks[ix];
                    xb.NewLine = true;
                    newBlocks[ix] = xb;
                }
                makeBlocks(cm.Equiv, false, hlArr[senseIdx], newBlocks, newLinks);
                makeBlocks(cm.Note, true, null, newBlocks, newLinks);
                // Measure each block
                for (int i = startIX; i != newBlocks.Count; ++i)
                {
                    Block tb = newBlocks[i];
                    SizeF sz = g.MeasureString(textPool.GetString(tb.TextPos), getFont(tb.FontIdx), 65535, sf);
                    tb.Width = (ushort)Math.Round(sz.Width);
                    newBlocks[i] = tb;
                }
                lastWasClassifier = classifier;
            }
            if (newLinks.Count != 0) targetLinks = newLinks;
            measuredBlocks = newBlocks.ToArray();
        }

        /// <summary>
        /// <para>Produces unmeasured display blocks from a single hybrid text. Marks highlights, if any.</para>
        /// <para>Does not fill in blocks' size, but fills in everything else.</para>
        /// </summary>
        /// <param name="htxt">Hybrid text to break down into blocks and measure.</param>
        /// <param name="isMeta">True if this is a domain or note (displayed in italics).</param>
        /// <param name="hl">Highlight to show in hybrid text, or null.</param>
        /// <param name="blocks">List of blocks to append to.</param>
        /// <param name="links">List to gather links (appending to list).</param>
        private void makeBlocks(HybridText htxt, bool isMeta, CedictTargetHighlight hl,
            List<Block> blocks, List<LinkArea> links)
        {
            byte fntIdxLatin = isMeta ? fntMetaLatin : fntSenseLatin;
            byte fntIdxZhoSimp = isMeta ? fntMetaHanziSimp : fntSenseHanziSimp;
            byte fntIdxZhoTrad = isMeta ? fntMetaHanziTrad : fntSenseHanziTrad;
            // Go run by run
            for (int runIX = 0; runIX != htxt.RunCount; ++runIX)
            {
                TextRun run = htxt.GetRunAt(runIX);
                // Latin run: split by spaces first
                if (run is TextRunLatin)
                {
                    string[] bySpaces = run.GetPlainText().Split(new char[] { ' ' });
                    // Each word: also by dash
                    int latnPos = 0;
                    foreach (string str in bySpaces)
                    {
                        string[] byDashes = splitByDash(str);
                        // Add block for each
                        int ofsPos = 0;
                        foreach (string blockStr in byDashes)
                        {
                            Block tb = new Block
                            {
                                TextPos = textPool.PoolString(blockStr),
                                FontIdx = fntIdxLatin,
                                SpaceAfter = false, // will set this true for last block in "byDashes"
                            };
                            // Does block's text intersect with highlight?
                            if (hl != null && hl.RunIx == runIX)
                            {
                                int blockStart = latnPos + ofsPos;
                                int blockEnd = blockStart + blockStr.Length;
                                if (blockStart >= hl.HiliteStart && blockStart < hl.HiliteStart + hl.HiliteLength)
                                    tb.Hilite = true;
                                else if (blockEnd > hl.HiliteStart && blockEnd <= hl.HiliteStart + hl.HiliteLength)
                                    tb.Hilite = true;
                                else if (blockStart < hl.HiliteStart && blockEnd >= hl.HiliteStart + hl.HiliteLength)
                                    tb.Hilite = true;
                            }
                            blocks.Add(tb);
                            // Keep track of position for highlight
                            ofsPos += blockStr.Length;
                        }
                        // Make sure last one is followed by space
                        Block xb = blocks[blocks.Count - 1];
                        xb.SpaceAfter = true;
                        blocks[blocks.Count - 1] = xb;
                        // Keep track of position in text - for highlights
                        latnPos += str.Length + 1;
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
                    if (analyzedScript != SearchScript.Traditional && zhoRun.Simp != null) strSimp = zhoRun.Simp;
                    string strTrad = string.Empty;
                    if (analyzedScript != SearchScript.Simplified && zhoRun.Trad != null) strTrad = zhoRun.Trad;
                    string strPy = string.Empty;
                    // Convert pinyin to display format (tone marks as diacritics; r5 glued)
                    if (zhoRun.Pinyin != null) strPy = "[" + zhoRun.GetPinyinInOne(true) + "]";

                    // Create link area, with query string
                    string strPyNumbers = string.Empty; // Pinyin with numbers as tone marks
                    if (zhoRun.Pinyin != null) strPyNumbers = zhoRun.GetPinyinRaw();
                    LinkArea linkArea = new LinkArea(strSimp, strTrad, strPyNumbers, analyzedScript);

                    // Block for simplified, if present
                    if (strSimp != string.Empty)
                    {
                        Block tb = new Block
                        {
                            TextPos = textPool.PoolString(strSimp),
                            FontIdx = fntIdxZhoSimp,
                            SpaceAfter = true,
                        };
                        blocks.Add(tb);
                        linkArea.BlockIds.Add(blocks.Count - 1);
                    }
                    // Separator if both simplified and traditional are there
                    // AND they are different...
                    if (strSimp != string.Empty && strTrad != string.Empty && strSimp != strTrad)
                    {
                        Block xb = blocks[blocks.Count - 1];
                        xb.StickRight = true;
                        blocks[blocks.Count - 1] = xb;
                        Block tb = new Block
                        {
                            TextPos = textPool.PoolString("•"),
                            FontIdx = fntIdxLatin,
                            SpaceAfter = true,
                        };
                        blocks.Add(tb);
                        linkArea.BlockIds.Add(blocks.Count - 1);
                    }
                    // Traditional, if present
                    if (strTrad != string.Empty && strTrad != strSimp)
                    {
                        Block tb = new Block
                        {
                            TextPos = textPool.PoolString(strTrad),
                            FontIdx = fntIdxZhoTrad,
                            SpaceAfter = true,
                        };
                        blocks.Add(tb);
                        linkArea.BlockIds.Add(blocks.Count - 1);
                    }
                    // Pinyin, if present
                    if (strPy != string.Empty)
                    {
                        // Split by spaces
                        string[] pyParts = strPy.Split(new char[] { ' ' });
                        foreach (string pyPart in pyParts)
                        {
                            Block tb = new Block
                            {
                                TextPos = textPool.PoolString(pyPart),
                                FontIdx = fntIdxLatin,
                                SpaceAfter = true,
                            };
                            blocks.Add(tb);
                            linkArea.BlockIds.Add(blocks.Count - 1);
                        }
                    }
                    // Last part will have requested a space after.
                    // Look ahead and if next text run is Latin and starts with punctuation, make it stick
                    TextRunLatin nextLatinRun = null;
                    if (runIX + 1 < htxt.RunCount) nextLatinRun = htxt.GetRunAt(runIX + 1) as TextRunLatin;
                    if (nextLatinRun != null && char.IsPunctuation(nextLatinRun.GetPlainText()[0]))
                    {
                        Block xb = blocks[blocks.Count - 1];
                        xb.SpaceAfter = false;
                        blocks[blocks.Count - 1] = xb;
                    }
                    // Collect link area
                    links.Add(linkArea);
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
        /// <para>Adds current list of highlight indexes (into <see cref="positionedBlocks"/> list)</para>
        /// <para>to <see cref="targetHiliteIndexes"/>, unless current list is empty.</para>
        /// </summary>
        private void doCollectHighlightRange(ref List<int> currIndexes)
        {
            if (currIndexes.Count == 0) return;
            if (targetHiliteIndexes == null) targetHiliteIndexes = new List<List<int>>();
            targetHiliteIndexes.Add(currIndexes);
            currIndexes = new List<int>();
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
                if (positionedBlocks.Length == 0) return lemmaTop;
                else return positionedBlocks[positionedBlocks.Length - 1].LocY + lemmaLineHeight;
            }

            // This is always re-done when function is called
            // We only get here when width has changed, so we do need to rearrange
            positionedBlocks = new PositionedBlock[measuredBlocks.Length];
            List<int> currHiliteIndexes = new List<int>();
            float blockX = lemmaL;
            float blockY = lemmaTop;
            bool firstBlock = true;
            PositionedBlock lastPB = new PositionedBlock();
            Block lastBlock = new Block();
            for (int i = 0; i != measuredBlocks.Length; ++i)
            {
                Block block = measuredBlocks[i];
                // If current block is a sense ID, and we've had block before:
                // Add extra space on left
                if (block.SenseId && !firstBlock)
                    blockX += spaceWidth;

                // Use current position
                PositionedBlock pb = new PositionedBlock
                {
                    BlockIdx = (ushort)i,
                    LocX = (short)Math.Round(blockX),
                    LocY = (short)Math.Round(blockY),
                };
                // New block extends beyond available width: break to next line
                // Also break if block explicitly requests it
                // But, if last block is "stick right", break together
                if (pb.LocX + ((float)block.Width) - lemmaL > lemmaW || block.NewLine)
                {
                    blockY += lemmaLineHeight;
                    blockX = lemmaL;
                    // No stick
                    if (firstBlock || !lastBlock.StickRight)
                    {
                        pb.LocX = (short)Math.Round(blockX);
                        pb.LocY = (short)Math.Round(blockY);
                    }
                    // We break together
                    else
                    {
                        // Last block breaks onto this line
                        lastPB.LocX = (short)Math.Round(blockX);
                        lastPB.LocY = (short)Math.Round(blockY);
                        positionedBlocks[i - 1] = lastPB;
                        // We move on by last block's width plus (optional) space
                        blockX += ((float)lastBlock.Width);
                        if (!lastBlock.SenseId && lastBlock.SpaceAfter)
                            blockX += spaceWidth;
                        // So.
                        pb.LocX = (short)Math.Round(blockX);
                        pb.LocY = (short)Math.Round(blockY);
                    }
                }
                // Add to list of positioned blocks
                positionedBlocks[i] = pb;
                // This is a text block with a highlight? Collect it too!
                if (!block.SenseId && block.Hilite)
                {
                    if (currHiliteIndexes.Count != 0 && currHiliteIndexes[currHiliteIndexes.Count - 1] != i - 1)
                        doCollectHighlightRange(ref currHiliteIndexes);
                    currHiliteIndexes.Add(i);
                }
                // Move right by block's width; space optional
                blockX += ((float)block.Width);
                if (!block.SenseId && block.SpaceAfter)
                    blockX += spaceWidth;
                // Remeber "last block" for next round
                lastPB = pb;
                lastBlock = measuredBlocks[lastPB.BlockIdx];
                firstBlock = false;
            }
            // Collect any last highlights
            doCollectHighlightRange(ref currHiliteIndexes);
            // In link areas, fill in positioned blocks and calculate actual link areas.
            doCalculateLinkAreas();
            // Return bottom of content area.
            return measuredBlocks.Length == 0 ? blockY : blockY + lemmaLineHeight;
        }

        /// <summary>
        /// Fills in positioned blocks for links, and calculates links' active areas.
        /// </summary>
        private void doCalculateLinkAreas()
        {
            // No links: nothing to do.
            if (targetLinks == null) return;
            // Clear old positioned blocks and active areas in links
            foreach (LinkArea link in targetLinks)
            {
                link.ActiveAreas.Clear();
                link.PositionedBlocks.Clear();
            }
            // Look at each positioned block, add to correct link that has its block.
            // Positioned blocks will be in their correct order in each link's list.
            foreach (PositionedBlock pb in positionedBlocks)
                foreach (LinkArea link in targetLinks)
                    if (link.BlockIds.Contains(pb.BlockIdx)) link.PositionedBlocks.Add(pb);
            // Calculate links' active areas. That means encapsulating rectangles
            // of positioned blocks that are on the same line.
            foreach (LinkArea link in targetLinks)
            {
                // There is a single block
                if (link.PositionedBlocks.Count == 1)
                {
                    PositionedBlock pb = link.PositionedBlocks[0];
                    ushort width = measuredBlocks[pb.BlockIdx].Width;
                    Rectangle rect = new Rectangle(
                        (int)pb.LocX,
                        (int)pb.LocY,
                        (int)width,
                        (int)lemmaCharHeight);
                    link.ActiveAreas.Add(rect);
                }
                // There are multiple blocks
                else
                {
                    short lastY = short.MinValue;
                    short currLeft = short.MinValue;
                    for (int i = 0; i != link.PositionedBlocks.Count; ++i)
                    {
                        PositionedBlock pb = link.PositionedBlocks[i];
                        // Block on a new line
                        if (pb.LocY != lastY)
                        {
                            // We had a previous block, which completed an area
                            if (lastY != short.MinValue)
                            {
                                PositionedBlock previousPB = link.PositionedBlocks[i - 1];
                                ushort previousWidth = measuredBlocks[previousPB.BlockIdx].Width;
                                Rectangle rect = new Rectangle(
                                    (int)currLeft,
                                    (int)lastY,
                                    (int)(previousPB.LocX + ((float)previousWidth) - currLeft),
                                    (int)(lemmaCharHeight));
                                link.ActiveAreas.Add(rect);
                            }
                            // This block's left is merged area's left.
                            currLeft = pb.LocX;
                        }
                        lastY = pb.LocY;
                    }
                    // Last block completes last area
                    PositionedBlock lastPB = link.PositionedBlocks[link.PositionedBlocks.Count - 1];
                    ushort lastWidth = measuredBlocks[lastPB.BlockIdx].Width;
                    Rectangle lastRect = new Rectangle(
                        (int)currLeft,
                        (int)lastY,
                        (int)(lastPB.LocX + ((float)lastWidth) - currLeft),
                        (int)(lemmaCharHeight));
                    link.ActiveAreas.Add(lastRect);
                }
            }
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
        private static bool doAnalyzeHanzi(Graphics g, string str, bool isSimp, StringFormat sf,
            List<HeadBlock> blocks, ref PointF loc, float right)
        {
            byte fntZhoHead = isSimp ? fntZhoHeadSimp : fntZhoHeadTrad;
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
                    Char = str[i],
                    Size = g.MeasureString(cstr, getFont(fntZhoHead), 65535, sf),
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
            // We only measure simplified. Assume simplified and traditional fonts come in matching pairs -> same size.
            if (ideoSize.Width == 0)
            {
                ideoSize = g.MeasureString(ideoTestStr, getFont(fntZhoHeadSimp), 65535, sf);
                var si = HanziMeasure.Instance.GetMeasures(Magic.ZhoSimpContentFontFamily, Magic.ZhoResultFontSize);
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
                lbrk |= doAnalyzeHanzi(g, entry.ChSimpl, true, sf, headInfo.SimpBlocks, ref loc, headInfo.HeadwordRight);
            }
            if (analyzedScript == SearchScript.Traditional || analyzedScript == SearchScript.Both)
            {
                //loc.X = padLeft;
                loc.X = ((float)padLeft) + ideoSize.Width;
                if (analyzedScript == SearchScript.Both) loc.Y += ideoLineHeight;
                lbrk |= doAnalyzeHanzi(g, entry.ChTrad, false, sf, headInfo.TradBlocks, ref loc, headInfo.HeadwordRight);
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
            var pcoll = entry.GetPinyinForDisplay(diacritics,
                res.PinyinHiliteStart, res.PinyinHiliteLength,
                out pinyinInfo.HiliteStart, out pinyinInfo.HiliteLength);
            float cx = headInfo.HeadwordRight + (float)padMid;
            float ctop = padTop;
            for (int i = 0; i != pcoll.Count; ++i)
            {
                PinyinSyllable ps = pcoll[i];
                // New pinyin block
                PinyinBlock pb = new PinyinBlock();
                // Text: syllable's display text
                string text = ps.GetDisplayString(true);
                pb.TextPos = textPool.PoolString(text);
                // If text is punctuation, glue it to previous syllable
                if (text.Length == 1 && char.IsPunctuation(text[0]) && i > 0) cx -= pinyinSpaceWidth;
                // Block's size and relative location
                SizeF sz = g.MeasureString(text, getFont(fntPinyinHead), 65535, sf);
                pb.Rect = new RectangleF(cx, ctop, sz.Width, sz.Height);
                cx += sz.Width + pinyinSpaceWidth;
                // Add block
                pinyinInfo.Blocks.Add(pb);
            }
            // Height of whole pinyin area
            pinyinInfo.PinyinHeight = pinyinInfo.Blocks[0].Rect.Height;
        }
    }
}
