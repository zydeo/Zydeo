using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DND.Gui
{
    partial class OneResultControl
    {
        /// <summary>
        /// 
        /// </summary>
        private class Block
        {
            public SizeF Size;
            public bool StickRight;
        }

        private class TextBlock : Block
        {
            public string Text;
            public Font Font;
        }

        private class SenseIdBlock : Block
        {
            public int Idx;
            public string Text
            {
                get
                {
                    if (Idx >= 0 && Idx < 9) return (Idx + 1).ToString();
                    int resInt = (int)'a';
                    resInt += Idx - 9;
                    string res = "";
                    res += (char)resInt;
                    return res;
                }
            }
        }

        private class PositionedBlock
        {
            public Block Block;
            public PointF Loc;
        }

        private class HeadBlock
        {
            public string Char;
            public PointF Loc;
            public SizeF Size;
            public bool Faded;
        }

        private enum HeadMode
        {
            OnlyTrad,
            OnlySimp,
            BothSingleLine,
            BothMultiLine,
        }

        private class HeadInfo
        {
            public float HeadwordRight;
            public float HeadwordBottom;
            public readonly List<HeadBlock> SimpBlocks = new List<HeadBlock>();
            public readonly List<HeadBlock> TradBlocks = new List<HeadBlock>();
            public HeadMode HeadMode;
        }

        private class PinyinInfo
        {
            public string PinyinDisplay;
            public SizeF PinyinSize;
        }
    }
}
