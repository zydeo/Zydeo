using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using ZD.Common;

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
                lst.Sort((x, y) => y.Avg.CompareTo(x.Avg));
                foreach (var x in lst) pinyin.Add(x.Py);
            }

            // Done.
            return new UniHanziInfo(canBeSimp, tradVariants.ToArray(), pinyin.ToArray());
        }

        public void WriteData(BinWriter bw)
        {
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
            // Go back to start of file, write file positions for each character.
            bw.Position = pos;
            foreach (var x in infos)
            {
                bw.WriteShort((short)x.Key);
                bw.WriteInt(x.Value.FilePos);
            }
        }
    }
}
