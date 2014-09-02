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
        /// Base class for an analyzed typographical block in entry body.
        /// </summary>
        private class Block
        {
            /// <summary>
            /// The block's size.
            /// </summary>
            public SizeF Size;
            /// <summary>
            /// If true, must keep with block on the right (i.e., non-breaking space after me).
            /// </summary>
            public bool StickRight;
        }

        /// <summary>
        /// A single "word," i.e., a sequence of characters and punctuation
        /// </summary>
        private class TextBlock : Block
        {
            /// <summary>
            /// The text to display.
            /// </summary>
            public string Text;
            /// <summary>
            /// The display font to use.
            /// </summary>
            public Font Font;
        }

        /// <summary>
        /// A block representing a sense ID, i.e., a circled number or letter.
        /// </summary>
        private class SenseIdBlock : Block
        {
            /// <summary>
            /// The index (number) of this sense.
            /// </summary>
            public int Idx;

            /// <summary>
            /// Gets the text to show. Idx + 1 up to 8, then a through z.
            /// </summary>
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

        /// <summary>
        /// A positioned typographical block, ready for painting within the control's current width.
        /// </summary>
        private class PositionedBlock
        {
            /// <summary>
            /// The measured block.
            /// </summary>
            public Block Block;
            /// <summary>
            /// The block's location in client coordinates.
            /// </summary>
            public PointF Loc;
        }

        /// <summary>
        /// One block, representing a single hanzi, in the headword.
        /// </summary>
        private class HeadBlock
        {
            /// <summary>
            /// The hanzi character.
            /// </summary>
            public string Char;
            /// <summary>
            /// The character's location in client coordinates.
            /// </summary>
            public PointF Loc;
            /// <summary>
            /// The character's side. Typically the standard ideographic rectangle, but can be smaller for Latn letters.
            /// </summary>
            public SizeF Size;
            /// <summary>
            /// If true, it's a traditional character identical to the simplified form: to be displayed in grey.
            /// </summary>
            public bool Faded;
        }

        /// <summary>
        /// Headword layouts depending on content and displayed scripts. Affects the way highlights are shown.
        /// </summary>
        private enum HeadMode
        {
            /// <summary>
            /// Only traditional characters.
            /// </summary>
            OnlyTrad,
            /// <summary>
            /// Only simplified characters.
            /// </summary>
            OnlySimp,
            /// <summary>
            /// Traditional plus simplified, both on a single line.
            /// </summary>
            BothSingleLine,
            /// <summary>
            /// Traditional plus simplified, with a line break inside.
            /// </summary>
            BothMultiLine,
        }

        /// <summary>
        /// Layout info about the headword area.
        /// </summary>
        private class HeadInfo
        {
            /// <summary>
            /// Right edge of the headword area.
            /// </summary>
            public float HeadwordRight;
            /// <summary>
            /// Bottom of the headword area.
            /// </summary>
            public float HeadwordBottom;
            /// <summary>
            /// Typographical blocks for the headword in simplified characters.
            /// </summary>
            public readonly List<HeadBlock> SimpBlocks = new List<HeadBlock>();
            /// <summary>
            /// Typographical blocks for the headword in traditional characters.
            /// </summary>
            public readonly List<HeadBlock> TradBlocks = new List<HeadBlock>();
            /// <summary>
            /// This headword's layout. See the <see cref="HeadMode"/> enum.
            /// </summary>
            public HeadMode HeadMode;
        }

        /// <summary>
        /// One pinyin syllable: rectangle and display text.
        /// </summary>
        private class PinyinBlock
        {
            /// <summary>
            /// The rectangle occupied by the syllable.
            /// </summary>
            public RectangleF Rect;
            /// <summary>
            /// The syllable's display text.
            /// </summary>
            public string Text;
        }

        /// <summary>
        /// Information about the entry's pinyin text.
        /// </summary>
        private class PinyinInfo
        {
            /// <summary>
            /// A list of each displayed syllable.
            /// </summary>
            public readonly List<PinyinBlock> Blocks = new List<PinyinBlock>();
            /// <summary>
            /// The height of the entire pinyin text.
            /// </summary>
            public float PinyinHeight;
            /// <summary>
            /// First block with pinyin highlight, or -1.
            /// </summary>
            public int HiliteStart;
            /// <summary>
            /// Number of highlighted pinyin blocks, or 0.
            /// </summary>
            public int HiliteLength;
        }
    }
}
