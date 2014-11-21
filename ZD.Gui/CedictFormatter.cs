using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZD.Common;

namespace ZD.Gui
{
    /// <summary>
    /// Formats pinyin and full Cedict entries for the UI and Clipboard.
    /// </summary>
    internal static class CedictFormatter
    {
        /// <summary>
        /// Gets entry's pinyin as a single string, with diacritics for tone.
        /// </summary>
        /// <param name="entry">The entry whose pinyin is retrieved.</param>
        /// <param name="syllLimit">Maximum number of syllables before ellipsis, or -1 for no limit.</param>
        /// <returns>The pinyin as a single string.</returns>
        public static string GetPinyinString(CedictEntry entry, int syllLimit = -1)
        {
            var pinyinFull = entry.GetPinyinForDisplay(true);
            List<PinyinSyllable> pinyinList = new List<PinyinSyllable>();
            bool ellipsed = false;
            if (syllLimit == -1) pinyinList.AddRange(pinyinFull);
            else
            {
                int i;
                for (i = 0; i < pinyinFull.Count && i < syllLimit; ++i)
                    pinyinList.Add(pinyinFull[i]);
                if (i != pinyinFull.Count) ellipsed = true;
            }
            string res = "";
            foreach (var x in pinyinList)
            {
                if (res.Length > 0 && !SticksLeft(x.Text)) res += " ";
                res += x.GetDisplayString(true);
            }
            if (ellipsed) res += " …";
            return res;
        }

        /// <summary>
        /// Returns whether string sticks left because it is a punctuation mark like a comma.
        /// </summary>
        public static bool SticksLeft(string str)
        {
            if (str.Length == 0) return true;
            return char.IsPunctuation(str[0]) && str[0] != '·';
        }

        /// <summary>
        /// Gets Pinyin syllables as a single string, unnormalized, with numbers for tone marks.
        /// </summary>
        public static string GetPinyinCedict(IEnumerable<PinyinSyllable> sylls)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (PinyinSyllable ps in sylls)
            {
                if (!first && !SticksLeft(ps.Text)) sb.Append(' ');
                first = false;
                sb.Append(ps.Text);
                if (ps.Tone != -1) sb.Append(ps.Tone.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts a hybrid text to CEDICT-formatted plain text (marking up hanzi+pinyin sections).
        /// </summary>
        public static string HybridToCedict(HybridText ht)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            for (int i = 0; i != ht.RunCount; ++i)
            {
                TextRun tr = ht.GetRunAt(i);
                if (tr is TextRunLatin)
                {
                    string strRun = tr.GetPlainText();
                    if (!first && strRun != string.Empty && !char.IsPunctuation(strRun[0])) sb.Append(' ');
                    sb.Append(strRun);
                }
                else
                {
                    if (!first) sb.Append(' ');
                    TextRunZho trz = tr as TextRunZho;
                    if (!string.IsNullOrEmpty(trz.Simp)) sb.Append(trz.Simp);
                    if (trz.Trad != trz.Simp && !string.IsNullOrEmpty(trz.Trad))
                    {
                        sb.Append('|');
                        sb.Append(trz.Trad);
                    }
                    if (trz.Pinyin != null)
                    {
                        sb.Append('[');
                        sb.Append(GetPinyinCedict(trz.Pinyin));
                        sb.Append(']');
                    }
                }
                first = false;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the entry formatted a single CEDICT plain text line.
        /// </summary>
        public static string GetCedict(CedictEntry entry)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(entry.ChTrad);
            sb.Append(' ');
            sb.Append(entry.ChSimpl);
            sb.Append(" [");
            sb.Append(GetPinyinCedict(entry.Pinyin));
            sb.Append("] /");
            foreach (var sense in entry.Senses)
            {
                string strDomain = HybridToCedict(sense.Domain);
                string strEquiv = HybridToCedict(sense.Equiv);
                string strNote = HybridToCedict(sense.Note);
                sb.Append(strDomain);
                if (strDomain != string.Empty && strDomain != "CL:")
                    if (strEquiv != string.Empty || strNote != string.Empty)
                        sb.Append(' ');
                sb.Append(strEquiv);
                if (strEquiv != string.Empty && strNote != string.Empty)
                    sb.Append(' ');
                sb.Append(strNote);
                sb.Append('/');
            }

            // Done.
            return sb.ToString();
        }
    }
}
