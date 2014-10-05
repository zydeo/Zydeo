using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using DND.Common;

namespace DND.CedictEngine
{
    /// <summary>
    /// Encapsulates functionality to tokenize a hybrid text (equiv).
    /// </summary>
    internal class Tokenizer
    {
        /// <summary>
        /// Word holder to resolve token IDs.
        /// </summary>
        private readonly WordHolder wh;

        /// <summary>
        /// Ctor: takes word holder to work with.
        /// </summary>
        public Tokenizer(WordHolder wh)
        {
            this.wh = wh;
        }

        /// <summary>
        /// <para>Tokenize the sense's equiv, presented as hybrid text.</para>
        /// <para>During parsing, creates new word IDs as tokens come up.</para>
        /// </summary>
        public ReadOnlyCollection<EquivToken> Tokenize(HybridText txt)
        {
            List<EquivToken> res = new List<EquivToken>();
            int runIX = -1;
            foreach (TextRun tr in txt.Runs)
            {
                ++runIX;
                if (tr is TextRunZho)
                {
                    int idZho = wh.IdZho;
                    EquivToken eqt = new EquivToken
                    {
                        TokenId = idZho,
                        RunIx = runIX,
                        StartInRun = 0,
                        LengthInRun = 0,
                    };
                    res.Add(eqt);
                    continue;
                }
                string str = tr.GetPlainText();
                tokenizeRun(str, runIX, res);
            }
            return new ReadOnlyCollection<EquivToken>(res);
        }

        /// <summary>
        /// Tokenizes a single plain text run.
        /// </summary>
        /// <param name="str">Text of the plain text run.</param>
        /// <param name="runIx">Current run index (to be stored in result).</param>
        /// <param name="tokens">List of tokens to append to.</param>
        private void tokenizeRun(string str, int runIx, List<EquivToken> tokens)
        {
            // One flag for each character.
            // If -1, character is outside of any token (e.g., whitespace, punctuation)
            // Otherwise, number indicates token index, from 0 upwards
            int[] flags = new int[str.Length];
            // At first, we assume entire text is a single token
            for (int i = 0; i != flags.Length; ++i) flags[i] = 0;
            // Split by space and dash
            int pos;
            // Trim from start
            for (pos = 0; pos != flags.Length; ++pos)
            {
                char c = str[pos];
                if (c != ' ' && c != '-') break;
                flags[pos] = -1;
            }
            // Move on; when encountering ' ' or '-' again, segment
            for (; pos != flags.Length; ++pos)
            {
                char c = str[pos];
                // Space or dash here: mark as non-token; increase token IX of rest of string
                if (c == ' ' || c == '-')
                {
                    flags[pos] = -1;
                    for (int i = pos + 1; i < flags.Length; ++i) ++flags[i];
                }
            }
            // Trim punctuation from start and end of each token
            pos = 0;
            while (pos != flags.Length)
            {
                // Skip non-tokens
                if (flags[pos] == -1) { ++pos; continue; }
                // Find end of token
                int tokenStart = pos;
                int tokenLength = 0;
                for (int i = pos; i != flags.Length; ++i)
                {
                    // Same token as long as index is the same
                    if (flags[i] == flags[pos]) ++tokenLength;
                    else break;
                }
                // We'll move on in input by length of this token
                pos += tokenLength;
                // Trim punctuation from this token
                trimPunct(str, flags, tokenStart, tokenLength);
            }
            // We're done splitting and trimming, now create an EquivToken for each token.
            pos = 0;
            while (pos != flags.Length)
            {
                if (flags[pos] == -1) { ++pos; continue; }
                // Find end of token
                int tokenStart = pos;
                int tokenLength = 0;
                for (int i = pos; i != flags.Length; ++i)
                {
                    // Same token as long as index is the same
                    if (flags[i] == flags[pos]) ++tokenLength;
                    else break;
                }
                pos += tokenLength;
                // Text of this token, lower-cased, OR *num*
                string strToken = str.Substring(tokenStart, tokenLength);
                if (reNumbers.IsMatch(strToken)) strToken = WordHolder.TokenNum;
                else strToken = strToken.ToLowerInvariant();
                int tokenId = wh.GetTokenId(strToken);
                EquivToken eqt = new EquivToken
                {
                    TokenId = tokenId,
                    RunIx = runIx,
                    StartInRun = tokenStart,
                    LengthInRun = tokenLength,
                };
                tokens.Add(eqt);
            }
        }

        /// <summary>
        /// Trims punctuation from a word.
        /// </summary>
        private static void trimPunct(string str, int[] flags, int start, int length)
        {
            int pos = start;
            // Eat up punctuation from start
            while (pos != start + length)
            {
                if (trimPunctChars.Contains(str[pos])) { flags[pos] = -1; ++pos; }
                else break;
            }
            // Nothing left of token: decrement subsequent indexes to account for disappeared word
            if (pos == start + length)
            {
                for (int i = pos; i != flags.Length; ++i)
                    if (flags[i] > 0) --flags[i];
                return;
            }
            // Move backwards from end of token; trim punctuation
            for (int i = start + length - 1; i >= pos; --i)
            {
                if (trimPunctChars.Contains(str[i])) flags[i] = -1;
                else break;
            }
        }

        /// <summary>
        /// Punctuation that we trim from start and end of words.
        /// </summary>
        static char[] trimPunctChars = new char[]
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

        /// <summary>
        /// Definition of "number", i.e., numerical entity that we don't index as a content word.
        /// </summary>
        private Regex reNumbers = new Regex(@"^([0-9\-\.\:\,\^\%]+|[0-9]+(th|nd|rd|st|s|m))$");
    }
}
