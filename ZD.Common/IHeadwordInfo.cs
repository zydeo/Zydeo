using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// <para>Information about a Hanzi from the Unihan database.</para>
    /// <para>Used in CHDICT's interactive "New entry" editor.</para>
    /// </summary>
    public class UniHanziInfo : IBinSerializable
    {
        /// <summary>
        /// True if character can be used in Simplified Chinese.
        /// </summary>
        public readonly bool CanBeSimp;

        /// <summary>
        /// <para>If character can be used as simplified, a list of its traditional variants.</para>
        /// <para>List may contain character itself, e.g., if traditional and simplified forms are identical.</para>
        /// <para>If character cannot be used as simplified, list is empty.</para>
        /// </summary>
        public readonly char[] TradVariants;

        /// <summary>
        /// Character's pinyin readings; more frequent first.
        /// </summary>
        public readonly PinyinSyllable[] Pinyin;

        /// <summary>
        /// Ctor: init from data.
        /// </summary>
        public UniHanziInfo(bool canBeSimp, char[] tradVariants, PinyinSyllable[] pinyin)
        {
            CanBeSimp = canBeSimp;
            if (tradVariants.Length > 127) throw new ArgumentException("Maximum allowed number of traditional variants is 127.");
            if (tradVariants.Length == 0) throw new ArgumentException("At least 1 traditional variant required; can be character itself.");
            TradVariants = new char[tradVariants.Length];
            for (int i = 0; i != TradVariants.Length; ++i) TradVariants[i] = tradVariants[i];
            if (pinyin.Length > 127) throw new ArgumentException("Maximum allowed number of Pinyin readings is 127.");
            if (pinyin.Length == 0) throw new ArgumentException("At least one Pinyin reading required.");
            if (Array.IndexOf(pinyin, null) != -1) throw new ArgumentException("No Pinyin syllable must be null.");
            Pinyin = new PinyinSyllable[pinyin.Length];
            for (int i = 0; i != Pinyin.Length; ++i) Pinyin[i] = pinyin[i];
        }

        /// <summary>
        /// Ctor: serialize from binary.
        /// </summary>
        public UniHanziInfo(BinReader br)
        {
            byte b = br.ReadByte();
            if (b == 0) CanBeSimp = false;
            else CanBeSimp = true;
            b = br.ReadByte();
            TradVariants = new char[b];
            for (byte i = 0; i != b; ++i) TradVariants[i] = br.ReadChar();
            b = br.ReadByte();
            Pinyin = new PinyinSyllable[b];
            for (byte i = 0; i != b; ++i) Pinyin[i] = new PinyinSyllable(br);
        }

        /// <summary>
        /// Serialize to binary.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            if (CanBeSimp) bw.WriteByte(1);
            else bw.WriteByte(0);
            bw.WriteByte((byte)TradVariants.Length);
            foreach (char c in TradVariants) bw.WriteChar(c);
            bw.WriteByte((byte)Pinyin.Length);
            foreach (PinyinSyllable syll in Pinyin) syll.Serialize(bw);
        }
    }

    /// <summary>
    /// One syllable in headword: simplified, traditional and pinyin.
    /// </summary>
    public class HeadwordSyll
    {
        /// <summary>
        /// Simplified Hanzi.
        /// </summary>
        public readonly char Simp;
        /// <summary>
        /// Traditional Hanzi.
        /// </summary>
        public readonly char Trad;
        /// <summary>
        /// Pinyin.
        /// </summary>
        public readonly string Pinyin;
        /// <summary>
        /// Ctor: init immutable instance.
        /// </summary>
        public HeadwordSyll(char simp, char trad, string pinyin)
        {
            Simp = simp;
            Trad = trad;
            Pinyin = pinyin;
        }
    }

    /// <summary>
    /// <para>Provides Unihan information about Hanzi for CHDICT's interactive entry editor.</para>
    /// <para>Provides known headwords (from CEDICT) by simplified HW.</para>
    /// <para>Provides CEDICT and HanDeDict entries by simplified HW.</para>
    /// </summary>
    public interface IHeadwordInfo
    {
        /// <summary>
        /// <para>Returns information about a bunch of Hanzi.</para>
        /// <para>Accepts A-Z, 0-9. Returns null in array for other non-ideographs, and for unknown Hanzi.</para>
        /// </summary>
        UniHanziInfo[] GetUnihanInfo(char[] c);

        /// <summary>
        /// <para>Returns information about a bunch of Hanzi.</para>
        /// <para>Accepts A-Z, 0-9. Returns null in array for other non-ideographs, and for unknown Hanzi.</para>
        /// </summary>
        UniHanziInfo[] GetUnihanInfo(string str);

        /// <summary>
        /// Gets headwords known in CEDICT from simplified string.
        /// </summary>
        /// <param name="simp">Simplified input to look up.</param>
        /// <param name="unihanFilter">If true, headwords are dropped where Unihan doesn't list traditional as a variant.</param>
        /// <returns>First dimension: one know CEDICT headword. Second dimension: equal length as input.</returns>
        HeadwordSyll[][] GetPossibleHeadwords(string simp, bool unihanFilter);

        /// <summary>
        /// Get all CEDICT and HanDeDict entries for provided simplified headword.
        /// </summary>
        void GetEntries(string simp, out CedictEntry[] ced, out CedictEntry[] hdd);

        /// <summary>
        /// Parse a line in the CEDICT format into an entry.
        /// </summary>
        CedictEntry ParseFromText(string line);
    }
}
