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
        private const string zhoTestStr = "中中中中";
        private const string spaceTestStr = "x";
        private readonly int padTop;
        private readonly int padBottom;
        private readonly int padMid;
        private readonly int padRight;

        public int LastTop = int.MinValue;
        public readonly CedictResult Res;

        private static int zhoWidth = 0;
        private static float spaceWidth = 0;
        private static float lemmaLineHeight;

        private int analyzedWidth = int.MinValue;
        private float simpTop = float.MinValue;
        private SizeF simpSize;
        private float simpLeft;
        private float tradTop;
        private SizeF tradSize;
        private float tradLeft;
        private string strPinyin;
        private SizeF pinyinSize;
        private float lemmaTop;
        private List<Block> measuredBlocks = null;
        private List<PositionedBlock> positionedBlocks = null;

        public OneResultControl(ZenControl owner, CedictResult cr)
            : base(owner)
        {
            this.Res = cr;
            padTop = (int)(5.0F * Scale);
            padBottom = (int)(10.0F * Scale);
            padMid = (int)(10.0F * Scale);
            padRight = (int)(5.0F * Scale);
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

        public override void DoPaint(System.Drawing.Graphics g)
        {
            if (analyzedWidth != Width) Analyze(g, Width);
            for (int i = 0; i != Height; ++i)
            {
                using (Pen p = new Pen(Color.White))
                {
                    g.DrawLine(p, AbsLeft, AbsTop + i, AbsLeft + Width, AbsTop + i);
                }
            }
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using (Brush b = new SolidBrush(Color.Black))
            using (Pen p = new Pen(b))
            {
                // Simplified and traditional - headword
                g.DrawString(Res.Entry.ChSimpl, fntZho, b, simpLeft + (float)AbsLeft, simpTop + (float)AbsTop);
                g.DrawString(Res.Entry.ChTrad, fntZho, b, tradLeft + (float)AbsLeft, tradTop + (float)AbsTop);
                // Pinyin
                float rx = (float)AbsLeft + zhoWidth + (float)padMid;
                g.DrawString(strPinyin, fntPinyin, b, rx, padTop + (float)AbsTop);
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
                        g.DrawEllipse(p,
                            pb.Loc.X + fLeft + pad,
                            pb.Loc.Y + fTop + Scale * pad,
                            sib.Size.Width - 2.0F * pad,
                            sib.Size.Height - 2.0F * pad);
                        g.DrawString(sib.Text, fntSenseId, b,
                            pb.Loc.X + fLeft + 2.5F * pad,
                            pb.Loc.Y + fTop + 1.5F * pad);
                    }
                    // Text
                    else if (pb.Block is TextBlock)
                    {
                        TextBlock tb = pb.Block as TextBlock;
                        g.DrawString(tb.Text, tb.Font, b, pb.Loc.X + fLeft, pb.Loc.Y + fTop);
                    }
                }
            }
        }

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

        public float doArrangeBlocks(float lemmaL, float lemmaW)
        {
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
            return blockY;
        }

        // Analyze layout with provided width; assume corresponding height; does not invalidate
        public void Analyze(Graphics g, int width)
        {
            // If width has not changed, nothing to do.
            if (analyzedWidth == width) return;
            analyzedWidth = Width;

            // This is how we measure
            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // On-demand: measure a single ideograph's width - for headword
            if (zhoWidth == 0)
                zhoWidth = (int)g.MeasureString(zhoTestStr, fntZho, 65535, sf).Width;
            // On-demand: measure a space's width - for entry text flow
            // Also line height in entry text
            if (spaceWidth == 0)
            {
                SizeF sz = g.MeasureString(spaceTestStr, fntEquiv, 65535, sf);
                spaceWidth = (int)sz.Width;
                lemmaLineHeight = sz.Height;
            }

            // Size and location of headword: simplified and traditional chars
            // If simpTop is not float.MinValue, we've done this already
            // On that value hinges pinyin measurement too
            if (simpTop == float.MinValue)
            {
                simpSize = g.MeasureString(Res.Entry.ChSimpl, fntZho, 65535, sf);
                simpLeft = (float)zhoWidth - simpSize.Width;
                simpTop = padTop;
                tradSize = g.MeasureString(Res.Entry.ChTrad, fntZho, 65535, sf);
                tradLeft = (float)zhoWidth - tradSize.Width;
                tradTop = simpTop + simpSize.Height;

                // Measure pinyin text
                strPinyin = "";
                foreach (string ps in Res.Entry.Pinyin)
                {
                    if (strPinyin.Length > 0) strPinyin += " ";
                    // TO-DO: convert tone numbers to accents here
                    strPinyin += ps;
                }
                pinyinSize = g.MeasureString(strPinyin, fntPinyin, 65535, sf);
            }

            // OK, now onto entry
            lemmaTop = padTop + pinyinSize.Height;
            // Measure blocks in themselves on demand
            doMeasureBlocks(g);
            // Arrange blocks
            float lemmaW = (float)width - zhoWidth - padMid - padRight;
            float lemmaL = zhoWidth + padMid;
            float lastTop = doArrangeBlocks(lemmaL, lemmaW);

            // My height: bottom of headword or bottom of entry, whichever is lower
            float entryHeight = lastTop + lemmaLineHeight + padBottom;
            float zhoHeight = tradTop + tradSize.Height + padBottom;
            float trueHeight = Math.Max(entryHeight, zhoHeight);
            // Assume this height, and also provided width
            Size = new Size(width, (int)trueHeight);
        }
    }
}
