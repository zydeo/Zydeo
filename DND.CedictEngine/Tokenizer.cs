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
            int rangeIx = -1;
            foreach (TextRun tr in txt.Runs)
            {
                ++rangeIx;
                if (tr is TextRunZho)
                {
                    int idZho = wh.IdZho;
                    EquivToken eqt = new EquivToken
                    {
                        TokenId = idZho,
                        RangeIx = rangeIx,
                        StartInRange = 0,
                        LengthInRange = 0,
                    };
                    res.Add(eqt);
                    continue;
                }
                string str = tr.GetPlainText();
                string[] parts = str.Split(new char[] { ' ', '-' });
                foreach (string wd in parts)
                {
                    string x = wd.Trim(trimPuncChars);
                    if (x == string.Empty) continue;
                    if (reNumbers.IsMatch(x)) x = WordHolder.TokenNum;
                    else x = x.ToLowerInvariant();
                    int tokenId = wh.GetTokenId(x);
                    EquivToken eqt = new EquivToken
                    {
                        TokenId = tokenId,
                        RangeIx = rangeIx,
                        StartInRange = 0,
                        LengthInRange = 0,
                    };
                    res.Add(eqt);
                }
            }
            return new ReadOnlyCollection<EquivToken>(res);
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
    }
}
