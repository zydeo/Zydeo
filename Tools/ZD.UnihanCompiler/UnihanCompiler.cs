using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using ZD.Common;
using ZD.CedictEngine;

namespace ZD.UnihanCompiler
{
    internal class UnihanCompiler
    {
        private class CharInfo
        {
            public char[] SimpVars;
            public char[] TradVars;
            public string[] Pinlu;
            public string[] Pinyin;
            public string Mandarin;
            public string[] XHC;
            public int FilePos;
        }

        private Dictionary<char, CharInfo> infos = new Dictionary<char, CharInfo>();
        
        private CharInfo getOrMake(char c)
        {
            if (infos.ContainsKey(c)) return infos[c];
            CharInfo ci = new CharInfo();
            infos[c] = ci;
            return ci;
        }

        public void ReadingLine(string line)
        {
            if (line.StartsWith("#") || line == "") return;
            string[] parts = line.Split('\t');
            // Over the 64k pane: ignore
            string cpoint = parts[0];
            if (cpoint.Length > 6) return;
            cpoint = cpoint.Substring(2);
            int cval = int.Parse(cpoint, System.Globalization.NumberStyles.HexNumber, null);
            char c = (char)cval;
            // The fields we care about
            string field = parts[1];
            if (field == "kHanyuPinlu") doHanyuPinlu(c, parts[2]);
            else if (field == "kHanyuPinyin") doHanyuPinyin(c, parts[2]);
            else if (field == "kMandarin") doMandarin(c, parts[2]);
            else if (field == "kXHC1983") doXHC1983(c, parts[2]);
        }

        private static Regex rePinlu = new Regex(@"([^\(]+)\(");

        private void doHanyuPinlu(char c, string str)
        {
            CharInfo ci = getOrMake(c);
            string[] parts = str.Split(' ');
            ci.Pinlu = new string[parts.Length];
            for (int i = 0; i != parts.Length; ++i)
            {
                Match m = rePinlu.Match(parts[i]);
                ci.Pinlu[i] = m.Groups[1].Value;
            }
        }

        private void doHanyuPinyin(char c, string str)
        {
            CharInfo ci = getOrMake(c);
            // For lines like this: 21244.040:nài,nì,nà 80020.040:nà,nài,nì
            // ...only care up to first space
            str = str.Split(' ')[0];
            string[] parts = str.Split(':')[1].Split(',');
            ci.Pinyin = new string[parts.Length];
            for (int i = 0; i != parts.Length; ++i)
                ci.Pinyin[i] = parts[i];
        }

        private void doMandarin(char c, string str)
        {
            CharInfo ci = getOrMake(c);
            ci.Mandarin = str;
        }

        private void doXHC1983(char c, string str)
        {
            CharInfo ci = getOrMake(c);
            string[] parts = str.Split(' ');
            ci.XHC = new string[parts.Length];
            for (int i = 0; i != parts.Length; ++i)
                ci.XHC[i] = parts[i].Split(':')[1];
        }

        public void PurgeReadingless()
        {
            List<char> torem = new List<char>();
            foreach (var x in infos)
            {
                CharInfo ci = x.Value;
                if (ci.Mandarin == null && ci.Pinlu == null && ci.Pinyin == null && ci.XHC == null)
                    torem.Add(x.Key);
            }
            foreach (char c in torem) infos.Remove(c);
        }

        public void VariantLine(string line)
        {
            if (line.StartsWith("#") || line == "") return;
            string[] parts = line.Split('\t');
            // Over the 64k pane: ignore
            string cpoint = parts[0];
            if (cpoint.Length > 6) return;
            cpoint = cpoint.Substring(2);
            int cval = int.Parse(cpoint, System.Globalization.NumberStyles.HexNumber, null);
            char c = (char)cval;
            // The fields we care about
            string field = parts[1];
            // Only traditional and simplified variants
            if (field != "kTraditionalVariant" && field != "kSimplifiedVariant") return;
            // Not seen in readings? Ignore.
            if (!infos.ContainsKey(c)) return;
            // Get list of chars
            string[] vars = parts[2].Split(' ');
            List<char> chars = new List<char>();
            foreach (string vcp in vars)
            {
                if (vcp.Length > 6) continue;
                int vval = int.Parse(vcp.Substring(2), System.Globalization.NumberStyles.HexNumber, null);
                chars.Add((char)vval);
            }
            // We may have dropped all variants as out-of-64k-pane: then, no variants.
            if (chars.Count == 0) return;
            // Remember in the correct field
            if (field == "kTraditionalVariant")
                infos[c].TradVars = chars.ToArray();
            else infos[c].SimpVars = chars.ToArray();
        }

        private class PyCalc
        {
            public string Py;
            public int Sum;
            public int Cnt;
            public float Avg { get { return ((float)Sum) / ((float)Cnt); } }
        }

        private UniHanziInfo getInfo(char c, CharInfo ci)
        {
            bool canBeSimp = false;
            List<char> tradVariants = new List<char>();

            // Character can be used as simplified or not
            // And its traditional variants
            // As per http://www.unicode.org/reports/tr38/index.html#SCTC from Unihan report
            // 1: Simp and trad forms identical
            if (ci.TradVars == null && ci.SimpVars == null)
            {
                canBeSimp = true;
                tradVariants.Add(c);
            }
            // 2: Only trad
            else if (ci.TradVars == null && ci.SimpVars != null)
            {
                canBeSimp = false;
                tradVariants.Add(c);
            }
            // 3: Only simp
            else if (ci.TradVars != null && ci.SimpVars == null)
            {
                canBeSimp = true;
                tradVariants.AddRange(ci.TradVars);
            }
            else
            {
                canBeSimp = true;
                // 4/1: Both; may remain or get mapped in traditional
                if (ci.TradVars.Contains(c))
                {
                    tradVariants.AddRange(ci.TradVars);
                }
                // 4/2: Both; different meaning
                else
                {
                    tradVariants.AddRange(ci.TradVars);
                }
            }

            List<string> pinyin = new List<string>();
            // Pinyin reading: use Mandarin only if no other source available
            // Otherwise, combine ranking of sources
            if (ci.Pinlu == null && ci.Pinyin == null && ci.XHC == null)
                pinyin.Add(ci.Mandarin);
            else
            {
                int max = 0;
                if (ci.Pinlu != null) max = ci.Pinlu.Length;
                if (ci.Pinyin != null && max < ci.Pinyin.Length) max = ci.Pinyin.Length;
                if (ci.XHC != null && max < ci.XHC.Length) max = ci.XHC.Length;
                Dictionary<string, PyCalc> cnts = new Dictionary<string, PyCalc>();
                if (ci.Pinlu != null)
                {
                    for (int i = 0; i != ci.Pinlu.Length; ++i)
                    {
                        PyCalc pyCalc;
                        if (cnts.ContainsKey(ci.Pinlu[i])) pyCalc = cnts[ci.Pinlu[i]];
                        else { pyCalc = new PyCalc(); pyCalc.Py = ci.Pinlu[i]; cnts[ci.Pinlu[i]] = pyCalc; }
                        pyCalc.Cnt++;
                        pyCalc.Sum += max - i;
                    }
                }
                if (ci.Pinyin != null)
                {
                    for (int i = 0; i != ci.Pinyin.Length; ++i)
                    {
                        PyCalc pyCalc;
                        if (cnts.ContainsKey(ci.Pinyin[i])) pyCalc = cnts[ci.Pinyin[i]];
                        else { pyCalc = new PyCalc(); pyCalc.Py = ci.Pinyin[i]; cnts[ci.Pinyin[i]] = pyCalc; }
                        pyCalc.Cnt++;
                        pyCalc.Sum += max - i;
                    }
                }
                if (ci.XHC != null)
                {
                    for (int i = 0; i != ci.XHC.Length; ++i)
                    {
                        PyCalc pyCalc;
                        if (cnts.ContainsKey(ci.XHC[i])) pyCalc = cnts[ci.XHC[i]];
                        else { pyCalc = new PyCalc(); pyCalc.Py = ci.XHC[i]; cnts[ci.XHC[i]] = pyCalc; }
                        pyCalc.Cnt++;
                        pyCalc.Sum += max - i;
                    }
                }
                List<PyCalc> lst = new List<PyCalc>();
                lst.AddRange(cnts.Values);
                lst.Sort((x, y) => y.Sum.CompareTo(x.Sum));
                foreach (var x in lst) pinyin.Add(x.Py);
            }

            // Convert to typed Pinyin syllables
            PinyinSyllable[] sylls = new PinyinSyllable[pinyin.Count];
            for (int i = 0; i != pinyin.Count; ++i) sylls[i] = PinyinSyllable.FromDisplayString(pinyin[i]);

            // Done.
            return new UniHanziInfo(canBeSimp, tradVariants.ToArray(), sylls);
        }

        public void WriteUnihanData(BinWriter bw)
        {
            // Start position of simplified hash array: we'll return here
            bw.WriteInt(0);
            // Number of characters
            bw.WriteInt((int)infos.Count);
            // File pointers for each char: we'll return here
            int pos = bw.Position;
            for (int i = 0; i != infos.Count; ++i)
            {
                bw.WriteShort(0);
                bw.WriteInt(0);
            }
            // Make character info for each character; serialize it; remember file position
            foreach (var x in infos)
            {
                x.Value.FilePos = bw.Position;
                UniHanziInfo uhi = getInfo(x.Key, x.Value);
                uhi.Serialize(bw);
            }
            // Remember end of file
            int endPos = bw.Position;
            // Go back to start of file, write file positions for each character.
            bw.Position = pos;
            foreach (var x in infos)
            {
                bw.WriteShort((short)x.Key);
                bw.WriteInt(x.Value.FilePos);
            }
            // Return to end of file
            bw.Position = endPos;
        }

        private int cedictDropped = 0;
        private int hddDropped = 0;

        private Dictionary<int, List<int>> cedictHashPoss = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> hddHashPoss = new Dictionary<int, List<int>>();

        public void DictLine(string line, bool cedict, BinWriter bw)
        {
            // Parse entry
            CedictEntry entry = CedictCompiler.ParseEntry(line);
            // Verify that simp, trad and pinyin are equal length
            if (entry != null)
                if (entry.ChSimpl.Length != entry.ChTrad.Length || entry.ChSimpl.Length != entry.PinyinCount)
                    entry = null;
            // Just count if failed to parse
            if (entry == null)
            {
                if (cedict) ++cedictDropped;
                else ++hddDropped;
                return;
            }
            // Serialize
            int fpos = bw.Position;
            // First: hash chain: next entry in file with same hash. Will fill later.
            bw.WriteInt(0);
            // Then, entry itself
            entry.Serialize(bw);
            // Hash simplified and remember file position
            int hash = CedictEntry.Hash(entry.ChSimpl);
            List<int> poss;
            Dictionary<int, List<int>> hashPoss = cedict ? cedictHashPoss : hddHashPoss;
            if (!hashPoss.ContainsKey(hash))
            {
                poss = new List<int>();
                hashPoss[hash] = poss;
            }
            else poss = hashPoss[hash];
            poss.Add(fpos);
        }

        private class PosPair
        {
            public int CedictPos = 0;
            public int HddPos = 0;
        }

        private class HashChainStarts
        {
            public int Hash;
            public int CedictPos;
            public int HddPos;
        }

        /// <summary>
        /// Finalize dictionary data.
        /// </summary>
        public void FinalizeDict(BinWriter bw)
        {
            // Combined items: hash > (CEDICT chain start; HDD chain start)
            Dictionary<int, PosPair> hashChains = new Dictionary<int, PosPair>();
            // Create hash chains in file
            foreach (var x in cedictHashPoss)
            {
                PosPair pp;
                if (!hashChains.ContainsKey(x.Key))
                {
                    pp = new PosPair();
                    hashChains[x.Key] = pp;
                }
                else pp = hashChains[x.Key];
                pp.CedictPos = x.Value[0];
                for (int i = x.Value.Count - 2; i >= 0; --i)
                {
                    bw.Position = x.Value[i];
                    bw.WriteInt(x.Value[i + 1]);
                }
            }
            foreach (var x in hddHashPoss)
            {
                PosPair pp;
                if (!hashChains.ContainsKey(x.Key))
                {
                    pp = new PosPair();
                    hashChains[x.Key] = pp;
                }
                else pp = hashChains[x.Key];
                pp.HddPos = x.Value[0];
                for (int i = x.Value.Count - 2; i >= 0; --i)
                {
                    bw.Position = x.Value[i];
                    bw.WriteInt(x.Value[i + 1]);
                }
            }
            // Create our sorted list of hash > (pos, pos)
            HashChainStarts[] lst = new HashChainStarts[hashChains.Count];
            int lstPos = 0;
            foreach (var x in hashChains)
            {
                lst[lstPos] = new HashChainStarts { Hash = x.Key, CedictPos = x.Value.CedictPos, HddPos = x.Value.HddPos };
                ++lstPos;
            }
            Array.Sort(lst, (x, y) => x.Hash.CompareTo(y.Hash));
            // We'll append this list to end of file
            // First, update position at the very start of file
            bw.MoveToEnd();
            int filePos = bw.Position;
            bw.Position = 0;
            bw.WriteInt(filePos);
            bw.MoveToEnd();
            // Element count
            bw.WriteInt(lst.Length);
            // Each element
            foreach (HashChainStarts hcs in lst)
            {
                bw.WriteInt(hcs.Hash);
                bw.WriteInt(hcs.CedictPos);
                bw.WriteInt(hcs.HddPos);
            }
        }
    }
}
