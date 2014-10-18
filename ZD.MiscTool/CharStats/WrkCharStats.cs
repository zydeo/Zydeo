using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace ZD.MiscTool
{
    internal class WrkCharStats : IWorker
    {
        private readonly OptCharStats opt;
        private StreamReader srStrokes;
        private StreamReader srTypes;
        private StreamReader srCedict;
        private StreamWriter swOut;

        private Dictionary<char, CharStatInfo> charMap = new Dictionary<char, CharStatInfo>();

        public WrkCharStats(OptCharStats opt)
        {
            this.opt = opt;
        }

        public void Init()
        {
            srStrokes = new StreamReader(opt.StrokesFileName);
            srTypes = new StreamReader(opt.StrokesTypesFileName);
            srCedict = new StreamReader(opt.CedictFileName);
            swOut = new StreamWriter(opt.OutFileName);
        }

        public void Work()
        {
            doTypesFile();
            doStrokesFile();
            doDictFile();
            detectSimpTradMismatches();
            writeResults();
        }

        public void Finish()
        {
        }

        public void Dispose()
        {
            if (srStrokes != null) srStrokes.Dispose();
            if (srTypes != null) srTypes.Dispose();
            if (srCedict != null) srCedict.Dispose();
            if (swOut != null) swOut.Dispose();
        }

        #region Detect simplified/traditional info mismatches (strokes types vs. actual dictionary data)

        private void detectSimpTradMismatches()
        {
            foreach (var x in charMap)
            {
                // Strokes says traditional, but char occurs in simplified headword
                if (x.Value.HLType == SimpTradType.Trad && x.Value.DictSimpCount > 0)
                    x.Value.SimpTradMismatch = true;
                // Strokes says simplified, but char occurs in traditional headword
                if (x.Value.HLType == SimpTradType.Simp && x.Value.DictTradCount > 0)
                    x.Value.SimpTradMismatch = true;
            }
        }

        #endregion

        #region Dictionary file

        private void doDictFile()
        {
            // Process file line by line
            string line;
            while ((line = srCedict.ReadLine()) != null)
            {
                if (line == "" || line.StartsWith("#")) continue;
                // Split by spaces: first two items will by traditional and simplified headword
                string[] parts = line.Split(new char[] { ' ' });
                string trad = parts[0];
                string simp = parts[1];
                // Create hash sets of chars for counting
                HashSet<char> setTrad = new HashSet<char>();
                HashSet<char> setSimp = new HashSet<char>();
                foreach (char c in trad) setTrad.Add(c);
                foreach (char c in simp) setSimp.Add(c);
                // Increase traditional and simplified counts; record new chars if needed
                foreach (char c in setSimp)
                {
                    if (charMap.ContainsKey(c))
                        ++charMap[c].DictSimpCount;
                    else
                    {
                        CharStatInfo csi = new CharStatInfo
                        {
                            DictSimpCount = 1,
                        };
                        charMap[c] = csi;
                    }
                }
                foreach (char c in setTrad)
                {
                    if (charMap.ContainsKey(c))
                        ++charMap[c].DictTradCount;
                    else
                    {
                        CharStatInfo csi = new CharStatInfo
                        {
                            DictTradCount = 1,
                        };
                        charMap[c] = csi;
                    }
                }
            }
        }

        #endregion

        #region HanziLookup character types

        /// <summary>
        /// Process a char of type 0 (generic)
        /// </summary>
        private void doCharType0(char char1)
        {
            CharStatInfo csi = new CharStatInfo
            {
                HLType = SimpTradType.Both,
            };
            charMap[char1] = csi;
        }

        /// <summary>
        /// Process a char of type 1: simplified form of something
        /// </summary>
        private void doCharType1(char char1, char char2)
        {
            // We alredy listed character before
            if (charMap.ContainsKey(char1))
            {
                // If we've added it as traditional, it now becomes "both"
                if (charMap[char1].HLType == SimpTradType.Trad)
                {
                    charMap[char1].HLType = SimpTradType.Both;
                }
            }
            // Otherwise, add as simplified
            else
            {
                // Add char on left as simplified
                CharStatInfo csi1 = new CharStatInfo
                {
                    HLType = SimpTradType.Simp,
                };
                charMap[char1] = csi1;
            }
            // Add char on right as traditional, or change to "both" if seen as simplified before
            if (charMap.ContainsKey(char2))
            {
                if (charMap[char2].HLType == SimpTradType.Simp)
                    charMap[char2].HLType = SimpTradType.Both;
            }
            else
            {
                CharStatInfo csi2 = new CharStatInfo
                {
                    HLType = SimpTradType.Trad,
                };
                charMap[char2] = csi2;
            }
        }

        /// <summary>
        /// Process a char of type 2: traditional form of something
        /// </summary>
        private void doCharType2(char char1, char char2)
        {
            // We alredy listed character before
            if (charMap.ContainsKey(char1))
            {
                // If we've added it as simplified, it now becomes "both"
                if (charMap[char1].HLType == SimpTradType.Simp)
                {
                    charMap[char1].HLType = SimpTradType.Both;
                }
            }
            // Otherwise, add as traditional
            else
            {
                // Add char on left as traditional
                CharStatInfo csi1 = new CharStatInfo
                {
                    HLType = SimpTradType.Trad,
                };
                charMap[char1] = csi1;
            }
            // Add char on right as simplified, or change to "both" if seen as traditional before
            if (charMap.ContainsKey(char2))
            {
                if (charMap[char2].HLType == SimpTradType.Trad)
                    charMap[char2].HLType = SimpTradType.Both;
            }
            else
            {
                CharStatInfo csi2 = new CharStatInfo
                {
                    HLType = SimpTradType.Simp,
                };
                charMap[char2] = csi2;
            }
        }

        /// <summary>
        /// Process HanziLookup's "character types" file, record simplified/traditional info
        /// </summary>
        private void doTypesFile()
        {
            // Collect equivalent forms here
            Dictionary<char, char> equivs = new Dictionary<char, char>();

            // Process file line by line
            string line;
            while ((line = srTypes.ReadLine()) != null)
            {
                if (line == "" || line.StartsWith("//")) continue;
                string[] parts = line.Split(new char[] { ' ' });
                int charVal1 = int.Parse(parts[0], NumberStyles.HexNumber);
                char char1 = (char)charVal1;
                int typeVal = int.Parse(parts[2]);
                int charVal2 = -1;
                if (parts.Length > 4) charVal2 = int.Parse(parts[4], NumberStyles.HexNumber);
                // Character is generic: we're adding it now
                if (typeVal == 0) doCharType0(char1);
                // Otherwise, it's some kind of relationship
                else
                {
                    char char2 = (char)charVal2;
                    // Simplified form of something
                    if (typeVal == 1) doCharType1(char1, char2);
                    // Traditional form of something
                    else if (typeVal == 2) doCharType2(char1, char2);
                    // Equivalent form: just record for now
                    else if (typeVal == 3) equivs[char1] = char2;
                    else throw new Exception("Undefined type value: " + typeVal.ToString());
                }
            }
            // Deal with equivalent forms
            foreach (var x in equivs)
            {
                // If neither key nor value is there yet, we have nothing to do - no info on simp or trad
                if (!charMap.ContainsKey(x.Key) && !charMap.ContainsKey(x.Value))
                    continue;
                // If both key and value are there: HL type should be identical
                // But it may not be; let's ignore
                if (charMap.ContainsKey(x.Key) && charMap.ContainsKey(x.Value))
                    continue;
                // New and existing char
                char charNew = charMap.ContainsKey(x.Key) ? x.Value : x.Key;
                char charSeen = charMap.ContainsKey(x.Key) ? x.Key : x.Value;
                // Add now
                CharStatInfo csi = new CharStatInfo
                {
                    HLType = charMap[charSeen].HLType,
                };
                charMap[charNew] = csi;
            }
        }

        #endregion

        #region HanziLookup strokes file

        /// <summary>
        /// Process HanziLookup's strokes file: just record if strokes data present for a character.
        /// </summary>
        private void doStrokesFile()
        {
            // Process file line by line
            string line;
            while ((line = srStrokes.ReadLine()) != null)
            {
                if (line == "" || line.StartsWith("//")) continue;
                // We just care about first 4 chars: that's hex of character
                string hex = line.Substring(0, 4);
                int charVal = int.Parse(hex, NumberStyles.HexNumber);
                char c = (char)charVal;
                // If already there in info, mark
                if (charMap.ContainsKey(c))
                    charMap[c].InStrokes = true;
                // Otherwise, add now (HL type will get default "None")
                else
                {
                    CharStatInfo csi = new CharStatInfo
                    {
                        InStrokes = true,
                    };
                    charMap[c] = csi;
                }
            }
        }

        #endregion

        #region Write results

        private void writeResults()
        {
            // Order results by Unicode value first
            List<KeyValuePair<char, CharStatInfo>> res = new List<KeyValuePair<char, CharStatInfo>>();
            foreach (var x in charMap)
                res.Add(x);
            res.Sort((x, y) => x.Key.CompareTo(y.Key));
            // Write header to file
            swOut.WriteLine("code\tchar\thl-script\tin-dict\tin-strokes\tscript-mismatch\tdict-simp-count\tdict-trad-count");
            // Write to results file
            foreach (var x in res)
            {
                string line = "";
                line += "U+" + ((int)x.Key).ToString("X4") + "\t" + x.Key.ToString() + "\t";
                if (x.Value.HLType == SimpTradType.Both) line += "both";
                else if (x.Value.HLType == SimpTradType.Simp) line += "simp";
                else if (x.Value.HLType == SimpTradType.Trad) line += "trad";
                else line += "na";
                if (x.Value.InDict) line += "\tyes";
                else line += "\tno";
                if (x.Value.InStrokes) line += "\tyes";
                else line += "\tno";
                if (x.Value.SimpTradMismatch) line += "\tyes";
                else line += "\tno";
                line += "\t" + x.Value.DictSimpCount.ToString();
                line += "\t" + x.Value.DictTradCount.ToString();

                swOut.WriteLine(line);
            }
        }


        #endregion
    }
}
