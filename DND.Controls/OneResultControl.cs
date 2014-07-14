using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

using DND.Common;

namespace DND.Controls
{
    internal class OneResultControl : ZenControl
    {
        private const string zhoTestStr = "中中中中";
        private const string spaceTestStr = " ";
        private readonly int padTop;
        private readonly int padBottom;
        private readonly int padMid;
        private readonly int padRight;

        public int LastTop = int.MinValue;
        public readonly CedictResult Res;

        private static int zhoWidth = 0;
        private static float spaceWidth = 0;
        private static float lemmaLineHeight;

        private class MeasuredBlock
        {
            public SizeF Size;
            public PointF Loc;
            public bool StickRight;
            public string Str;
        }

        private int analyzedWidth = int.MinValue;
        private SizeF simpSize;
        private float simpTop;
        private float simpLeft;
        private SizeF tradSize;
        private float tradTop;
        private float tradLeft;
        private string strPinyin;
        private SizeF pinyinSize;
        //private PointF pinyinLoc;
        private float lemmaTop;
        private List<MeasuredBlock> lemmaBlocks;

        public OneResultControl(float scale, IZenControlOwner owner, CedictResult cr)
            : base(scale, owner)
        {
            this.Res = cr;
            padTop = (int)(5.0F * scale);
            padBottom = (int)(10.0F * scale);
            padMid = (int)(10.0F * scale);
            padRight = (int)(5.0F * scale);
        }

        // Graphics resource: static, singleton, never disposed
        private static Font fntZho;
        private static Font fntPinyin;
        private static Font fntLemma;

        static OneResultControl()
        {
            fntZho = new Font(ZenParams.ZhoFontFamily, ZenParams.ZhoFontSize);
            fntPinyin = new Font(ZenParams.PinyinFontFamily, ZenParams.PinyinFontSize, FontStyle.Bold);
            fntLemma = new Font(ZenParams.LemmaFontFamily, ZenParams.LemmaFontSize);
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
            using (Brush b = new SolidBrush(Color.Black))
            {
                g.DrawString(Res.Entry.ChSimpl, fntZho, b, simpLeft + (float)AbsLeft, simpTop + (float)AbsTop);
                g.DrawString(Res.Entry.ChTrad, fntZho, b, tradLeft + (float)AbsLeft, tradTop + (float)AbsTop);
                float rx = (float)AbsLeft + zhoWidth + (float)padMid;
                g.DrawString(strPinyin, fntPinyin, b, rx, padTop + (float)AbsTop);
                foreach (MeasuredBlock mb in lemmaBlocks)
                {
                    g.DrawString(mb.Str, fntLemma, b, mb.Loc.X + (float)AbsLeft, mb.Loc.Y + (float)AbsTop);
                }
            }
        }

        // Analyze layout with provided width; assume corresponding height; does not invalidate
        public void Analyze(Graphics g, int width)
        {
            if (analyzedWidth == width) return;
            analyzedWidth = Width;

            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            if (zhoWidth == 0)
                zhoWidth = (int)g.MeasureString(zhoTestStr, fntZho, 65535, sf).Width;
            if (spaceWidth == 0)
            {
                SizeF sz = g.MeasureString(spaceTestStr, fntLemma, 65535, sf);
                spaceWidth = (int)sz.Width;
                lemmaLineHeight = sz.Height;
            }

            simpSize = g.MeasureString(Res.Entry.ChSimpl, fntZho);
            simpLeft = (float)zhoWidth - simpSize.Width;
            simpTop = padTop;
            tradSize = g.MeasureString(Res.Entry.ChTrad, fntZho);
            tradLeft = (float)zhoWidth - tradSize.Width;
            tradTop = simpTop + simpSize.Height;
            float zhoHeight = tradTop + tradSize.Height + padBottom;

            strPinyin = "";
            foreach (string ps in Res.Entry.Pinyin)
            {
                if (strPinyin.Length > 0) strPinyin += " ";
                // TO-DO: convert tone numbers to accents here
                strPinyin += ps;
            }
            pinyinSize = g.MeasureString(strPinyin, fntPinyin, 65535, sf);

            lemmaTop = padTop + pinyinSize.Height;
            lemmaBlocks = new List<MeasuredBlock>();
            int meaningIdx = 0;
            float lemmaW = (float)width - zhoWidth - padMid - padRight;
            float lemmaL = zhoWidth + padMid;
            float blockX = lemmaL;
            float blockY = lemmaTop;
            foreach (CedictMeaning cm in Res.Entry.Meanings)
            {
                ++meaningIdx;
                string meaningIdxStr = "(" + meaningIdx.ToString() + ")";
                MeasuredBlock mbIdx = new MeasuredBlock
                {
                    Size = g.MeasureString(meaningIdxStr, fntLemma),
                    Loc = new PointF(blockX, blockY),
                    StickRight = true,
                    Str = meaningIdxStr
                };
                if (mbIdx.Loc.X + mbIdx.Size.Width - lemmaL > lemmaW)
                {
                    blockY += lemmaLineHeight;
                    blockX = lemmaL;
                    mbIdx.Loc = new PointF(blockX, blockY);
                }
                lemmaBlocks.Add(mbIdx);
                blockX += mbIdx.Size.Width + spaceWidth;
                string[] parts = cm.Equiv.Split(new char[] { ' ' });
                foreach (string wd in parts)
                {
                    MeasuredBlock mbWd = new MeasuredBlock
                    {
                        Size = g.MeasureString(wd, fntLemma),
                        Loc = new PointF(blockX, blockY),
                        StickRight = false,
                        Str = wd
                    };
                    if (mbWd.Loc.X + mbWd.Size.Width - lemmaL > lemmaW)
                    {
                        blockY += lemmaLineHeight;
                        blockX = lemmaL;
                        mbWd.Loc = new PointF(blockX, blockY);
                    }
                    lemmaBlocks.Add(mbWd);
                    blockX += mbWd.Size.Width + spaceWidth;
                }
            }
            float entryHeight = blockY + lemmaLineHeight + padBottom;
            float trueHeight = Math.Max(entryHeight, zhoHeight);
            
            SetSizeNoInvalidate(new Size(width, (int)trueHeight));
        }
    }
}
