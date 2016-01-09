using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZD.Common;

namespace ZD.CedictEngine
{
    /// <summary>
    /// Holds information about Hanzi (stroke order data, decomposition etc.)
    /// </summary>
    internal class HanziRepo
    {
        /// <summary>
        /// File positions of each Hanzi, or 0 if no info for that Unicode code point.
        /// </summary>
        private readonly int[] hanziInfoIdx;

        /// <summary>
        /// Init: read HanziInfo file positions for covered Hanzi.
        /// </summary>
        public HanziRepo(BinReader br)
        {
            hanziInfoIdx = new int[65536];
            int cnt = br.ReadInt();
            for (int i = 0; i != cnt; ++i)
            {
                char c = br.ReadChar();
                int pos = br.ReadInt();
                hanziInfoIdx[(int)c] = pos;
            }
        }

        /// <summary>
        /// Gets HanziInfo for a character, or null if no info is available.
        /// </summary>
        public HanziInfo GetHanziInfo(char c, BinReader br)
        {
            int val = (int)c;
            if (val > hanziInfoIdx.Length) return null;
            int pos = hanziInfoIdx[val];
            if (pos == 0) return null;
            br.Position = pos;
            return new HanziInfo(br);
        }
    }
}
