using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// One pinyin syllable, normalized into text and tone.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{GetDisplayString(true)}")]
    public partial class PinyinSyllable : IBinSerializable, IComparable<PinyinSyllable>
    {
        /// <summary>
        /// The syllable text, without tone. Can be a "weird" syllable, e.g., a Latin letter.
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// <para>The syllable's tone:</para>
        /// <para>-1: None (e.g., not specified in search input; or it's a "weird" syllable w/o tone.</para>
        /// <para>0: Neutral.</para>
        /// <para>1-4: Tones 1 through 4.</para>
        /// </summary>
        public readonly int Tone;

        /// <summary>
        /// Ctor: init immutable instance.
        /// </summary>
        public PinyinSyllable(string text, int tone)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentException("text");
            if (tone < -1 || tone > 4) throw new ArgumentException("tone");
            Text = text;
            Tone = tone;
        }

        /// <summary>
        /// Ctor: deserialize from binary stream.
        /// </summary>
        public PinyinSyllable(BinReader br)
        {
            Text = br.ReadString();
            byte b = br.ReadByte();
            Tone = ((int)b) - 1;
        }

        /// <summary>
        /// Serializes pinyin syllable into binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            bw.WriteString(Text);
            bw.WriteByte((byte)(Tone + 1));
        }

        /// <summary>
        /// Compares syllable to other, for lexicographical ordering.
        /// </summary>
        public int CompareTo(PinyinSyllable other)
        {
            // First, text without tone, case-insensitive
            int i = string.Compare(Text, other.Text, StringComparison.InvariantCultureIgnoreCase);
            if (i != 0) return i;
            // Seems identical: compare tone
            // Neutral tone is 0, so that comes first, as required
            i = Tone.CompareTo(other.Tone);
            if (i != 0) return i;
            // Still identical: case-sensitive comparison
            return string.Compare(Text, other.Text, StringComparison.InvariantCulture);
        }
    }
}
