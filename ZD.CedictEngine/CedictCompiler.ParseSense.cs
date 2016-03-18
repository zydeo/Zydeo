using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using ZD.Common;

namespace ZD.CedictEngine
{
    partial class CedictCompiler
    {
        /// <summary>
        /// <para>Parses one sense, to separate domain, equivalent, and note.</para>
        /// <para>In input, sense comes like this, with domain/note optional:</para>
        /// <para>(domain) (domain) equiv, equiv, equiv (note) (note)</para>
        /// </summary>
        private static void trimSense(string sense, out string domain, out string equiv, out string note)
        {
            sense = sense.Trim();
            // Special case: sense starts with "CL:"
            // --> This is a classifier. Put "CL:" in domain and leave only Chinese in equiv
            if (sense.StartsWith("CL:"))
            {
                equiv = sense.Substring(3);
                domain = "CL:";
                note = "";
                return;
            }
            // Array with parenthesis depths and content/non-content flags for chars in sense
            // -1: WS or parenthesis
            // 0 or greater: parenthesis depth
            int[] flags = new int[sense.Length];
            int depth = 0;
            for (int i = 0; i != sense.Length; ++i)
            {
                char c = sense[i];
                if (char.IsWhiteSpace(c)) flags[i] = -1;
                else if (c == '(')
                {
                    flags[i] = -1;
                    ++depth;
                }
                else if (c == ')')
                {
                    flags[i] = -1;
                    --depth;
                }
                else flags[i] = depth;
            }
            // Find first char that is depth 0, from left
            int equivStart = -1;
            for (int i = 0; i != flags.Length; ++i)
            {
                if (flags[i] == 0)
                {
                    equivStart = i;
                    break;
                }
            }
            // No real equiv, just domain
            if (equivStart == -1)
            {
                domain = sense;
                equiv = note = "";
                return;
            }
            domain = sense.Substring(0, equivStart);
            // Find first char that is depth 0, from right
            int equivEnd = -1;
            for (int i = flags.Length - 1; i >= 0; --i)
            {
                if (flags[i] == 0)
                {
                    equivEnd = i;
                    break;
                }
            }
            // Cannot be -1: we found at least one depth=0 char before
            // No note
            if (equivEnd == flags.Length - 1)
            {
                equiv = sense.Substring(equivStart);
                note = "";
                return;
            }
            equiv = sense.Substring(equivStart, equivEnd - equivStart + 1);
            note = sense.Substring(equivEnd + 1);
        }

        /// <summary>
        /// Base classes for translating regex matches into specific Chinese text runs.
        /// </summary>
        private abstract class MatchTranslator
        {
            /// <summary>
            /// Translates a regex match with groups into a Chinese text run.
            /// </summary>
            /// <param name="groups">The associated regex's groups.</param>
            /// <returns>The resolved Chinese text run.</returns>
            public abstract TextRunZho Translate(GroupCollection groups, NormalizePinyinDelegate npd);
        }

        /// <summary>
        /// Regex to find 茶壺|茶壶[cha2 hu2] sequences.
        /// </summary>
        private static Regex reSTP = new Regex(@"([0-9A-B\u2e80-\ufff0]+) *\| *([0-9A-B\u2e80-\ufff0]+) *\[([^\]]+)\]");

        /// <summary>
        /// Translates one match of the <see cref="reSTP"/> regex into a Chinese text run.
        /// </summary>
        private class MatchTranslatorSTP : MatchTranslator
        {
            public override TextRunZho Translate(GroupCollection groups, NormalizePinyinDelegate npd)
            {
                string trad = groups[1].Value;
                string simp = groups[2].Value;
                string[] pinyinParts = groups[3].Value.Trim().Split(new char[] { ' ' });
                PinyinSyllable[] pinyin;
                List<int> pinyinMap;
                npd(pinyinParts, out pinyin, out pinyinMap);
                return new TextRunZho(simp, trad, pinyin);
            }
        }

        /// <summary>
        /// Regex to find 夂[zhi3] sequences.
        /// </summary>
        private static Regex reSP = new Regex(@"([0-9A-B\u2e80-\ufff0]+) *\[([^\]]+)\]");

        /// <summary>
        /// Translates one match of the <see cref="reSP"/> regex into a Chinese text run.
        /// </summary>
        private class MatchTranslatorSP : MatchTranslator
        {
            public override TextRunZho Translate(GroupCollection groups, NormalizePinyinDelegate npd)
            {
                string trad = groups[1].Value;
                string simp = groups[1].Value;
                string[] pinyinParts = groups[2].Value.Trim().Split(new char[] { ' ' });
                PinyinSyllable[] pinyin;
                List<int> pinyinMap;
                npd(pinyinParts, out pinyin, out pinyinMap);
                return new TextRunZho(simp, trad, pinyin);
            }
        }

        /// <summary>
        /// Regex to find 一併|一并 sequences.
        /// </summary>
        private static Regex reST = new Regex(@"([0-9A-B\u2e80-\ufff0]+) *\| *([0-9A-B\u2e80-\ufff0]+)");

        /// <summary>
        /// Translates one match of the <see cref="reST"/> regex into a Chinese text run.
        /// </summary>
        private class MatchTranslatorST : MatchTranslator
        {
            public override TextRunZho Translate(GroupCollection groups, NormalizePinyinDelegate npd)
            {
                string trad = groups[1].Value;
                string simp = groups[2].Value;
                return new TextRunZho(simp, trad, null);
            }
        }

        /// <summary>
        /// Regex to find 祭祀 sequences.
        /// </summary>
        private static Regex reS = new Regex(@"([\u2e80-\ufff0]+)");

        /// <summary>
        /// Translates one match of the <see cref="reS"/> regex into a Chinese text run.
        /// </summary>
        private class MatchTranslatorS : MatchTranslator
        {
            public override TextRunZho Translate(GroupCollection groups, NormalizePinyinDelegate npd)
            {
                string trad = groups[1].Value;
                string simp = groups[1].Value;
                return new TextRunZho(simp, trad, null);
            }
        }

        /// <summary>
        /// Regex to find [tou4] sequences.
        /// </summary>
        private static Regex reP = new Regex(@"\[([a-zA-Z][^\]]+)\]");

        /// <summary>
        /// Translates one match of the <see cref="reP"/> regex into a Chinese text run.
        /// </summary>
        private class MatchTranslatorP : MatchTranslator
        {
            public override TextRunZho Translate(GroupCollection groups, NormalizePinyinDelegate npd)
            {
                string[] pinyinParts = groups[1].Value.Trim().Split(new char[] { ' ' });
                PinyinSyllable[] pinyin;
                List<int> pinyinMap;
                npd(pinyinParts, out pinyin, out pinyinMap);
                return new TextRunZho(pinyin);
            }
        }

        /// <summary>
        /// Parses a single text run (plain text), replacing regex matches with Chinese text ranges.
        /// </summary>
        /// <param name="strRun">The text to parse.</param>
        /// <param name="re">The regex to use.</param>
        /// <param name="mtr">An object matching the regex, used to translate into correct Chinese run.</param>
        /// <returns>The new runs that shall replace the one whose text we parsed.</returns>
        private static List<TextRun> parseRun(string strRun, Regex re, MatchTranslator mtr)
        {
            List<TextRun> res = new List<TextRun>();
            // Find regex's matches in inpout
            var matches = re.Matches(strRun);
            int start = 0;
            foreach (Match m in matches)
            {
                // Anything before match's start and the last character we looked at?
                // If yes, it becomes a Latin run
                string textBefore = strRun.Substring(start, m.Index - start).Trim();
                if (textBefore != string.Empty) res.Add(new TextRunLatin(textBefore));
                // Translate match into a Chinese run
                res.Add(mtr.Translate(m.Groups, normalizePinyin));
                // Move finger
                start = m.Index + m.Length;
            }
            // Trailing text at end? That's a final Latin range.
            if (start < strRun.Length)
            {
                string textAfter = strRun.Substring(start, strRun.Length - start).Trim();
                if (textAfter != string.Empty) res.Add(new TextRunLatin(textAfter));
            }
            // Donee
            return res;
        }

        /// <summary>
        /// Split a list of runs into possibly more runs, by running a specific Chinese recognizer.
        /// </summary>
        private static List<TextRun> splitRuns(List<TextRun> runs, Regex re, MatchTranslator mtr)
        {
            List<TextRun> res = new List<TextRun>();
            foreach (TextRun run in runs)
            {
                if (run is TextRunZho) res.Add(run);
                else res.AddRange(parseRun(run.GetPlainText(), re, mtr));
            }
            return res;
        }

        /// <summary>
        /// <para>Parses embedded Chinese ranges in string to create hybrid text with mixed runs.</para>
        /// </summary>
        /// <param name="str">String to parse. Can be null or empty.</param>
        /// <param name="lineNum">Line number in input file (to log errors/warnings).</param>
        /// <param name="logStream">Log stream (to log errors/warnings).</param>
        /// <returns>The input's hybrid text representation, or null if we failed to parse.</returns>
        private static HybridText plainTextToHybrid(string str, int lineNum, StreamWriter logStream)
        {
            if (string.IsNullOrEmpty(str)) return HybridText.Empty;

            // Start with a single run, assumed to be pure Latin
            List<TextRun> runs = new List<TextRun>();
            runs.Add(new TextRunLatin(str));

            // Go over each run and replace it with one or more runs, as per result of inreasingly more narrow matchers
            runs = splitRuns(runs, reSTP, new MatchTranslatorSTP());
            runs = splitRuns(runs, reSP, new MatchTranslatorSP());
            runs = splitRuns(runs, reST, new MatchTranslatorST());
            runs = splitRuns(runs, reS, new MatchTranslatorS());
            runs = splitRuns(runs, reP, new MatchTranslatorP());

            // Sanity checks
            foreach (TextRun tr in runs)
            {
                TextRunLatin trl = tr as TextRunLatin;
                if (trl == null) continue;
                // Must not have | or hanzi in Latin
                string rtext = trl.GetPlainText();
                if (rtext.Contains("|") || hasIdeo(rtext))
                {
                    string msg = "Line {0}: ERROR: Failed to convert sense to hybrid text: {1}";
                    msg = string.Format(msg, lineNum, str);
                    if (logStream != null) logStream.WriteLine(msg);
                    return null;
                }
            }

            // Done.
            return new HybridText(new ReadOnlyCollection<TextRun>(runs));
        }
    }
}
