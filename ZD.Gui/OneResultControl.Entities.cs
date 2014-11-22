using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

using ZD.Common;

namespace ZD.Gui
{
    partial class OneResultControl
    {
        /// <summary>
        /// Describes "hot" areas of senses in displayed content.
        /// </summary>
        private struct SenseArea
        {
            /// <summary>
            /// 2-byte position from left of target area; 2-byte width of this sense area.
            /// </summary>
            uint posLeftAndRight;

            /// <summary>
            /// 2-byte line number (index) in target area; 2-byte sense index in entry.
            /// </summary>
            uint lineIxAndSenseIx;

            /// <summary>
            /// Ctor: init from read data.
            /// </summary>
            public SenseArea(ushort lineIx, ushort left, ushort right, ushort senseIx)
            {
                posLeftAndRight = left;
                posLeftAndRight <<= 16;
                posLeftAndRight |= right;
                lineIxAndSenseIx = lineIx;
                lineIxAndSenseIx <<= 16;
                lineIxAndSenseIx |= senseIx;
            }

            /// <summary>
            /// Line index in which hot area is located. This determines vertical extent of hot zone.
            /// </summary>
            public int LineIx
            {
                get
                {
                    uint x = lineIxAndSenseIx;
                    x &= 0xffff0000;
                    x >>= 16;
                    return (int)x;
                }
            }

            /// <summary>
            /// Left of area, in control's coordinates.
            /// </summary>
            public int Left
            {
                get
                {
                    uint x = posLeftAndRight;
                    x &= 0xffff0000;
                    x >>= 16;
                    return (int)x;
                }
            }

            /// <summary>
            /// Right of area, in control's coordinates.
            /// </summary>
            public int Right
            {
                get { return (int)posLeftAndRight & 0xffff; }
            }

            public short SenseIx
            {
                get { return (short)(lineIxAndSenseIx & 0xffff); }
            }
        }

        /// <summary>
        /// <para>Represents one hyperlink in the control, made up of multiple blocks.</para>
        /// <para>Hyperlinks have active hover behavior, and trigger a new query when clicked.</para>
        /// </summary>
        private class LinkArea
        {
            /// <summary>
            /// The string to query when the link is clicked.
            /// </summary>
            public readonly string QueryString;
            /// <summary>
            /// The link's active areas (hover/click). Expressed in the control's relative coordinates.
            /// </summary>
            public readonly List<Rectangle> ActiveAreas = new List<Rectangle>();
            /// <summary>
            /// The blocks that make up the link area. Unchanged once blocks have been measured.
            /// </summary>
            public readonly HashSet<int> BlockIds = new HashSet<int>();
            /// <summary>
            /// <para>The positioned blocks that make up the link (they all change display state together on hover).</para>
            /// <para>Re-calculated on the basis of <see cref="Blocks"/> when recreating positioned blocks.</para>
            /// </summary>
            public readonly List<PositionedBlock> PositionedBlocks = new List<PositionedBlock>();

            /// <summary>
            /// Ctor: sets the link's query string based on available information.
            /// </summary>
            /// <param name="simp">Simplified Hanzi, or empty string.</param>
            /// <param name="trad">Traditional Hanzi, or empty string.</param>
            /// <param name="pinyin">Pinyin, or empty string.</param>
            /// <param name="script">Search script (to choose simp/trad Hanzi, if available).</param>
            public LinkArea(string simp, string trad, string pinyin, SearchScript script)
            {
                if (simp == string.Empty && trad != string.Empty) simp = trad;
                else if (trad == string.Empty && simp != string.Empty) trad = simp;
                // We have hanzi. Use that.
                if (simp != string.Empty)
                    QueryString = script == SearchScript.Traditional ? trad : simp;
                // No hanzi. Must have pinyin, use that.
                else
                    QueryString = pinyin;
            }
        }

        /// <summary>
        /// One measured text block in entry body (target text).
        /// </summary>
        private struct Block
        {
            /// <summary>
            /// Display text's position in text pool.
            /// </summary>
            public ushort TextPos;
            /// <summary>
            /// The block's width in pixels: rounded up to next integer
            /// </summary>
            public ushort Width;
            /// <summary>
            /// Index of this block's display font.
            /// </summary>
            public byte FontIdx;
            /// <summary>
            /// Compact representation of boolean flags. Do not access directly; use properties.
            /// </summary>
            public byte Flags;
            /// <summary>
            /// True if this block represents a sense ID.
            /// </summary>
            public bool SenseId
            {
                get { return (Flags & 1) == 1; }
                set { Flags &= (byte.MaxValue ^ 1); if (value) Flags |= 1; }
            }
            /// <summary>
            /// If true, must keep with block on the right (i.e., non-breaking space after me).
            /// </summary>
            public bool StickRight
            {
                get { return (Flags & 2) == 2; }
                set { Flags &= (byte.MaxValue ^ 2); if (value) Flags |= 2; }
            }
            /// <summary>
            /// If true, block must come at start of line, inducing line break
            /// </summary>
            public bool NewLine
            {
                get { return (Flags & 4) == 4; }
                set { Flags &= (byte.MaxValue ^ 4); if (value) Flags |= 4; }
            }
            /// <summary>
            /// True if block is followed by a space. If false, contiguous to next block, but can break.
            /// </summary>
            public bool SpaceAfter
            {
                get { return (Flags & 8) == 8; }
                set { Flags &= (byte.MaxValue ^ 8); if (value) Flags |= 8; }
            }
            /// <summary>
            /// True if block is to be highlighted (match).
            /// </summary>
            public bool Hilite
            {
                get { return (Flags & 16) == 16; }
                set { Flags &= (byte.MaxValue ^ 16); if (value) Flags |= 16; }
            }
            /// <summary>
            /// <para>True if block, of whatever kind, is first block of a Cedict sense.</para>
            /// <para>Can be true even if <see cref="SenseId"/> is false: if sense is classifier.</para>
            /// </summary>
            public bool FirstInCedictSense
            {
                get { return (Flags & 32) == 32; }
                set { Flags &= (byte.MaxValue ^ 32); if (value) Flags |= 32; }
            }
        }

        /// <summary>
        /// A positioned typographical block, ready for painting within the control's current width.
        /// </summary>
        private struct PositionedBlock
        {
            /// <summary>
            /// The block's X coordinate
            /// </summary>
            public short LocX;
            /// <summary>
            /// The block's Y coordinate
            /// </summary>
            public short LocY;
            /// <summary>
            /// The measured block - index in the array of measued blocks.
            /// </summary>
            public ushort BlockIdx;
        }

        /// <summary>
        /// One block, representing a single hanzi, in the headword.
        /// </summary>
        private class HeadBlock
        {
            /// <summary>
            /// The character's location in client coordinates.
            /// </summary>
            public PointF Loc;
            /// <summary>
            /// The character's side. Typically the standard ideographic rectangle, but can be smaller for Latn letters.
            /// </summary>
            public SizeF Size;
            /// <summary>
            /// The hanzi character.
            /// </summary>
            public char Char;
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
            /// Position of the syllable's display text in the text pool.
            /// </summary>
            public ushort TextPos;
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
