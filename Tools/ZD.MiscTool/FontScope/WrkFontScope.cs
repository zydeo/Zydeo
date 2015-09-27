using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZD.MiscTool
{
    internal class WrkFontScope : IWorker
    {
        private readonly OptFontScope opt;
        private StreamReader srCedict;
        private StreamReader srGB;
        private StreamReader srBig5;
        private StreamReader srUni;
        private StreamReader srZ;
        private StreamWriter swChars;
        private StreamWriter swS2T;

        /// <summary>
        /// Roles a character has been attested in.
        /// </summary>
        private enum STRole
        {
            Simp,
            Trad,
            Both,
            Unknown,
        }

        /// <summary>
        /// Info we've gathered about a character.
        /// </summary>
        private class CharInfo
        {
            /// <summary>
            /// Roles this character has been attested in.
            /// </summary>
            public STRole Role;
            /// <summary>
            /// Traditional forms of character, or null.
            /// </summary>
            public string TradForms = "";
            /// <summary>
            /// GB font includes character.
            /// </summary>
            public bool GB;
            /// <summary>
            /// Big5 font includes character.
            /// </summary>
            public bool Big5;
            /// <summary>
            /// Full Arphic font includes character.
            /// </summary>
            public bool Uni;
            /// <summary>
            /// Z strokes anaylizer data includes character.
            /// </summary>
            public bool Z;
            /// <summary>
            /// Number of times the character is attested in CEDICT.
            /// </summary>
            public int AttCount;
        }

        /// <summary>
        /// Info about each character (index is Unicode code point).
        /// </summary>
        private readonly CharInfo[] infos = new CharInfo[65536];

        public WrkFontScope(OptFontScope opt)
        {
            this.opt = opt;
        }

        public void Init()
        {
            srCedict = new StreamReader(opt.CedictFileName);
            srGB = new StreamReader(opt.GBFileName);
            srBig5 = new StreamReader(opt.Big5FileName);
            srUni = new StreamReader(opt.UniFileName);
            srZ = new StreamReader(opt.ZFileName);
            swChars = new StreamWriter(opt.OutCharFileName);
            swS2T = new StreamWriter(opt.OutS2TFileName);
        }

        public void Work()
        {
            // Parse Cedict
            string line;
            while ((line = srCedict.ReadLine()) != null)
            {
                if (line == "" || line.StartsWith("#")) continue;
                // Split by spaces: first two items will by traditional and simplified headword
                string[] parts = line.Split(new char[] { ' ' });
                string trad = parts[0];
                string simp = parts[1];
                // Skip anomalies (length mismatch)
                if (simp.Length != trad.Length) continue;
                // Process
                doOneHeadword(simp, trad);
            }
            // Parse font coverage files
            doParseCoverage(srGB);
            doParseCoverage(srBig5);
            doParseCoverage(srUni);
            doParseCoverage(srZ);
        }

        private void doOneHeadword(string simp, string trad)
        {
            for (int i = 0; i != simp.Length; ++i)
            {
                char s = simp[i];
                char t = trad[i];
                // They are the same
                if (s == t)
                {
                    int val = (int)s;
                    CharInfo ci = infos[val];
                    // Not seen before
                    if (ci == null)
                    {
                        ci = new CharInfo { Role = STRole.Both, AttCount = 1 };
                        infos[val] = ci;
                    }
                    // Seen before
                    else
                    {
                        ci.Role = STRole.Both;
                        ++ci.AttCount;
                    }
                }
                // They are different
                else
                {
                    int sVal = (int)s;
                    int tVal = (int)t;
                    CharInfo sci = infos[sVal];
                    // Simplified character not seen before
                    if (sci == null)
                    {
                        sci = new CharInfo { Role = STRole.Simp, AttCount = 1 };
                        if (!sci.TradForms.Contains(t)) sci.TradForms += t;
                        infos[sVal] = sci;
                    }
                    // Simplified character seen before
                    else
                    {
                        if (sci.Role == STRole.Trad) sci.Role = STRole.Both;
                        ++sci.AttCount;
                        if (!sci.TradForms.Contains(t)) sci.TradForms += t;
                    }
                    CharInfo tci = infos[tVal];
                    // Traditional character not seen before
                    if (tci == null)
                    {
                        tci = new CharInfo { Role = STRole.Trad, AttCount = 1 };
                        infos[tVal] = tci;
                    }
                    // Traditional character seen before
                    else
                    {
                        if (tci.Role == STRole.Simp) tci.Role = STRole.Both;
                        ++tci.AttCount;
                    }
                }
            }
        }

        private void doParseCoverage(StreamReader sr)
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] parts = line.Split(new char[] { '\t' });
                if (parts.Length != 3) continue;
                char c = parts[2][0];
                int val = (int)c;
                CharInfo ci = infos[val];
                if (ci == null)
                {
                    ci = new CharInfo { Role = STRole.Unknown };
                    infos[val] = ci;
                }
                if (sr == srGB) ci.GB = true;
                else if (sr == srBig5) ci.Big5 = true;
                else if (sr == srUni) ci.Uni = true;
                else if (sr == srZ) ci.Z = true;
                else throw new Exception("WTF");
            }
        }

        public void Finish()
        {
            // Write character info
            swChars.WriteLine("Uni\tChar\tSTB\tAttest\tGB\tBig5\tUni\tZ");
            for (int i = 0x4000; i != infos.Length; ++i)
            {
                CharInfo ci = infos[i];
                if (ci == null) continue;
                string stb = "S";
                if (ci.Role == STRole.Trad) stb = "T";
                else if (ci.Role == STRole.Both) stb = "B";
                else if (ci.Role == STRole.Unknown) stb = "U";
                string line = "\\u{0:X4}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}";
                line = string.Format(line, i, (char)i, stb, ci.AttCount,
                    ci.GB ? "yes" : "no",
                    ci.Big5 ? "yes" : "no",
                    ci.Uni ? "yes" : "no",
                    ci.Z ? "yes" : "no");
                swChars.WriteLine(line);
            }
            swChars.Flush();
            // Write S2T
            swS2T.WriteLine("Simp-Uni\tSimp-C\tS-attest\tTrad");
            for (int i = 0x4000; i != infos.Length; ++i)
            {
                CharInfo ci = infos[i];
                if (ci == null) continue;
                if (ci.TradForms == "") continue;
                string line = "\\u{0:X4}\t{1}\t{2}\t{3}";
                line = string.Format(line, i, (char)i, ci.AttCount, ci.TradForms);
                swS2T.WriteLine(line);
            }
            swS2T.Flush();
        }

        public void Dispose()
        {
            if (srCedict != null) srCedict.Dispose();
            if (srGB != null) srGB.Dispose();
            if (srBig5 != null) srBig5.Dispose();
            if (srUni != null) srUni.Dispose();
            if (swChars != null) swChars.Dispose();
            if (swS2T != null) swS2T.Dispose();
        }
    }
}
