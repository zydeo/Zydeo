using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ZD.Common;

namespace ZD.CedictEngine
{
    /// <summary>
    /// Implements <see cref="ZD.Common.IHeadwordInfo"/>.
    /// </summary>
    public class HeadwordInfo : IHeadwordInfo
    {
        /// <summary>
        /// Data file name; will keep opening at every query.
        /// </summary>
        private readonly string dataFileName;

        /// <summary>
        /// File position for each known character, or 0.
        /// </summary>
        private readonly int[] chrPoss = new int[65335];

        /// <summary>
        /// Pointers to chain starts for a given simplified headword hash.
        /// </summary>
        private struct HashChainPointer
        {
            /// <summary>
            /// Hash of simplified headword.
            /// </summary>
            public readonly int Hash;
            /// <summary>
            /// Position of chain's first item in CEDICT, or 0.
            /// </summary>
            public readonly int CedictPos;
            /// <summary>
            /// Position of chain's first item in HanDeDict, or 0.
            /// </summary>
            public readonly int HanDeDictPos;
            /// <summary>
            /// Ctor: init immutable instance.
            /// </summary>
            public HashChainPointer(int hash, int cedictPost, int hanDeDictPost)
            {
                Hash = hash;
                CedictPos = cedictPost;
                HanDeDictPos = hanDeDictPost;
            }
            /// <summary>
            /// Ctor: init only hash (artifact for searching in sorted array).
            /// </summary>
            public HashChainPointer(int hash)
            {
                Hash = hash;
                CedictPos = 0;
                HanDeDictPos = 0;
            }
        }

        /// <summary>
        /// Pointers to first items in hash chains; sorted by hash.
        /// </summary>
        private readonly HashChainPointer[] hashPtrs;

        /// <summary>
        /// Ctor: init from compiled binary file.
        /// </summary>
        public HeadwordInfo(string dataFileName)
        {
            this.dataFileName = dataFileName;
            using (BinReader br = new BinReader(dataFileName))
            {
                // Start of dictionary index
                int dictStartPos = br.ReadInt();
                // Number of characters
                int chrCnt = br.ReadInt();
                // File pointer of each character's info
                for (int i = 0; i != chrCnt; ++i)
                {
                    short chrVal = br.ReadShort();
                    char chr = (char)chrVal;
                    int filePos = br.ReadInt();
                    chrPoss[(int)chr] = filePos;
                }
                // Read sorted list of hash chain pointers
                br.Position = dictStartPos;
                int hashCnt = br.ReadInt();
                hashPtrs = new HashChainPointer[hashCnt];
                for (int i = 0; i != hashCnt; ++i)
                {
                    int hash = br.ReadInt();
                    int cdp = br.ReadInt();
                    int hdp = br.ReadInt();
                    hashPtrs[i] = new HashChainPointer(hash, cdp, hdp);
                }
            }
        }

        /// <summary>
        /// Helper class for searching in sorted array.
        /// </summary>
        private class HashComp : IComparer<HashChainPointer>
        {
            public int Compare(HashChainPointer x, HashChainPointer y)
            {
                return x.Hash.CompareTo(y.Hash);
            }
        }

        /// <summary>
        /// See <see cref="ZD.Common.IHeadwordInfo.GetPossibleHeadwords"/>.
        /// </summary>
        public HeadwordSyll[][] GetPossibleHeadwords(string simp, bool unihanFilter)
        {
            int hash = CedictEntry.Hash(simp);
            // Do we have this hash?
            HashChainPointer hcp = new HashChainPointer(hash);
            int pos = Array.BinarySearch(hashPtrs, hcp, new HashComp());
            if (pos < 0 || hashPtrs[pos].CedictPos == 0) return new HeadwordSyll[0][];
            // Yes! Read all entries with this hash from chain; keep those where simplified really matches.
            List<HeadwordSyll[]> cdHeads = new List<HeadwordSyll[]>();
            using (BinReader br = new BinReader(dataFileName))
            {
                int binPos = hashPtrs[pos].CedictPos;
                while (binPos != 0)
                {
                    br.Position = binPos;
                    // Next in chain
                    binPos = br.ReadInt();
                    // Entry
                    CedictEntry entry = new CedictEntry(br);
                    // Only keep if simplified really is identical
                    // Could be a hash collision
                    if (entry.ChSimpl == simp) addHeadIfNew(cdHeads, entry, unihanFilter);
                }
            }
            if (cdHeads.Count == 0) return new HeadwordSyll[0][];
            return cdHeads.ToArray();
        }

        /// <summary>
        /// <para>Makes an array of headword syllables from entry's data.</para>
        /// <para>Result has same length as entry's headword.</para>
        /// </summary>
        private void addHeadIfNew(List<HeadwordSyll[]> cdHeads, CedictEntry entry, bool unihanFilter)
        {
            // The new headword: with pinyin lower-cased
            HeadwordSyll[] res = new HeadwordSyll[entry.ChSimpl.Length];
            for (int i = 0; i != res.Length; ++i)
            {
                string pyLower = entry.GetPinyinAt(i).GetDisplayString(true);
                // Do not lower-case single latin letter
                if (pyLower.Length == 1 && pyLower[0] >= 'A' && pyLower[0] <= 'Z')
                { /* NOP */ }
                else pyLower = pyLower.ToLowerInvariant();
                res[i] = new HeadwordSyll(entry.ChSimpl[i], entry.ChTrad[i], pyLower);
            }
            // Is it already on list?
            // Do traditional chars make sense?
            UniHanziInfo[] uhis = null;
            if (unihanFilter)
            {
                char[] simp = new char[entry.ChSimpl.Length];
                for (int i = 0; i != simp.Length; ++i) simp[i] = entry.ChSimpl[i];
                uhis = GetUnihanInfo(simp);
            }
            bool toSkip = false;
            foreach (HeadwordSyll[] x in cdHeads)
            {
                // Only add if new
                bool different = false;
                for (int i = 0; i != res.Length; ++i)
                {
                    if (x[i].Simp != res[i].Simp) { different = true; break; }
                    if (x[i].Trad != res[i].Trad) { different = true; break; }
                    if (x[i].Pinyin != res[i].Pinyin) { different = true; break; }
                }
                if (!different)
                {
                    toSkip = true;
                    break;
                }
            }
            // Drop those where traditional character is odd
            if (unihanFilter)
            {
                for (int i = 0; i != res.Length; ++i)
                {
                    if (uhis[i] == null) continue;
                    if (Array.IndexOf(uhis[i].TradVariants, res[i].Trad) < 0)
                    { toSkip = true; break; }
                }
            }
            // If traditionals chars are OK and HW is new, add
            if (!toSkip) cdHeads.Add(res);
        }

        /// <summary>
        /// See <see cref="ZD.Common.IHeadwordInfo.GetUnihanInfo"/>.
        /// </summary>
        public UniHanziInfo[] GetUnihanInfo(string str)
        {
            char[] charr = new char[str.Length];
            for (int i = 0; i != str.Length; ++i) charr[i] = str[i];
            return GetUnihanInfo(charr);
        }

        /// <summary>
        /// See <see cref="ZD.Common.IHeadwordInfo.GetUnihanInfo"/>.
        /// </summary>
        public UniHanziInfo[] GetUnihanInfo(char[] chars)
        {
            UniHanziInfo[] res = new UniHanziInfo[chars.Length];
            using (BinReader br = new BinReader(dataFileName))
            {
                for (int i = 0; i != chars.Length; ++i)
                {
                    char c = chars[i];
                    // Character is an upper-case letter or a digit: itself
                    if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z'))
                    {
                        PinyinSyllable syll = new PinyinSyllable(c.ToString(), -1);
                        UniHanziInfo uhi = new UniHanziInfo(true, new char[] { c }, new PinyinSyllable[] { syll });
                        res[i] = uhi;
                    }
                    // Get genuine Hanzi info, if present
                    else
                    {
                        int pos = chrPoss[(int)c];
                        if (pos == 0) continue;
                        br.Position = pos;
                        res[i] = new UniHanziInfo(br);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// See <see cref="IHeadwordInfo.GetEntries"/>.
        /// </summary>
        public void GetEntries(string simp, out CedictEntry[] ced, out CedictEntry[] hdd)
        {
            List<CedictEntry> cedList = new List<CedictEntry>();
            List<CedictEntry> hddList = new List<CedictEntry>();
            int hash = CedictEntry.Hash(simp);
            // Do we have this hash?
            HashChainPointer hcp = new HashChainPointer(hash);
            int pos = Array.BinarySearch(hashPtrs, hcp, new HashComp());
            using (BinReader br = new BinReader(dataFileName))
            {
                // CEDICT entries
                if (pos >= 0 && hashPtrs[pos].CedictPos != 0)
                {
                    int binPos = hashPtrs[pos].CedictPos;
                    while (binPos != 0)
                    {
                        br.Position = binPos;
                        // Next in chain
                        binPos = br.ReadInt();
                        // Entry
                        CedictEntry entry = new CedictEntry(br);
                        // Only keep if simplified really is identical
                        // Could be a hash collision
                        if (entry.ChSimpl == simp) cedList.Add(entry);
                    }
                }
                // HanDeDict entries
                if (pos >= 0 && hashPtrs[pos].HanDeDictPos != 0)
                {
                    int binPos = hashPtrs[pos].HanDeDictPos;
                    while (binPos != 0)
                    {
                        br.Position = binPos;
                        // Next in chain
                        binPos = br.ReadInt();
                        // Entry
                        CedictEntry entry = new CedictEntry(br);
                        // Only keep if simplified really is identical
                        // Could be a hash collision
                        if (entry.ChSimpl == simp) hddList.Add(entry);
                    }
                }
            }
            // Our results
            ced = cedList.ToArray();
            hdd = hddList.ToArray();
        }

        /// <summary>
        /// See <see cref="IHeadwordInfo.ParseFromText"/>.
        /// </summary>
        public CedictEntry ParseFromText(string line)
        {
            return CedictCompiler.ParseEntry(line, 0, null, null);
        }
    }
}
