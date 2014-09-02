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
    partial class OneResultControl
    {
        /// <summary>
        /// Draws hilite for one character in headword.
        /// </summary>
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

        /// <summary>
        /// Paints all hilites in headword.
        /// </summary>
        private void doPaintHanziHilites(Graphics g)
        {
            if (Res.HanziHiliteStart == -1)
                return;

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

        /// <summary>
        /// Paint headword's pinyin, with highlights.
        /// </summary>
        private void doPaintPinyin(Graphics g, Brush bfont, SolidBrush bhilite, StringFormat sf, Color bgcol)
        {
            if (pinyinInfo == null || pinyinInfo.Blocks.Count == 0)
                return;

            // We do have highlights
            if (pinyinInfo.HiliteStart != -1)
            {
                // Needed to make gradient work
                g.SmoothingMode = SmoothingMode.None;
                float edgeLeft = 0;
                float edgeRight = 0;
                for (int i = 0; i != pinyinInfo.Blocks.Count; ++i)
                {
                    PinyinBlock pb = pinyinInfo.Blocks[i];
                    // Where?
                    PointF loc = pb.Rect.Location;
                    loc.Y += (float)AbsTop;
                    // Not in highlight now
                    if (i < pinyinInfo.HiliteStart || i >= pinyinInfo.HiliteStart + pinyinInfo.HiliteLength)
                        continue;
                    // Remember edges
                    if (i == pinyinInfo.HiliteStart)
                        edgeLeft = pb.Rect.Left;
                    if (i + 1 == pinyinInfo.HiliteStart + pinyinInfo.PinyinHeight)
                        edgeRight = pb.Rect.Right;
                    // Hilight syllable itself
                    RectangleF hlr = new RectangleF(loc, pb.Rect.Size);
                    g.FillRectangle(bhilite, hlr);
                    // For middle and last: hilight space before
                    if (i > pinyinInfo.HiliteStart)
                    {
                        float xprev = pinyinInfo.Blocks[i - 1].Rect.Right;
                        hlr.X = xprev;
                        hlr.Width = loc.X - xprev;
                        g.FillRectangle(bhilite, hlr);
                    }
                }
                // Left and right edges: gradient
                // Extends one space's width beyond edge
                // Reach max color two spaces' width within syllable, OR
                // by midpoint, if edgeRight - edgeLeft < 4 spaces
                float y = pinyinInfo.Blocks[0].Rect.Top + (float)AbsTop;
                float w = pinyinSpaceWidth;
                if (edgeLeft != 0 && edgeRight != 0)
                {
                    float midPoint = (edgeLeft + edgeRight) / 2.0F;
                    float whalf = (edgeRight - edgeLeft) / 2.0F;
                    RectangleF rleft;
                    if (whalf < 2.0F * w)
                        rleft = new RectangleF(edgeLeft - w, y, w + whalf, pinyinInfo.PinyinHeight);
                    else
                        rleft = new RectangleF(edgeLeft - w, y, w * 3.0F, pinyinInfo.PinyinHeight);
                    using (LinearGradientBrush lbr = new LinearGradientBrush(rleft, bgcol, ZenParams.PinyinHiliteColor, LinearGradientMode.Horizontal))
                    {
                        g.FillRectangle(lbr, rleft);
                    }
                    RectangleF rright;
                    if (whalf < 2.0F * w)
                        rright = new RectangleF(midPoint, y, w + whalf, pinyinInfo.PinyinHeight);
                    else
                        rright = new RectangleF(edgeRight - 2.0F * w, y, w * 3.0F, pinyinInfo.PinyinHeight);
                    using (LinearGradientBrush lbr = new LinearGradientBrush(rright, ZenParams.PinyinHiliteColor, bgcol, LinearGradientMode.Horizontal))
                    {
                        g.FillRectangle(lbr, rright);
                    }
                }
            }

            // Now, write each syllable - so it goes on top of hilites
            foreach (PinyinBlock pb in pinyinInfo.Blocks)
            {
                PointF loc = pb.Rect.Location;
                loc.Y += (float)AbsTop;
                // Draw string
                g.DrawString(pb.Text, fntPinyin, bfont, loc, sf);

            }
        }

        /// <summary>
        /// Paints full control. Analyzes on demand, but meant to be called after analysis up front.
        /// </summary>
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
                using (SolidBrush bhilite = new SolidBrush(ZenParams.PinyinHiliteColor))
                {
                    doPaintPinyin(g, bnorm, bhilite, sf, bgcol);
                }
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
    }
}
