using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// See <see cref="TargetHilites"/>.
        /// </summary>
        private readonly CedictTargetHighlight[] targetHilites;

        /// <summary>
        /// Highlights in target text. Never null; can be empty.
        /// </summary>
        public IEnumerable<CedictTargetHighlight> TargetHilites
        {
            get { return targetHilites; }
        }

        /// <summary>
        /// Ctor: init immutable instance - result of target lookup.
        /// </summary>
        public CedictResult(CedictEntry entry, ReadOnlyCollection<CedictTargetHighlight> targetHilites)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            if (targetHilites == null) throw new ArgumentNullException("targetHilites");

            this.targetHilites = new CedictTargetHighlight[targetHilites.Count];
            for (int i = 0; i != targetHilites.Count; ++i)
            {
                if (targetHilites[i] == null) throw new ArgumentException("Null element in highlights array.");
                this.targetHilites[i] = targetHilites[i];
            }
            HanziWarning = SimpTradWarning.None;
            Entry = entry;
            HanziHiliteStart = -1;
            HanziHiliteLength = 0;
            PinyinHiliteStart = -1;
            PinyinHiliteLength = 0;
        }

        /// <summary>
        /// Ctor: init immutable instance - result of hanzi lookup.
        /// </summary>
        public CedictResult(SimpTradWarning hanziWarning, CedictEntry entry,
            int hanziHiliteStart, int hanziHiliteLength)
        {
            if (entry == null) throw new ArgumentNullException("entry");

            targetHilites = new CedictTargetHighlight[0];
            HanziWarning = hanziWarning;
            Entry = entry;
            HanziHiliteStart = hanziHiliteStart;
            HanziHiliteLength = hanziHiliteLength;
            calculatePinyinHighlights(out PinyinHiliteStart, out PinyinHiliteLength);
        }

        /// <summary>
        /// Ctor: init immutable instance - result of pinyin lookup.
        /// </summary>
        public CedictResult(CedictEntry entry,
            int pinyinHiliteStart, int pinyinHiliteLength)
        {
            if (entry == null) throw new ArgumentNullException("entry");

            targetHilites = new CedictTargetHighlight[0];
            HanziWarning = SimpTradWarning.None;
            Entry = entry;
            PinyinHiliteStart = pinyinHiliteStart;
            PinyinHiliteLength = pinyinHiliteLength;
            calculateHanziHighlights(out HanziHiliteStart, out HanziHiliteLength);
        }

        /// <summary>
        /// Calculates hanzi highlights from pinyin highlights.
        /// </summary>
        private void calculateHanziHighlights(out int hhStart, out int hhLength)
        {
            // No pinyin highlights either.
            if (PinyinHiliteStart == -1)
            {
                hhStart = -1;
                hhLength = 0;
                return;
            }
            int first = -1;
            int lastResolved = -1;
            ReadOnlyCollection<short> map = Entry.HanziPinyinMap;
            for (int i = 0; i != map.Count; ++i)
            {
                int pix = map[i];
                // Current hanzi does not resolve - nothing to do
                if (pix == -1) continue;
                // Hanzi resolves into a pinyin that's outside our pinyin highlight range
                if (pix < PinyinHiliteStart || pix >= PinyinHiliteStart + PinyinHiliteLength)
                    continue;
                // First hanzi we've found?
                if (first == -1) first = i;
                // Also the last one so far
                lastResolved = i;
            }
            // Could not resolve anything
            if (first == -1)
            {
                hhStart = -1;
                hhLength = 0;
                return;
            }
            // All good
            hhStart = first;
            hhLength = lastResolved - first + 1;
        }

        /// <summary>
        /// Calculates pinyin highlights from hanzi highlights.
        /// </summary>
        private void calculatePinyinHighlights(out int phStart, out int phLength)
        {
            // No hanzi highlights either.
            if (HanziHiliteStart == -1)
            {
                phStart = -1;
                phLength = 0;
                return;
            }
            int first = -1;
            int lastResolved = -1;
            ReadOnlyCollection<short> map = Entry.HanziPinyinMap;
            for (int i = HanziHiliteStart; i != HanziHiliteStart + HanziHiliteLength; ++i)
            {
                int pix = map[i];
                // Current hanzi does not resolve - nothing to do
                if (pix == -1) continue;
                // First one we can resolve
                if (first == -1) first = pix;
                // Also the last one so far
                lastResolved = pix;
            }
            // Could not resolve anything
            if (first == -1)
            {
                phStart = -1;
                phLength = 0;
                return;
            }
            // All good
            phStart = first;
            phLength = lastResolved - first + 1;
        }
    }
}
