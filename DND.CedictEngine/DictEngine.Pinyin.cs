using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace DND.CedictEngine
{
    partial class DictEngine
    {
        /// <summary>
        /// Info about a single pinyin syllable for splitting words written w/o spaces
        /// </summary>
        private class PinyinParseSyllable
        {
            /// <summary>
            /// Syllable text (no tone mark, but may include trailing r)
            /// </summary>
            public readonly string Text;
            /// <summary>
            /// True if syllable starts with a vowel (cannot be inside word: apostrophe would be needed)
            /// </summary>
            public readonly bool VowelStart;
            /// <summary>
            /// Ctor: initialize immutable instance.
            /// </summary>
            public PinyinParseSyllable(string text, bool vowelStart)
            {
                Text = text;
                VowelStart = vowelStart;
            }
        }

        /// <summary>
        /// List of known pinyin syllables; longer first.
        /// </summary>
        private static List<PinyinParseSyllable> syllList = new List<PinyinParseSyllable>();

        /// <summary>
        /// Loads known pinyin syllables from embedded resource.
        /// </summary>
        private static void loadSyllabary()
        {
            Assembly a = Assembly.GetExecutingAssembly();
            using (Stream s = a.GetManifestResourceStream("DND.CedictEngine.Resources.pinyin.txt"))
            using (StreamReader sr = new StreamReader(s))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line == string.Empty) continue;
                    string[] parts = line.Split(new char[] { '\t' });
                    PinyinParseSyllable ps = new PinyinParseSyllable(parts[0], parts[1] == "v");
                    syllList.Add(ps);
                }
            }
        }

        /// <summary>
        /// Recursively match pinyin syllables from start position in string.
        /// </summary>
        private static bool doMatchSylls(string str, int pos, List<int> ends)
        {
            // Reach end of string: good
            if (pos == str.Length) return true;
            // Get rest of string to match
            string rest = pos == 0 ? str : str.Substring(pos);
            // Try all syllables in syllabary
            foreach (PinyinParseSyllable ps in syllList)
            {
                // Syllables starting with a vowel not allowed inside text
                if (pos != 0 && ps.VowelStart) continue;
                // Find matching syllable
                if (rest.StartsWith(ps.Text))
                {
                    ends.Add(pos + ps.Text.Length);
                    // If rest matches, we're done
                    if (doMatchSylls(str, pos + ps.Text.Length, ends)) return true;
                    // Otherwise, backtrack, move on to next syllable
                    ends.RemoveAt(ends.Count - 1);
                }
            }
            // If we're here, failed to resolve syllables
            return false;
        }

        /// <summary>
        /// Split string into possible multiple pinyin syllables, or return as whole if not possible.
        /// </summary>
        private static List<string> doPinyinSplitSyllables(string str)
        {
            List<string> res = new List<string>();
            // Sanity check
            if (str == string.Empty) return res;
            // Ending positions of syllables
            List<int> ends = new List<int>();
            // Recursive matching
            doMatchSylls(str, 0, ends);
            // Failed to match: return original string in one
            if (ends.Count == 0)
            {
                res.Add(str);
                return res;
            }
            // Split
            int pos = 0;
            foreach (int i in ends)
            {
                string part = str.Substring(pos, i - pos);
                res.Add(part);
                pos = i;
            }
            // Done.
            return res;
        }
    }
}
