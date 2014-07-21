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
    internal partial class OneResultControl : ZenControl
    {
        public readonly CedictResult Res;
        private readonly int maxHeadLength;
        private readonly bool odd;

        private readonly int padLeft;
        private readonly int padTop;
        private readonly int padBottom;
        private readonly int padMid;
        private readonly int padRight;

        private const string ideoTestStr = "中";
        private const string spaceTestStr = "f";
        private static SizeF ideoSize = new SizeF(0, 0);
        private static float spaceWidth = 0;
        private static float lemmaLineHeight = 0;
        
        private int analyzedWidth = int.MinValue;
        private SearchScript analyzedScript;
        private HeadInfo headInfo = null;
        private PinyinInfo pinyinInfo = null;
        private List<Block> measuredBlocks = null;
        private List<PositionedBlock> positionedBlocks = null;

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

        // Graphics resource: static, singleton, never disposed.
        // When we're quitting it doesn't matter anymore, anyway.
        private static Font fntZho;
        private static Font fntPinyin;
        private static Font fntEquiv;
        private static Font fntMeta;
        private static Font fntSenseId;

        static OneResultControl()
        {
            fntZho = new Font(ZenParams.ZhoFontFamily, ZenParams.ZhoFontSize);
            fntPinyin = new Font(ZenParams.PinyinFontFamily, ZenParams.PinyinFontSize, FontStyle.Bold);
            fntEquiv = new Font(ZenParams.LemmaFontFamily, ZenParams.LemmaFontSize);
            fntMeta = new Font(ZenParams.LemmaFontFamily, ZenParams.LemmaFontSize, FontStyle.Italic);
            fntSenseId = new Font(ZenParams.LemmaFontFamily, ZenParams.LemmaFontSize * 0.8F);
        }

        private void doHiliteOneHeadBlock(Graphics g, Pen p, HeadBlock hb, bool tradSimpMid,
            bool leftHorn, bool rightHorn)
        {
            float leftX = hb.Loc.X + (float)AbsLeft;
            float y = hb.Loc.Y + hb.Size.Height + (float)AbsTop;
            if (!tradSimpMid) y -= ideoSize.Height * 0.1F;
            else y -= ideoSize.Height * 0.05F;
            // Left horn
            if (leftHorn)
            {
                // If only pinyin or both but with line break: go up
                if (!tradSimpMid)
                {
                    float hornY = y - ideoSize.Height / 9.0F;
                    g.DrawLine(p, (int)leftX, (int)y, (int)leftX, (int)hornY);
                }
                // Otherwise, go up and down: this will be sitting between traditional and simplified
                else
                {
                    float hornY1 = y - ideoSize.Height / 9.0F;
                    float hornY2 = y + ideoSize.Height / 9.0F;
                    g.DrawLine(p, (int)leftX, (int)hornY1, (int)leftX, (int)hornY2);
                }
            }
            // Underline
            float rightX = hb.Loc.X + hb.Size.Width + (float)AbsLeft;
            g.DrawLine(p, (int)leftX, (int)y, (int)rightX, (int)y);
            // Right horn
            if (rightHorn)
            {
                // If only pinyin or both but with line break: go up
                if (!tradSimpMid)
                {
                    float hornY = y - ideoSize.Height / 9.0F;
                    g.DrawLine(p, (int)rightX, (int)y, (int)rightX, (int)hornY);
                }
                // Otherwise, go up and down: this will be sitting between traditional and simplified
                else
                {
                    float hornY1 = y - ideoSize.Height / 9.0F;
                    float hornY2 = y + ideoSize.Height / 9.0F;
                    g.DrawLine(p, (int)rightX, (int)hornY1, (int)rightX, (int)hornY2);
                }
            }
        }

        private void doPaintHanziHilites(Graphics g)
        {
            using (Pen p = new Pen(Color.Maroon))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                // In simplified
                if (headInfo.SimpBlocks.Count != 0)
                {
                    for (int ix = Res.HanziHiliteStart; ix != Res.HanziHiliteStart + Res.HanziHiliteLength; ++ix)
                    {
                        HeadBlock hb = headInfo.SimpBlocks[ix];
                        doHiliteOneHeadBlock(g, p, hb, headInfo.HeadMode == HeadMode.BothSingleLine,
                            ix == Res.HanziHiliteStart, ix + 1 == Res.HanziHiliteStart + Res.HanziHiliteLength);
                    }
                }
                // In traditional
                if (headInfo.TradBlocks.Count != 0 && headInfo.HeadMode != HeadMode.BothSingleLine)
                {
                    for (int ix = Res.HanziHiliteStart; ix != Res.HanziHiliteStart + Res.HanziHiliteLength; ++ix)
                    {
                        HeadBlock hb = headInfo.TradBlocks[ix];
                        doHiliteOneHeadBlock(g, p, hb, false,
                            ix == Res.HanziHiliteStart, ix + 1 == Res.HanziHiliteStart + Res.HanziHiliteLength);
                    }
                }
            }
        }

        public override void DoPaint(Graphics g)
        {
            // If size changed and we get a pain requested without having re-analized:
            // Analyze now. Not the best time here in paint, but must do.
            if (analyzedWidth != Width) Analyze(g, Width, analyzedScript);

            // Background. Alternating at that!
            Color bgcol = odd ? Color.FromArgb(248, 248, 255) : Color.White;
            using (Brush b = new SolidBrush(bgcol))
            {
                g.FillRectangle(b, AbsLeft, AbsTop, Width, Height);
            }

            // This is how we draw text
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            StringFormat sf = StringFormat.GenericTypographic;
            // Headword, pinyin, entry body
            using (Brush bnorm = new SolidBrush(Color.Black))
            using (Brush bfade = new SolidBrush(Color.FromArgb(200, 200, 200)))
            using (Pen pnorm = new Pen(bnorm))
            {
                // Simplified and traditional - headword
                foreach (HeadBlock hb in headInfo.SimpBlocks)
                {
                    PointF loc = new PointF(hb.Loc.X + (float)AbsLeft, hb.Loc.Y + (float)AbsTop);
                    g.DrawString(hb.Char, fntZho, bnorm, loc, sf);
                }
                foreach (HeadBlock hb in headInfo.TradBlocks)
                {
                    PointF loc = new PointF(hb.Loc.X + (float)AbsLeft, hb.Loc.Y + (float)AbsTop);
                    Brush b = hb.Faded ? bfade : bnorm;
                    g.DrawString(hb.Char, fntZho, b, loc, sf);
                }
                // Pinyin
                float rx = (float)AbsLeft + headInfo.HeadwordRight + (float)padMid;
                g.DrawString(pinyinInfo.PinyinDisplay, fntPinyin, bnorm, rx, padTop + (float)AbsTop, sf);
                // All the measured and positioned blocks in entry body
                float fLeft = (float)AbsLeft;
                float fTop = (float)AbsTop;
                foreach (PositionedBlock pb in positionedBlocks)
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    // Sense ID
                    if (pb.Block is SenseIdBlock)
                    {
                        SenseIdBlock sib = pb.Block as SenseIdBlock;
                        float pad = lemmaLineHeight * 0.1F;
                        g.DrawEllipse(pnorm,
                            pb.Loc.X + fLeft,
                            pb.Loc.Y + fTop + Scale * pad,
                            sib.Size.Width - 2.0F * pad,
                            sib.Size.Height - 2.0F * pad);
                        g.DrawString(sib.Text, fntSenseId, bnorm,
                            pb.Loc.X + fLeft + 2.0F * pad,
                            pb.Loc.Y + fTop + 1.5F * pad, sf);
                    }
                    // Text
                    else if (pb.Block is TextBlock)
                    {
                        TextBlock tb = pb.Block as TextBlock;
                        g.DrawString(tb.Text, tb.Font, bnorm, pb.Loc.X + fLeft, pb.Loc.Y + fTop, sf);
                    }
                }
            }
            // Hanzi highlights
            doPaintHanziHilites(g);
        }

        /// <summary>
        /// Measures an array of alphabetic words, which are all typographic display blocks.
        /// </summary>
        /// <param name="g">A Graphics object used for measurements.</param>
        /// <param name="sf">StringFormat used for measurements.</param>
        /// <param name="font">Display font for block.</param>
        /// <param name="words">Array of words to measure.</param>
        /// <param name="blocks">List of measured blocks to APPEND to.</param>
        private static void doMeasureWords(Graphics g, StringFormat sf, Font font,
            string[] words, List<Block> blocks)
        {
            foreach (string wd in words)
            {
                if (wd.Trim() == "") continue;
                TextBlock tb = new TextBlock
                {
                    Size = g.MeasureString(wd, fntEquiv, 65535, sf),
                    StickRight = false,
                    Font = font,
                    Text = wd,
                };
                blocks.Add(tb);
            }
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
            SizeF xSize = g.MeasureString("x", fntEquiv, 65535, sf);
            SizeF senseIdxSize = new SizeF(xSize.Height, xSize.Height);

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
                // Each word of the domain, split by spaces is a block
                string[] domParts = cm.Domain.Split(new char[] { ' ' });
                doMeasureWords(g, sf, fntMeta, domParts, measuredBlocks);
                // Each word of the meaning (equiv), split by spaces
                string[] equivParts = cm.Equiv.Split(new char[] { ' ' });
                doMeasureWords(g, sf, fntEquiv, equivParts, measuredBlocks);
                // Each word of the note
                string[] noteParts = cm.Note.Split(new char[] { ' ' });
                doMeasureWords(g, sf, fntMeta, noteParts, measuredBlocks);
            }
        }

        /// <summary>
        /// Calculates layout of content in entry body, taking current width into account for line breaks.
        /// </summary>
        /// <param name="lemmaL">Left position of body content area.</param>
        /// <param name="lemmaW">Width of body content area.</param>
        /// <returns>Bottom of content area.</returns>
        private float doArrangeBlocks(float lemmaL, float lemmaW)
        {
            float lemmaTop = (float)padTop + pinyinInfo.PinyinSize.Height * 1.3F;

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
                        // We move on by last block's width plus space
                        blockX += lastPB.Block.Size.Width + spaceWidth;
                        // Sorry, no/less space after sense ID
                        if (lastPB.Block is SenseIdBlock) blockX -= spaceWidth;
                        // So.
                        pb.Loc = new PointF(blockX, blockY);
                    }
                }
                positionedBlocks.Add(pb);
                // Move right by block's width plus space
                blockX += block.Size.Width + spaceWidth;
                // Sorry, no/less space after sense ID
                if (block is SenseIdBlock) blockX -= spaceWidth;
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
                    Size = g.MeasureString(cstr, fntZho, 65535, sf),
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
                    loc.Y += ideoSize.Height;
                    doRightAlign(blocks, firstCharOfLine, charsOnLine - 1, right);
                    charsOnLine = 1;
                    firstCharOfLine = blocks.Count - 1;
                    hb.Loc = loc;
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

            // On-demand: measure a single ideograph's width
            if (ideoSize.Width == 0)
                ideoSize = g.MeasureString(ideoTestStr, fntZho, 65535, sf);

            headInfo = new HeadInfo();
            if (analyzedScript == SearchScript.Simplified) headInfo.HeadMode = HeadMode.OnlySimp;
            else if (analyzedScript == SearchScript.Traditional) headInfo.HeadMode = HeadMode.OnlyTrad;
            else headInfo.HeadMode = HeadMode.BothSingleLine;
            // For width of headword, use padLeft from border, plus 4 to 6 ideographs' worth of space
            // Depending on longest headword in entire list
            int hwChars = maxHeadLength;
            if (hwChars < 4) hwChars = 4;
            if (hwChars > 6) hwChars = 6;
            float hwidth = ((float)hwChars) * ideoSize.Width;
            headInfo.HeadwordRight = padLeft + hwidth;
            // Measure simplified chars from start; break when needed
            PointF loc = new PointF(padLeft, padTop / 2.0F); // Less padding above hanzi - font leaves enough space
            bool lbrk = false;
            if (analyzedScript == SearchScript.Simplified || analyzedScript == SearchScript.Both)
            {
                lbrk |= doAnalyzeHanzi(g, Res.Entry.ChSimpl, sf, headInfo.SimpBlocks, ref loc, headInfo.HeadwordRight);
            }
            if (analyzedScript == SearchScript.Traditional || analyzedScript == SearchScript.Both)
            {
                loc.X = padLeft;
                if (analyzedScript == SearchScript.Both) loc.Y += ideoSize.Height;
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
            headInfo.HeadwordBottom = loc.Y + ideoSize.Height;
            // If we had a line break and we're showing both scripts, update info
            if (analyzedScript == SearchScript.Both && lbrk)
                headInfo.HeadMode = HeadMode.BothMultiLine;
        }

        private void doAnalyzePinyin(Graphics g)
        {
            // If already measured, nothing to do
            if (pinyinInfo != null) return;

            // This is how we measure
            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            pinyinInfo = new PinyinInfo();
            // Measure pinyin text
            pinyinInfo.PinyinDisplay = "";
            foreach (string ps in Res.Entry.Pinyin)
            {
                if (pinyinInfo.PinyinDisplay.Length > 0) pinyinInfo.PinyinDisplay += " ";
                // TO-DO: convert tone numbers to accents here
                pinyinInfo.PinyinDisplay += ps;
            }
            pinyinInfo.PinyinSize = g.MeasureString(pinyinInfo.PinyinDisplay, fntPinyin, 65535, sf);
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
            }

            // This is how we measure
            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // On-demand: measure a space's width - for entry text flow
            // Also line height in entry text
            if (spaceWidth == 0)
            {
                SizeF sz = g.MeasureString(spaceTestStr, fntEquiv, 65535, sf);
                spaceWidth = (int)sz.Width;
                lemmaLineHeight = sz.Height * 1.1F;
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
