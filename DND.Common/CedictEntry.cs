using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DND.Common
{
    /// <summary>
    /// One dictionary entry.
    /// </summary>
    public class CedictEntry : IBinSerializable
    {
        /// <summary>
        /// <para>The entry's pinyin syllables. Retroflexed "r5" is not a separate syllable.</para>
        /// </summary>
        private readonly CedictPinyinSyllable[] pinyin;

        /// <summary>
        /// The Chinese headword's senses in the target language.
        /// </summary>
        private readonly CedictSense[] senses;

        /// <summary>
        /// Headword in simplified script.
        /// </summary>
        public readonly string ChSimpl;

        /// <summary>
        /// Headword in traditional script.
        /// </summary>
        public readonly string ChTrad;

        /// <summary>
        /// Gets the headword's pinyin syllables.
        /// </summary>
        public ReadOnlyCollection<CedictPinyinSyllable> Pinyin
        {
            get { return new ReadOnlyCollection<CedictPinyinSyllable>(pinyin); }
        }

        /// <summary>
        /// Gets an enumerator of the entry's target-language senses.
        /// </summary>
        public IEnumerable<CedictSense> Senses
        {
            get { return senses; }
        }

        /// <summary>
        /// Compares headword's pinyin to other headword to get lexicographical ordering.
        /// </summary>
        public int PinyinCompare(CedictEntry other)
        {
            int length = Math.Min(pinyin.Length, other.pinyin.Length);
            // Compare syllable by syllable
            for (int i = 0; i != length; ++i)
            {
                int cmp = pinyin[i].CompareTo(other.pinyin[i]);
                if (cmp != 0) return cmp;
            }
            // If shorter is the prefix of longer, or the two are identical: shorter wins
            return pinyin.Length.CompareTo(other.pinyin.Length);
        }

        /// <summary>
        /// Ctor: init immutable instance.
        /// </summary>
        public CedictEntry(string chSimpl, string chTrad,
            ReadOnlyCollection<CedictPinyinSyllable> pinyin,
            ReadOnlyCollection<CedictSense> senses)
        {
            ChSimpl = chSimpl;
            ChTrad = chTrad;
            this.pinyin = new CedictPinyinSyllable[pinyin.Count];
            for (int i = 0; i != pinyin.Count; ++i) this.pinyin[i] = pinyin[i];
            this.senses = new CedictSense[senses.Count];
            for (int i = 0; i != senses.Count; ++i) this.senses[i] = senses[i];
        }

        /// <summary>
        /// Ctor: deserialize from binary stream.
        /// </summary>
        public CedictEntry(BinReader br)
        {
            pinyin = br.ReadArray(brr => new CedictPinyinSyllable(brr));
            ChSimpl = br.ReadString();
            ChTrad = br.ReadString();
            senses = br.ReadArray(brr => new CedictSense(brr));
        }

        /// <summary>
        /// Serialize to binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            bw.WriteArray(pinyin, (ps, bwr) => ps.Serialize(bwr));
            bw.WriteString(ChSimpl);
            bw.WriteString(ChTrad);
            bw.WriteArray(senses);
        }
    }
}
