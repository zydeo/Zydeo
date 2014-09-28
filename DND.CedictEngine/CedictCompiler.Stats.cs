using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using DND.Common;

namespace DND.CedictEngine
{
    partial class CedictCompiler
    {
        /// <summary>
        /// Encapsulates statistics captured while compiling dictionary.
        /// </summary>
        private class Stats
        {
            /// <summary>
            /// Name of output file with token counts.
            /// </summary>
            private const string tokenCountsFileName = "ccomp-wc.txt";

            /// <summary>
            /// Count of each normalized target word token (# of entries where they occur).
            /// </summary>
            private readonly Dictionary<string, int> tokenCounts = new Dictionary<string, int>();

            /// <summary>
            /// Add data from one entry to stats.
            /// </summary>
            public void CalculateEntryStats(CedictEntry entry)
            {
                // Get tokens from entry
                HashSet<string> tokens = new HashSet<string>();
                foreach (CedictSense sense in entry.Senses)
                {
                    foreach (TextRun tr in sense.Equiv.Runs)
                    {
                        if (tr is TextRunZho) continue;
                        getTokens(tr as TextRunLatin, tokens);
                    }
                }
                // Increase counts of tokens
                foreach (string token in tokens)
                {
                    if (tokenCounts.ContainsKey(token)) ++tokenCounts[token];
                    else tokenCounts[token] = 1;
                }
            }

            static char[] trimPuncChars = new char[]
            {
                ',',
                ';',
                ':',
                '.',
                '?',
                '!',
                '\'',
                '"',
                '(',
                ')'
            };

            private Regex reNumbers = new Regex(@"^([0-9\-\.\:\,\^\%]+|[0-9]+(th|nd|rd|st|s|m))$");

            /// <summary>
            /// Extract normalized tokens from Latin text run; add each to set.
            /// </summary>
            private void getTokens(TextRunLatin tr, HashSet<string> tokens)
            {
                string str = tr.GetPlainText();
                string[] parts = str.Split(new char[] { ' ', '-' });
                foreach (string wd in parts)
                {
                    string x = wd.Trim(trimPuncChars);
                    if (x == string.Empty) continue;
                    if (reNumbers.IsMatch(x)) x = "*num*";
                    x = x.ToLowerInvariant();
                    tokens.Add(x);
                }
            }

            /// <summary>
            /// Write statistics results to output files.
            /// </summary>
            /// <param name="statsFolder"></param>
            public void WriteStats(string statsFolder)
            {
                string wcFileNameFull = Path.Combine(statsFolder, tokenCountsFileName);
                using (StreamWriter swTokenCounts = new StreamWriter(wcFileNameFull))
                {
                    List<KeyValuePair<string, int>> tokenCountsOrdered = new List<KeyValuePair<string,int>>();
                    foreach (var x in tokenCounts) tokenCountsOrdered.Add(x);
                    tokenCountsOrdered.Sort((a, b) => b.Value.CompareTo(a.Value));
                    foreach (var x in tokenCountsOrdered)
                    {
                        string line = x.Key + "\t" + x.Value;
                        swTokenCounts.WriteLine(line);
                    }
                }
            }
        }
    }
}
