using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace DND.Common
{
    partial class CedictPinyinSyllable
    {
        static Dictionary<string, List<string>> toneMap = new Dictionary<string, List<string>>();

        /// <summary>
        /// Static ctor: initializes tone mark map of pinyin syllables from embedded resource.
        /// </summary>
        static CedictPinyinSyllable()
        {
            Assembly a = Assembly.GetExecutingAssembly();
            using (Stream s = a.GetManifestResourceStream("DND.Common.Resources.pinyin.txt"))
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
            if (toneMap.ContainsKey(Text))
            {
                if (!diacritics)
                {
                    string res = toneMap[Text][0];
                    res += displayTone.ToString();
                    return res;
                }
                return toneMap[Text][Tone];
            }
            // Try removing retroflex r
            if (Text.EndsWith("r") && toneMap.ContainsKey(Text.Substring(0, Text.Length - 1)))
            {
                string syllKey = Text.Substring(0, Text.Length - 1);
                if (!diacritics)
                {
                    string res = toneMap[syllKey][0];
                    res += "r";
                    res += displayTone.ToString();
                    return res;
                }
                return toneMap[syllKey][Tone] + "r";
            }
            // Still not found: return as is, but add tone mark
            return Text + displayTone.ToString();
        }
    }
}
