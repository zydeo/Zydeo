using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DND.Common;

namespace DND.CedictEngine
{
    internal class Index : IBinSerializable
    {
        public readonly Dictionary<char, IdeoIndexItem> IdeoIndex;
        public readonly Dictionary<string, PinyinIndexItem> PinyinIndex;

        public Index()
        {
            IdeoIndex = new Dictionary<char, IdeoIndexItem>();
            PinyinIndex = new Dictionary<string, PinyinIndexItem>();
        }

        public Index(BinReader br)
        {
            IdeoIndex = new Dictionary<char, IdeoIndexItem>();
            PinyinIndex = new Dictionary<string, PinyinIndexItem>();

            int ideoIndexKeyCount = br.ReadInt();
            for (int i = 0; i != ideoIndexKeyCount; ++i)
            {
                char c = br.ReadChar();
                IdeoIndexItem iii = new IdeoIndexItem(br);
                IdeoIndex[c] = iii;
            }

            int pinyinIndexKeyCount = br.ReadInt();
            for (int i = 0; i != pinyinIndexKeyCount; ++i)
            {
                string str = br.ReadString();
                PinyinIndexItem pyi = new PinyinIndexItem(br);
                PinyinIndex[str] = pyi;
            }
        }

        public void Serialize(BinWriter bw)
        {
            int ideoIndexKeyCount = IdeoIndex.Count;
            bw.WriteInt(ideoIndexKeyCount);
            foreach (var x in IdeoIndex)
            {
                bw.WriteChar(x.Key);
                x.Value.Serialize(bw);
            }

            int pinyinIndexKeyCount = PinyinIndex.Keys.Count;
            bw.WriteInt(pinyinIndexKeyCount);
            foreach (var x in PinyinIndex)
            {
                bw.WriteString(x.Key);
                x.Value.Serialize(bw);
            }
        }
    }
}
