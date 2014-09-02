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
        /// Paints all hilites in headword.
        /// </summary>
        private void doPaintHanziHilites(Graphics g, Color bgcol)
        {
            if (Res.HanziHiliteStart == -1)
                return;

            g.SmoothingMode = SmoothingMode.None;
            var si = HanziMeasure.Instance.GetMeasures(ZenParams.ZhoFontFamily, ZenParams.ZhoFontSize);
            HeadBlock hb;
            RectangleF rect;
            // Width of gradient
            float gradw = ideoSize.Width / 2.0F;
            // Extent of gradient outside character
            float gradext = 0;
            using (Brush b = new SolidBrush(ZenParams.HanziHiliteColor))
            {
                // In simplified
                if (headInfo.SimpBlocks.Count != 0)
                {
                    // Solid highlight on each character
                    for (int ix = Res.HanziHiliteStart; ix != Res.HanziHiliteStart + Res.HanziHiliteLength; ++ix)
                    {
                        hb = headInfo.SimpBlocks[ix];
                        rect = new RectangleF(hb.Loc.X, hb.Loc.Y, hb.Size.Width, si.RealRect.Height);
                        rect.X += (float)AbsLeft;
                        rect.Y += (float)AbsTop;
                        g.FillRectangle(b, rect);
                    }
                    // First and last chars get gradient on left and right
                    hb = headInfo.SimpBlocks[Res.HanziHiliteStart];
                    rect = new RectangleF(hb.Loc.X, hb.Loc.Y, gradw, si.RealRect.Height);
                    rect.X += (float)AbsLeft;
                    rect.X -= gradext;
                    rect.Y += (float)AbsTop;
                    using (LinearGradientBrush lgb = new LinearGradientBrush(rect, bgcol, ZenParams.HanziHiliteColor, LinearGradientMode.Horizontal))
                    {
                        g.FillRectangle(lgb, rect);
                    }
                    hb = headInfo.SimpBlocks[Res.HanziHiliteStart + Res.HanziHiliteLength - 1];
                    rect = new RectangleF(hb.Loc.X + hb.Size.Width, hb.Loc.Y, gradw, si.RealRect.Height);
                    rect.X += (float)AbsLeft;
                    rect.X += gradext;
                    rect.X -= gradw;
                    rect.Y += (float)AbsTop;
                    using (LinearGradientBrush lgb = new LinearGradientBrush(rect, ZenParams.HanziHiliteColor, bgcol, LinearGradientMode.Horizontal))
                    {
                        g.FillRectangle(lgb, rect);
                    }
                }
            }

            //using (Brush b = new SolidBrush(ZenParams.HanziHiliteColor))
            //{
            //    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //    // Only a single line, or both simplified and traditional are multi-line
            //    // Highlight each character individually
            //    if (headInfo.HeadMode != HeadMode.BothSingleLine)
            //    {
            //        // In simplified
            //        if (headInfo.SimpBlocks.Count != 0)
            //        {
            //            var si = HanziMeasure.Instance.GetMeasures(ZenParams.ZhoFontFamily, ZenParams.ZhoFontSize);
            //            for (int ix = Res.HanziHiliteStart; ix != Res.HanziHiliteStart + Res.HanziHiliteLength; ++ix)
            //            {
            //                HeadBlock hb = headInfo.SimpBlocks[ix];
            //                float hlHeight = 3.0F;
            //                hlHeight *= Scale;
            //                float hlTop = hb.Loc.Y + si.RealRect.Height;
            //                hlTop += ((float)AbsTop);
            //                RectangleF rect = new RectangleF(((float)AbsLeft) + hb.Loc.X, hlTop, hb.Size.Width, hlHeight);
            //                g.FillRectangle(b, rect);
            //            }
            //        }
            //        // In traditional
            //        if (headInfo.TradBlocks.Count != 0)
            //        {
            //            var si = HanziMeasure.Instance.GetMeasures(ZenParams.ZhoFontFamily, ZenParams.ZhoFontSize);
            //            for (int ix = Res.HanziHiliteStart; ix != Res.HanziHiliteStart + Res.HanziHiliteLength; ++ix)
            //            {
            //                HeadBlock hb = headInfo.TradBlocks[ix];
            //                float hlHeight = 3.0F;
            //                hlHeight *= Scale;
            //                float hlTop = hb.Loc.Y + si.RealRect.Height;
            //                hlTop += ((float)AbsTop);
            //                RectangleF rect = new RectangleF(((float)AbsLeft) + hb.Loc.X, hlTop, hb.Size.Width, hlHeight);
            //                g.FillRectangle(b, rect);
            //            }
            //        }
            //    }
            //}
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

            // Hanzi highlights. May draw on top, so must come before actual characters are drawn.
            doPaintHanziHilites(g, bgcol);

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
        }
    }
}
