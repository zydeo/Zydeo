using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace ZD.Common
{
    partial class PinyinSyllable
    {
        static Dictionary<string, List<string>> toneMap = new Dictionary<string, List<string>>();

        /// <summary>
        /// Static ctor: initializes tone mark map of pinyin syllables from embedded resource.
        /// </summary>
        static PinyinSyllable()
        {
            Assembly a = Assembly.GetExecutingAssembly();
            using (Stream s = a.GetManifestResourceStream("ZD.Common.Resources.pinyin.txt"))
            using (StreamReader sr = new StreamReader(s))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line == string.Empty) continue;
                    string[] parts = line.Split(new char[] { '\t' });
                    List<string> vals = new List<string>();
                    vals.Add(parts[1]);
                    vals.Add(parts[2]);
                    vals.Add(parts[3]);
                    vals.Add(parts[4]);
                    vals.Add(parts[5]);
                    toneMap[parts[0]] = vals;
                }
            }
        }

        /// <summary>
        /// Upper-cases first character of string, depending on casing of other string's first character.
        /// </summary>
        private static string upperCaseIfNeeded(string to, string from)
        {
            if (!char.IsUpper(from[0])) return to;
            string res = "";
            res += char.ToUpperInvariant(to[0]);
            if (to.Length > 1) res += to.Substring(1);
            return res;
        }

        /// <summary>
        /// <para>Returns a typed Pinyin syllable from its display string (with diacritics).</para>
        /// <para>Returns null if provded string is not recognized as a Pinyin syllable.</para>
        /// </summary>
        public static PinyinSyllable FromDisplayString(string ds)
        {
            PinyinSyllable syll = null;
            foreach (var x in toneMap)
            {
                string pure = null;
                int tone = -1;
                if (x.Value[0] == ds) { pure = x.Value[0]; tone = 0; }
                else if (x.Value[1] == ds) { pure = x.Value[0]; tone = 1; }
                else if (x.Value[2] == ds) { pure = x.Value[0]; tone = 2; }
                else if (x.Value[3] == ds) { pure = x.Value[0]; tone = 3; }
                else if (x.Value[4] == ds) { pure = x.Value[0]; tone = 4; }
                if (tone != -1)
                {
                    syll = new PinyinSyllable(pure, tone);
                    break;
                }
            }
            if (ds == "r") syll = new PinyinSyllable("r", 0);
            return syll;
        }

        /// <summary>
        /// Gets the syllable's display string.
        /// </summary>
        /// <param name="diacritics">If yes, adds diacritics for tone marks; otherwise, appends number.</param>
        /// <returns>String representation to show in UI.</returns>
        public string GetDisplayString(bool diacritics)
        {
            if (Tone == -1) return Text;
            int displayTone = Tone;
            if (displayTone == 0) displayTone = 5;

            // This is a standalone "r5"
            if (Text == "r" && displayTone == 5)
            {
                if (diacritics) return "r";
                else return "r5";
            }

            // We've got display version: go ahead
            string textLo = Text.ToLowerInvariant();
            if (toneMap.ContainsKey(textLo))
            {
                if (!diacritics)
                {
                    string res = toneMap[textLo][0];
                    res = upperCaseIfNeeded(res, Text);
                    res += displayTone.ToString();
                    return res;
                }
                string resD = toneMap[textLo][Tone];
                resD = upperCaseIfNeeded(resD, Text);
                return resD;
            }
            // Try removing retroflex r
            if (textLo.EndsWith("r") && toneMap.ContainsKey(textLo.Substring(0, textLo.Length - 1)))
            {
                string syllKey = textLo.Substring(0, Text.Length - 1);
                if (!diacritics)
                {
                    string res = toneMap[syllKey][0];
                    res += "r";
                    res += displayTone.ToString();
                    res = upperCaseIfNeeded(res, Text);
                    return res;
                }
                string resD = toneMap[syllKey][Tone] + "r";
                resD = upperCaseIfNeeded(resD, Text);
                return resD;
            }
            // Still not found: return as is, but add tone mark
            return Text + displayTone.ToString();
        }
    }
}
