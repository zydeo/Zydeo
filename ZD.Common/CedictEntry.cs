using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// One dictionary entry.
    /// </summary>
    public class CedictEntry : IBinSerializable
    {
        /// <summary>
        /// <para>The entry's pinyin syllables. Retroflexed "r5" is not a separate syllable.</para>
        /// </summary>
        private readonly PinyinSyllable[] pinyin;

        /// <summary>
        /// The Chinese headword's senses in the target language.
        /// </summary>
        private readonly CedictSense[] senses;

        /// <summary>
        /// For each Hanzi, contains index of corresponding pinyin syllable, or -1.
        /// </summary>
        private readonly short[] hanziPinyinMap;

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
        public ReadOnlyCollection<PinyinSyllable> Pinyin
        {
            get { return new ReadOnlyCollection<PinyinSyllable>(pinyin); }
        }

        /// <summary>
        /// Gets count of unnormalized ("raw") pinyin syllables.
        /// </summary>
        public int PinyinCount
        {
            get { return pinyin.Length; }
        }

        /// <summary>
        /// Gets hanzi-to-pinyin map: for each hanzi, contains index of corresponding Pinyin syllable, or -1.
        /// </summary>
        public ReadOnlyCollection<short> HanziPinyinMap
        {
            get { return new ReadOnlyCollection<short>(hanziPinyinMap); }
        }

        /// <summary>
        /// Returns (unnormalized, "raw") pinyin syllable at specific index.
        /// </summary>
        public PinyinSyllable GetPinyinAt(int pos)
        {
            return pinyin[pos];
        }

        /// <summary>
        /// Gets the entry's pinyin display string: normalized; may have fewer items than raw syllables.
        /// </summary>
        /// <param name="diacritics">If yes, adds diacritics for tone marks; otherwise, appends number.</param>
        /// <returns>String representation to show in UI.</returns>
        public ReadOnlyCollection<PinyinSyllable> GetPinyinForDisplay(bool diacritics)
        {
            int a, b;
            return GetPinyinForDisplay(diacritics, -1, 0, out a, out b);
        }

        /// <summary>
        /// <para>Gets the entry's pinyin display string: normalized; may have fewer items than raw syllables.</para>
        /// <para>Calculates highlights in transformed UI string.</para>
        /// </summary>
        /// <param name="diacritics">If yes, adds diacritics for tone marks; otherwise, appends number.</param>
        /// <param name="origHiliteStart">Start of pinyin highlight from result, or -1.</param>
        /// <param name="origHiliteLength">Length of pinyin highlight from result, or 0.</param>
        /// <param name="hiliteStart">Start of pinyin hilight in returned collection, or -1.</param>
        /// <param name="hiliteLength">Length of pinyin hilight in returned collection, or 0.</param>
        /// <returns>String representation to show in UI.</returns>
        public ReadOnlyCollection<PinyinSyllable> GetPinyinForDisplay(bool diacritics,
            int origHiliteStart, int origHiliteLength,
            out int hiliteStart, out int hiliteLength)
        {
            // If pinyin does not have an "r5", no transformation needed
            if (Array.FindIndex(pinyin, x => x.Text == "r" && x.Tone == 0) == -1)
            {
                hiliteStart = origHiliteStart;
                hiliteLength = origHiliteLength;
                return Pinyin;
            }
            // Create new array where we merge "r" into previous syllable
            // Map decomposed positions to merged positions
            int[] posMap = new int[pinyin.Length];
            for (int i = 0; i != posMap.Length; ++i) posMap[i] = i;
            List<PinyinSyllable> res = new List<PinyinSyllable>(pinyin);
            int mi = 0;
            for (int i = 0; i < res.Count; ++i, ++mi)
            {
                PinyinSyllable ps = res[i];
                if (i >= 0 && ps.Text == "r" && ps.Tone == 0)
                {
                    res[i - 1] = new PinyinSyllable(res[i - 1].Text + "r", res[i - 1].Tone);
                    res.RemoveAt(i);
                    for (int j = mi; j != posMap.Length; ++j) --posMap[j];
                }
            }
            // Done.
            if (origHiliteStart == -1){ hiliteStart = -1; hiliteLength = 0; }
            else
            {
                hiliteStart = posMap[origHiliteStart];
                int hiliteEnd = origHiliteStart + origHiliteLength - 1;
                hiliteEnd = posMap[hiliteEnd];
                hiliteLength = hiliteEnd - hiliteStart + 1;
            }
            return new ReadOnlyCollection<PinyinSyllable>(res);
        }

        /// <summary>
        /// Gets an enumerator of the entry's target-language senses.
        /// </summary>
        public IEnumerable<CedictSense> Senses
        {
            get { return senses; }
        }

        /// <summary>
        /// Gets number of senses in entry.
        /// </summary>
        public int SenseCount
        {
            get { return senses.Length; }
        }

        /// <summary>
        /// Gets sense at provided index.
        /// </summary>
        /// <param name="ix"></param>
        /// <returns></returns>
        public CedictSense GetSenseAt(int ix)
        {
            return senses[ix];
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
            ReadOnlyCollection<PinyinSyllable> pinyin,
            ReadOnlyCollection<CedictSense> senses,
            short[] hanziPinyinMap)
        {
            if (chSimpl.Length != chTrad.Length)
                throw new ArgumentException("Different number of simplified and traditional hanzi.");

            ChSimpl = chSimpl;
            ChTrad = chTrad;
            this.pinyin = new PinyinSyllable[pinyin.Count];
            for (int i = 0; i != pinyin.Count; ++i) this.pinyin[i] = pinyin[i];
            this.senses = new CedictSense[senses.Count];
            for (int i = 0; i != senses.Count; ++i) this.senses[i] = senses[i];
            if (hanziPinyinMap != null)
            {
                if (hanziPinyinMap.Length != ChSimpl.Length)
                    throw new ArgumentException("hanziPinyinMap.Length does not equal number of hanzi.");
                this.hanziPinyinMap = hanziPinyinMap;
            }
            else
            {
                this.hanziPinyinMap = new short[ChSimpl.Length];
                for (int i = 0; i != this.hanziPinyinMap.Length; ++i)
                    this.hanziPinyinMap[i] = -1;
            }
        }

        /// <summary>
        /// Gets entry in the CEDICT format.
        /// </summary>
        /// <param name="head"></param>
        /// <param name="trg"></param>
        public void GetCedict(out string head, out string trg)
        {
            StringBuilder sbHead = new StringBuilder();
            StringBuilder sbTrg = new StringBuilder();
            sbHead.Append(ChTrad);
            sbHead.Append(' ');
            sbHead.Append(ChSimpl);
            sbHead.Append(" [");
            bool first = true;
            foreach (PinyinSyllable syll in pinyin)
            {
                if (!first) sbHead.Append(' ');
                else first = false;
                sbHead.Append(syll.GetDisplayString(false));
            }
            sbHead.Append(']');
            sbTrg.Append('/');
            foreach (CedictSense sense in senses)
            {
                sbTrg.Append(sense.GetCedict());
                sbTrg.Append('/');
            }
            head = sbHead.ToString();
            trg = sbTrg.ToString();
        }

        /// <summary>
        /// Stable hash of a headword string (simplified or traditional, whatever).
        /// </summary>
        public static int Hash(string headword)
        {
            int hash = 5381;
            foreach (char c in headword)
            {
                int i = (int)c;
                hash = ((hash << 5) + hash) + c;
            }
            return hash;
        }
        
        /// <summary>
        /// Deserializes simplified and traditional headword, without reading *full* entry.
        /// </summary>
        public static void DeserializeHanzi(BinReader br, out string simp, out string trad)
        {
            // Pinyin - will drop this
            br.ReadArray(brr => new PinyinSyllable(brr));
            // Headword simp & trad
            simp = br.ReadString();
            trad = br.ReadString();
        }

        /// <summary>
        /// Ctor: deserialize from binary stream.
        /// </summary>
        public CedictEntry(BinReader br)
        {
            pinyin = br.ReadArray(brr => new PinyinSyllable(brr));
            ChSimpl = br.ReadString();
            ChTrad = br.ReadString();
            senses = br.ReadArray(brr => new CedictSense(brr));
            hanziPinyinMap = br.ReadArray(brr => brr.ReadShort());
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
            bw.WriteArray(hanziPinyinMap, (x, bwr) => bwr.WriteShort(x));
        }
    }
}
