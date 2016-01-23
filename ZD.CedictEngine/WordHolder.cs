using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZD.Common;

namespace ZD.CedictEngine
{
    /// <summary>
    /// Maps normalized words (tokens) to IDs.
    /// </summary>
    internal class WordHolder : IBinSerializable
    {
        /// <summary>
        /// Hard-wired placeholder for numbers (and other weird number-like entities).
        /// </summary>
        public const string TokenNum = "NUM";

        /// <summary>
        /// Hard-wired placeholder for Chinese text runs separating Latin stuff from more Latin stuff.
        /// </summary>
        public const string TokenZho = "ZHO";

        /// <summary>
        /// ID of unknown word.
        /// </summary>
        public const int IdUnknown = int.MaxValue;

        /// <summary>
        /// ID of number placeholder.
        /// </summary>
        public readonly int IdNum;

        /// <summary>
        /// ID of Chinese run placeholder.
        /// </summary>
        public readonly int IdZho;

        /// <summary>
        /// Maps tokens to IDs.
        /// </summary>
        private readonly Dictionary<string, int> tokenToIdMap = new Dictionary<string, int>();

        /// <summary>
        /// True if object is used in dictionary parsing mode; false if used for lookup.
        /// </summary>
        private readonly bool parsing;

        /// <summary>
        /// Ctor: init empty; used while parsing dictionary content.
        /// </summary>
        internal WordHolder()
        {
            parsing = true;
            IdNum = GetTokenId(TokenNum);
            IdZho = GetTokenId(TokenZho);
        }

        /// <summary>
        /// Ctor: deserialize from binary stream.
        /// </summary>
        internal WordHolder(BinReader br)
        {
            parsing = false;
            int count = br.ReadInt();
            for (int i = 0; i != count; ++i)
            {
                string token = br.ReadString();
                int id = br.ReadInt();
                tokenToIdMap[token] = id;
            }
            IdNum = GetTokenId(TokenNum);
            IdZho = GetTokenId(TokenZho);
        }

        /// <summary>
        /// Serialize to binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            bw.WriteInt(tokenToIdMap.Count);
            foreach (var x in tokenToIdMap)
            {
                bw.WriteString(x.Key);
                bw.WriteInt(x.Value);
            }
        }

        /// <summary>
        /// Gets ID for token. In parsing mode, creates new IDs for unseen tokens; otherwise, returns <see cref="IdUnknown"/>.
        /// </summary>
        internal int GetTokenId(string token)
        {
            if (tokenToIdMap.ContainsKey(token)) return tokenToIdMap[token];
            if (!parsing) return IdUnknown;
            int id = tokenToIdMap.Count;
            tokenToIdMap[token] = id;
            return id;
        }

        /// <summary>
        /// Gets the first real (non-placeholder) token.
        /// </summary>
        internal string GetFirstRealToken()
        {
            foreach (string tok in tokenToIdMap.Keys)
            {
                if (tok == TokenNum) continue;
                if (tok == TokenZho) continue;
                return tok;
            }
            return null;
        }

        /// <summary>
        /// Gets preceding and following words; null at edges.
        /// </summary>
        internal void GetPrevNext(string str, out string prev, out string next)
        {
            prev = next = null;
            string x = null;
            bool nextIsNext = false;
            foreach (string tok in tokenToIdMap.Keys)
            {
                if (tok == TokenNum) continue;
                if (tok == TokenZho) continue;
                if (nextIsNext)
                {
                    next = tok;
                    break;
                }
                if (tok == str)
                {
                    prev = x;
                    nextIsNext = true;
                }
                x = tok;
            }
        }
    }
}
