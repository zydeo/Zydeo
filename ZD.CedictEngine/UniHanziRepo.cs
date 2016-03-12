using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ZD.Common;

namespace ZD.CedictEngine
{
    /// <summary>
    /// Implements <see cref="ZD.Common.IUniHanziRepo"/>.
    /// </summary>
    public class UniHanziRepo : IUniHanziRepo
    {
        /// <summary>
        /// Data file name; will keep opening at every query.
        /// </summary>
        private readonly string dataFileName;

        /// <summary>
        /// File position for each known character, or 0.
        /// </summary>
        private int[] chrPoss = new int[65335];

        /// <summary>
        /// Ctor: init from compiled binary file.
        /// </summary>
        public UniHanziRepo(string dataFileName)
        {
            this.dataFileName = dataFileName;
            using (BinReader br = new BinReader(dataFileName))
            {
                int chrCnt = br.ReadInt();
                for (int i = 0; i != chrCnt; ++i)
                {
                    short chrVal = br.ReadShort();
                    char chr = (char)chrVal;
                    int filePos = br.ReadInt();
                    chrPoss[(int)chr] = filePos;
                }
            }
        }

        /// <summary>
        /// See <see cref="ZD.Common.IUniHanziRepo.GetInfo"/>.
        /// </summary>
        public UniHanziInfo[] GetInfo(char[] chars)
        {
            UniHanziInfo[] res = new UniHanziInfo[chars.Length];
            using (BinReader br = new BinReader(dataFileName))
            {
                for (int i = 0; i != chars.Length; ++i)
                {
                    char c = chars[i];
                    int pos = chrPoss[(int)c];
                    if (pos == 0) continue;
                    br.Position = pos;
                    res[i] = new UniHanziInfo(br);
                }
            }
            return res;
        }
    }
}
