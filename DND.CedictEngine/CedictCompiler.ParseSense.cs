using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using DND.Common;

namespace DND.CedictEngine
{
    partial class CedictCompiler
    {
        /// <summary>
        /// <para>Parses one sense, to separate domain, equivalent, and note.</para>
        /// <para>In input, sense comes like this, with domain/note optional:</para>
        /// <para>(domain) (domain) equiv, equiv, equiv (note) (note)</para>
        /// </summary>
        private void trimSense(string sense, out string domain, out string equiv, out string note)
        {
            sense = sense.Trim();
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
        /// <para>Parses embedded Chinese ranges in string to create hybrid text with mixed runs.</para>
        /// </summary>
        /// <param name="str">String to parse. Can be null or empty.</param>
        /// <returns>The input's hybrid text representation.</returns>
        private HybridText plainTextToHybrid(string str)
        {
            if (string.IsNullOrEmpty(str)) return HybridText.Empty;

            List<TextRun> runs = new List<TextRun>();
            runs.Add(new TextRunLatin(str));
            return new HybridText(new ReadOnlyCollection<TextRun>(runs));
        }
    }
}
