using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.Common
{
    public class CedictResult
    {
        /// <summary>
        /// Represents unexpected Hanzi lookup results.
        /// </summary>
        public enum SimpTradWarning
        {
            /// <summary>
            /// No unexpected results.
            /// </summary>
            None,
            /// <summary>
            /// Traditional lookup was requested but result contains simplified characters.
            /// </summary>
            HasExtraSimp,
            /// <summary>
            /// Simplified lookup was requested but results contains traditional characters.
            /// </summary>
            HasExtraTrad,
        }

        /// <summary>
        /// Indicates if lookup returns simplified chars for a traditional lookup or the other way around.
        /// </summary>
        public readonly SimpTradWarning HanziWarning;

        /// <summary>
        /// The CEDICT entry.
        /// </summary>
        public readonly CedictEntry Entry;

        /// <summary>
        /// Start of search term in entry's headword (Hanzi), or -1.
        /// </summary>
        public readonly int HanziHiliteStart;

        /// <summary>
        /// Length of search term in entry's headword (hanzi), or 0.
        /// </summary>
        public readonly int HanziHiliteLength;

        /// <summary>
        /// Start of search term in entry's headword (pinyin syllables), or -1.
        /// </summary>
        public readonly int PinyinHiliteStart;

        /// <summary>
        /// Length of search term in entry's headword (pinyin syllables), or 0.
        /// </summary>
        public readonly int PinyinHiliteLength;

        /// <summary>
        /// Ctor: init immutable instance - result of hanzi lookup.
        /// </summary>
        public CedictResult(SimpTradWarning hanziWarning, CedictEntry entry,
            int hanziHiliteStart, int hanziHiliteLength)
        {
            HanziWarning = hanziWarning;
            Entry = entry;
            HanziHiliteStart = hanziHiliteStart;
            HanziHiliteLength = hanziHiliteLength;
            PinyinHiliteStart = -1;
            PinyinHiliteLength = 0;
        }

        /// <summary>
        /// Ctor: init immutable instance - result of pinyin lookup.
        /// </summary>
        public CedictResult(CedictEntry entry,
            int pinyinHiliteStart, int pinyinHiliteLength)
        {
            HanziWarning = SimpTradWarning.None;
            Entry = entry;
            HanziHiliteStart = -1;
            HanziHiliteLength = 01;
            PinyinHiliteStart = pinyinHiliteStart;
            PinyinHiliteLength = pinyinHiliteLength;
        }
    }
}
