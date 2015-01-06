using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZD.LexRank
{
    internal class WrkLexRank
    {
        private readonly StreamReader srHsk;
        private readonly StreamReader srWict;
        private readonly StreamReader srSubtle;

        public WrkLexRank(StreamReader srHsk, StreamReader srWict, StreamReader srSubtle)
        {
            this.srHsk = srHsk;
            this.srWict = srWict;
            this.srSubtle = srSubtle;
        }

        public void Work()
        {
            doReadHsk();
            doReadWict();
            doReadSubtle();
        }

        /// <summary>
        /// Info about a single headword (HSK level, ranks).
        /// </summary>
        private class HwInfo
        {
            public readonly string Word;
            public int HskLevel = 0;
            public int HskRank = 0;
            public int WictRank = 0;
            public int SubtleRank = 0;
            public int NormRank = 0;
            public HwInfo(string word)
            {
                Word = word;
            }
            public float CombRank
            {
                get
                {
                    // Not on either Wictionary or Subtle list: rank is HSK rank plus 50k
                    if (WictRank == 0 && SubtleRank == 0) return HskRank + 50000;
                    // If subtle rank is missing, assume 50k
                    int sr = SubtleRank == 0 ? 50000 : SubtleRank;
                    // If wict rank is missing, assume 10k
                    int wr = WictRank == 0 ? 10000 : WictRank;
                    // Combo rank is weighted average of subtle and wict ranks; subtle weight is double
                    float cr = 2 * sr;
                    cr += wr;
                    cr /= 3F;
                    return cr;
                }
            }
        }

        /// <summary>
        /// Info about each headword.
        /// </summary>
        private readonly Dictionary<string, HwInfo> hwInfos = new Dictionary<string, HwInfo>();

        private void doReadHsk()
        {
            string line;
            while ((line = srHsk.ReadLine()) != null)
            {
                string[] parts = line.Split(new char[] { '\t' });
                string word = parts[0];
                int level = int.Parse(parts[1]);
                int rank = int.Parse(parts[2]);
                HwInfo hwi = new HwInfo(word);
                hwi.HskLevel = level;
                hwi.HskRank = rank;
                hwInfos[word] = hwi;
            }
        }

        private void doReadWict()
        {
            string line;
            while ((line = srWict.ReadLine()) != null)
            {
                string[] parts = line.Split(new char[] { '\t' });
                string word = parts[0];
                int rank = int.Parse(parts[1]);
                HwInfo hwi;
                if (hwInfos.ContainsKey(word)) hwi = hwInfos[word];
                else
                {
                    hwi = new HwInfo(word);
                    hwInfos[word] = hwi;
                }
                hwi.WictRank = rank;
            }
        }

        private void doReadSubtle()
        {
            string line;
            while ((line = srSubtle.ReadLine()) != null)
            {
                string[] parts = line.Split(new char[] { '\t' });
                string word = parts[0];
                int rank = int.Parse(parts[1]);
                HwInfo hwi;
                if (hwInfos.ContainsKey(word)) hwi = hwInfos[word];
                else
                {
                    hwi = new HwInfo(word);
                    hwInfos[word] = hwi;
                }
                hwi.SubtleRank = rank;
            }
        }
        
        public void Finish(StreamWriter swOut)
        {
            // Sort whole bunch by combined rank
            List<HwInfo> sortedFull = new List<HwInfo>(hwInfos.Count);
            foreach (HwInfo hwi in hwInfos.Values) sortedFull.Add(hwi);
            sortedFull.Sort((x, y) => x.CombRank.CompareTo(y.CombRank));
            // What we keep: everything with an HSK level, plus best-ranking words to fill up 10k quota
            List<HwInfo> kept = new List<HwInfo>(10000);
            foreach (HwInfo hwi in sortedFull)
            {
                if (hwi.HskLevel != 0) kept.Add(hwi);
            }
            for (int i = 0; kept.Count != 10000; ++i)
            {
                HwInfo hwi = sortedFull[i];
                if (hwi.HskLevel != 0) continue;
                kept.Add(hwi);
            }
            // Now sort kept items
            kept.Sort((x, y) => x.CombRank.CompareTo(y.CombRank));
            // Inject normalized rank
            for (int i = 0; i != kept.Count; ++i) kept[i].NormRank = i + 1;
            // Reorder to keep prefixes in a single bunch, but in a stable way
            var preSorted = doPrefixSort(kept);
            // Write results
            doWriteResults(swOut, preSorted);
        }

        private class PrefixGroup
        {
            public readonly List<HwInfo> Items = new List<HwInfo>();
            public PrefixGroup(HwInfo highestRankItem)
            {
                Items.Add(highestRankItem);
            }
        }

        private static int pgComp(HwInfo a, HwInfo b)
        {
            int cmp = a.Word.Length.CompareTo(b.Word.Length);
            if (cmp != 0) return cmp;
            return a.NormRank.CompareTo(b.NormRank);
        }

        private static List<HwInfo> doPrefixSort(List<HwInfo> hwList)
        {
            HashSet<int> usedIndexes = new HashSet<int>();
            List<PrefixGroup> groups = new List<PrefixGroup>();
            for (int i = 0; i != hwList.Count; ++i)
            {
                if (usedIndexes.Contains(i)) continue;
                usedIndexes.Add(i);
                HwInfo hwiFirst = hwList[i];
                PrefixGroup group = new PrefixGroup(hwiFirst);
                groups.Add(group);
                // Collect all other words of which this one's a prefix, or which are prefixes of this one
                for (int j = i + 1; j < hwList.Count; ++j)
                {
                    if (usedIndexes.Contains(j)) continue;
                    HwInfo hwi = hwList[j];
                    if (hwiFirst.Word.StartsWith(hwi.Word) || hwi.Word.StartsWith(hwiFirst.Word))
                    {
                        usedIndexes.Add(j);
                        group.Items.Add(hwi);
                    }
                }
            }
            // Final, ordered results
            List<HwInfo> res = new List<HwInfo>(hwList.Count);
            foreach (PrefixGroup pg in groups)
            {
                pg.Items.Sort(pgComp);
                res.AddRange(pg.Items);
            }
            return res;
        }

        private void doWriteResults(StreamWriter swOut, List<HwInfo> res)
        {
            swOut.WriteLine("Rank\tWord\tHSKX\tRank-W\tRank-S\tRank-H\tCRANK");
            StringBuilder sb = new StringBuilder();
            foreach (HwInfo hwi in res)
            {
                sb.Clear();
                sb.Append(hwi.NormRank);
                sb.Append('\t');
                sb.Append(hwi.Word);
                sb.Append('\t');
                sb.Append(hwi.HskLevel == 0 ? "X" : hwi.HskLevel.ToString());
                sb.Append('\t');
                sb.Append(hwi.WictRank == 0 ? "X" : hwi.WictRank.ToString());
                sb.Append('\t');
                sb.Append(hwi.SubtleRank == 0 ? "X" : hwi.SubtleRank.ToString());
                sb.Append('\t');
                sb.Append(hwi.HskRank == 0 ? "X" : hwi.HskRank.ToString());
                sb.Append('\t');
                sb.Append(hwi.CombRank);
                swOut.WriteLine(sb.ToString());
            }
        }
    }
}
