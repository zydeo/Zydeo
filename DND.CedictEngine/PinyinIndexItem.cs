using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DND.Common;

namespace DND.CedictEngine
{
    public class PinyinIndexItem : IBinSerializable
    {
        public readonly List<int> Entries;

        public PinyinIndexItem()
        {
            Entries = new List<int>();
        }

        public PinyinIndexItem(BinReader br)
        {
            Entries = new List<int>(br.ReadArray(brr => brr.ReadInt()));
        }

        public void Serialize(BinWriter bw)
        {
            bw.WriteArray(Entries, (i, bwr) => bwr.WriteInt(i));
        }
    }
}
