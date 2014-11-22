using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

using ZD.Common;
using ZD.Gui.Zen;

namespace ZD.Gui
{
    partial class OneResultControl
    {
        /// <summary>
        /// Paints all hilites in headword.
        /// </summary>
        private void doPaintHanziHilites(Graphics g, Color bgcol)
        {
            if (res.HanziHiliteStart == -1)
                return;

            g.SmoothingMode = SmoothingMode.None;
            var si = HanziMeasure.Instance.GetMeasures(Magic.ZhoSimpContentFontFamily, Magic.ZhoResultFontSize);
            HeadBlock hb;
            RectangleF rect;
            // Width of gradient
            float gradw = ideoSize.Width / 2.0F;
            // Extent of gradient outside character
            float gradext = 0;
            using (Brush b = new SolidBrush(Magic.HiliteColor))
            {
                // In simplified
                if (headInfo.SimpBlocks.Count != 0)
                {
                    // Solid highlight on each character
                    for (int ix = res.HanziHiliteStart; ix != res.HanziHiliteStart + res.HanziHiliteLength; ++ix)
                    {
                        hb = headInfo.SimpBlocks[ix];
                        rect = new RectangleF(hb.Loc.X, hb.Loc.Y, hb.Size.Width, si.RealRect.Height);
                        g.FillRectangle(b, rect);
                    }
                    // First and last chars get gradient on left and right
                    hb = headInfo.SimpBlocks[res.HanziHiliteStart];
                    rect = new RectangleF(hb.Loc.X, hb.Loc.Y, gradw, si.RealRect.Height);
                    rect.X -= gradext;
                    using (LinearGradientBrush lgb = new LinearGradientBrush(rect, bgcol, Magic.HiliteColor, LinearGradientMode.Horizontal))
                    {
                        g.FillRectangle(lgb, rect);
                    }
                    hb = headInfo.SimpBlocks[res.HanziHiliteStart + res.HanziHiliteLength - 1];
                    rect = new RectangleF(hb.Loc.X + hb.Size.Width, hb.Loc.Y, gradw, si.RealRect.Height);
                    rect.X += gradext;
                    rect.X -= gradw;
                    using (LinearGradientBrush lgb = new LinearGradientBrush(rect, Magic.HiliteColor, bgcol, LinearGradientMode.Horizontal))
                    {
                        g.FillRectangle(lgb, rect);
                    }
                }
                // In traditional
                if (headInfo.TradBlocks.Count != 0)
                {
                    // Solid highlight on each character
                    for (int ix = res.HanziHiliteStart; ix != res.HanziHiliteStart + res.HanziHiliteLength; ++ix)
                    {
                        hb = headInfo.TradBlocks[ix];
                        rect = new RectangleF(hb.Loc.X, hb.Loc.Y, hb.Size.Width, si.RealRect.Height);
                        g.FillRectangle(b, rect);
                    }
                    // First and last chars get gradient on left and right
                    hb = headInfo.TradBlocks[res.HanziHiliteStart];
                    rect = new RectangleF(hb.Loc.X, hb.Loc.Y, gradw, si.RealRect.Height);
                    rect.X -= gradext;
                    using (LinearGradientBrush lgb = new LinearGradientBrush(rect, bgcol, Magic.HiliteColor, LinearGradientMode.Horizontal))
                    {
                        g.FillRectangle(lgb, rect);
                    }
                    hb = headInfo.TradBlocks[res.HanziHiliteStart + res.HanziHiliteLength - 1];
                    rect = new RectangleF(hb.Loc.X + hb.Size.Width, hb.Loc.Y, gradw, si.RealRect.Height);
                    rect.X += gradext;
                    rect.X -= gradw;
                    using (LinearGradientBrush lgb = new LinearGradientBrush(rect, Magic.HiliteColor, bgcol, LinearGradientMode.Horizontal))
                    {
                        g.FillRectangle(lgb, rect);
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
                    // Not in highlight now
                    if (i < pinyinInfo.HiliteStart || i >= pinyinInfo.HiliteStart + pinyinInfo.HiliteLength)
                        continue;
                    // Remember edges
                    if (i == pinyinInfo.HiliteStart)
                        edgeLeft = pb.Rect.Left;
                    if (i + 1 == pinyinInfo.HiliteStart + pinyinInfo.HiliteLength)
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
                float y = pinyinInfo.Blocks[0].Rect.Top;
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
                    using (LinearGradientBrush lbr = new LinearGradientBrush(rleft, bgcol, Magic.HiliteColor, LinearGradientMode.Horizontal))
                    {
                        g.FillRectangle(lbr, rleft);
                    }
                    RectangleF rright;
                    if (whalf < 2.0F * w)
                        rright = new RectangleF(midPoint, y, w + whalf, pinyinInfo.PinyinHeight);
                    else
                        rright = new RectangleF(edgeRight - 2.0F * w, y, w * 3.0F, pinyinInfo.PinyinHeight);
                    using (LinearGradientBrush lbr = new LinearGradientBrush(rright, Magic.HiliteColor, bgcol, LinearGradientMode.Horizontal))
                    {
                        g.FillRectangle(lbr, rright);
                    }
                }
            }

            // Now, write each syllable - so it goes on top of hilites
            foreach (PinyinBlock pb in pinyinInfo.Blocks)
            {
                PointF loc = pb.Rect.Location;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                // Draw string
                g.DrawString(textPool.GetString(pb.TextPos), getFont(fntPinyinHead), bfont, loc, sf);

            }
        }

        /// <summary>
        /// Returns vertical offset of target Hanzi; retrieves Hanzi measures on demand.
        /// </summary>
        private float getTargetHanziOfs()
        {
            if (targetHanziOfs != 0) return targetHanziOfs;
            // Calculate on demand
            Font myFontZho = getFont(fntSenseHanziSimp);
            var sizeInfo = HanziMeasure.Instance.GetMeasures(myFontZho.Name, myFontZho.Size);
            float hanziMidY = sizeInfo.RealRect.Top + sizeInfo.RealRect.Height / 2.0F;
            Font myFontLatn = getFont(fntSenseLatin);
            FontFamily ffLatn = myFontLatn.FontFamily;
            float latinMidY = getFont(fntSenseLatin).Height * 0.55F; // Tweak when Latin font changes
            targetHanziOfs = latinMidY - hanziMidY;
            return targetHanziOfs;
        }

        /// <summary>
        /// Paints target (entry body).
        /// </summary>
        private void doPaintTarget(Graphics g, Pen pnorm, Brush bnorm, StringFormat sf)
        {
            // Take index of hovered-over sense once - this is atomic, and we're prolly in different thread from mouse
            short hsix = hoverSenseIx;
            Brush bhover = null;
            try
            {
                // All the measured and positioned blocks in entry body
                short currSenseIx = -1;
                foreach (PositionedBlock pb in positionedBlocks)
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    Block block = measuredBlocks[pb.BlockIdx];
                    if (block.FirstInCedictSense) ++currSenseIx;
                    bool hoverSense = currSenseIx == hsix;
                    // Sense ID
                    if (block.SenseId)
                    {
                        float pad = lemmaLineHeight * 0.1F;
                        if (!hoverSense)
                        {
                            g.DrawEllipse(pnorm,
                                pb.LocX,
                                pb.LocY + scale * pad,
                                ((float)block.Width) - 2.0F * pad,
                                lemmaCharHeight - 2.0F * pad);
                            g.DrawString(textPool.GetString(block.TextPos), getFont(fntSenseId), bnorm,
                                pb.LocX + 2.0F * pad,
                                pb.LocY + /* 1.5F * */ pad, sf); // TO-DO: vertical paddig of character will need more work.
                        }
                        else
                        {
                            g.FillEllipse(bnorm,
                                pb.LocX,
                                pb.LocY + scale * pad,
                                ((float)block.Width) - 2.0F * pad,
                                lemmaCharHeight - 2.0F * pad);
                            using (Brush bgBrush = new SolidBrush(ZenParams.WindowColor))
                            g.DrawString(textPool.GetString(block.TextPos), getFont(fntSenseId), bgBrush,
                                pb.LocX + 2.0F * pad,
                                pb.LocY + /* 1.5F * */ pad, sf); // TO-DO: vertical paddig of character will need more work.
                        }
                    }
                    // Text
                    else
                    {
                        // Extra vertical offset on Hanzi blocks
                        float vOfs = 0;
                        bool isHanzi = false;
                        if (block.FontIdx == fntMetaHanziSimp || block.FontIdx == fntMetaHanziTrad ||
                            block.FontIdx == fntSenseHanziSimp || block.FontIdx == fntSenseHanziTrad)
                        {
                            vOfs += getTargetHanziOfs();
                            isHanzi = true;
                        }
                        // No hover: draw with normal brush
                        Brush brush = bnorm;
                        // Hover: create hover brush on demand; draw with that
                        if (hoverLink != null)
                        {
                            if (hoverLink.BlockIds.Contains(pb.BlockIdx))
                            {
                                if (bhover == null) bhover = new SolidBrush(Magic.LinkHoverColor);
                                brush = bhover;
                            }
                        }
                        if (isHanzi) g.TextRenderingHint = TextRenderingHint.AntiAlias;
                        else g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                        g.DrawString(textPool.GetString(block.TextPos), getFont(block.FontIdx),
                            brush, pb.LocX, pb.LocY + vOfs, sf);
                    }
                }
            }
            finally
            {
                if (bhover != null) bhover.Dispose();
            }
        }
        
        /// <summary>
        /// Paints highlights behind target text blocks.
        /// </summary>
        private void doPaintTargetHilites(Graphics g, Color bgcol)
        {
            // No highlights - done here
            if (targetHiliteIndexes == null) return;

            // Needed to make gradient work
            g.SmoothingMode = SmoothingMode.None;
            // We offset highlight vertically for more pleasing aesthetics (lots of empty space at top in text)
            float topOfs = lemmaCharHeight / 15.0F; // TO-DO: This will need more work; depends on font size & Scale
            // All the measured and positioned blocks in entry body
            using (Brush b = new SolidBrush(Magic.HiliteColor))
            {
                // Every adjacent range
                foreach (List<int> idxList in targetHiliteIndexes)
                {
                    int lastY = int.MinValue;
                    int lastRight = int.MinValue;
                    for (int i = 0; i != idxList.Count; ++i)
                    {
                        PositionedBlock pb = positionedBlocks[idxList[i]];
                        Block tb = measuredBlocks[pb.BlockIdx];
                        // !! Paint gradients first.
                        // Linear gradient brush has ugly bug where white line (1px or less) is drawn right at the darkest edge
                        // If we draw a bit bigger gradient, then overdraw with solid, this will be covered up.
                        // Thank you, Microsoft. Beer's on me.
                        // EXtending gradient on left
                        if (i == 0)
                        {
                            RectangleF rleft = new RectangleF(
                                pb.LocX - 2.0F * spaceWidth + 1.0F, pb.LocY,
                                2.0F * spaceWidth, lemmaCharHeight);
                            rleft.Y += topOfs;
                            using (LinearGradientBrush lbr = new LinearGradientBrush(rleft, bgcol, Magic.HiliteColor, LinearGradientMode.Horizontal))
                            {
                                rleft.X += 1.0F;
                                rleft.Width -= 1.0F;
                                g.FillRectangle(lbr, rleft);
                            }
                        }
                        // Extending gradient on right
                        if (i == idxList.Count - 1)
                        {
                            RectangleF rright = new RectangleF(
                                pb.LocX + ((float)tb.Width) - 1.0F, pb.LocY,
                                2.0F * spaceWidth, lemmaCharHeight);
                            rright.Y += topOfs;
                            using (LinearGradientBrush lbr = new LinearGradientBrush(rright, Magic.HiliteColor, bgcol, LinearGradientMode.Horizontal))
                            {
                                g.FillRectangle(lbr, rright);
                            }
                        }
                        // Rectangle behind this specific block
                        RectangleF rect = new RectangleF(
                            new PointF(pb.LocX, pb.LocY),
                            new SizeF((float)tb.Width, lemmaCharHeight));
                        rect.X -= 1.0F; // Extend solid area to cover up buggy gradient edge
                        rect.Y += topOfs;
                        rect.Width += 2.0F; // Extend solid area to cover up buggy gradient edge
                        g.FillRectangle(b, rect);
                        // If this block is on the same line as before, fill space between blocks
                        if (pb.LocY == lastY)
                        {
                            rect.X = lastRight;
                            rect.Width = pb.LocX - lastRight;
                            g.FillRectangle(b, rect);
                        }
                        // Remember Y of block so we can fill empty areas between blocks on the same line
                        lastY = pb.LocY;
                        lastRight = pb.LocX + tb.Width;
                    }
                }
            }
        }

        /// <summary>
        /// Paints full control. Analyzes on demand, but meant to be called after analysis up front.
        /// </summary>
        public override void DoPaint(Graphics g)
        {
            // If size changed and we get a pain requested without having re-analized:
            // Analyze now. Not the best time here in paint, but must do.
            if (analyzedWidth != Width) Analyze(g, Width);

            // Background. Alternating at that!
            Color bgcol = odd ? Magic.ResultsAltBackColor : ZenParams.WindowColor;
            using (Brush b = new SolidBrush(bgcol))
            {
                g.FillRectangle(b, 0, 0, Width, Height);
            }

            // Hanzi highlights. May draw on top, so must come before actual characters are drawn.
            doPaintHanziHilites(g, bgcol);
            // Target text highlights (characters will come on top).
            doPaintTargetHilites(g, bgcol);

            // This is how we draw text
            StringFormat sf = StringFormat.GenericTypographic;
            // Headword, pinyin, entry body
            using (Brush bnorm = new SolidBrush(Color.Black))
            using (Brush bfade = new SolidBrush(Color.FromArgb(200, 200, 200)))
            using (Pen pnorm = new Pen(bnorm))
            {
                // This works best for Hanzi
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                // Simplified and traditional - headword
                foreach (HeadBlock hb in headInfo.SimpBlocks)
                {
                    PointF loc = new PointF(hb.Loc.X, hb.Loc.Y);
                    g.DrawString(hb.Char.ToString(), getFont(fntZhoHeadSimp), bnorm, loc, sf);
                }
                foreach (HeadBlock hb in headInfo.TradBlocks)
                {
                    PointF loc = new PointF(hb.Loc.X, hb.Loc.Y);
                    Brush b = hb.Faded ? bfade : bnorm;
                    g.DrawString(hb.Char.ToString(), getFont(fntZhoHeadTrad), b, loc, sf);
                }
                // Pinyin
                using (SolidBrush bhilite = new SolidBrush(Magic.HiliteColor))
                {
                    doPaintPinyin(g, bnorm, bhilite, sf, bgcol);
                }
                // Target (body)
                doPaintTarget(g, pnorm, bnorm, sf);
            }
        }
    }
}
