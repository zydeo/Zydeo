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
    }
}
