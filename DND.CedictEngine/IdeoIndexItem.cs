using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DND.Common;

namespace DND.CedictEngine
{
    internal class IdeoIndexItem : IBinSerializable
    {
        public readonly List<int> EntriesHeadwordSimp;
        public readonly List<int> EntriesHeadwordTrad;
        public readonly List<int> EntriesText;

        public IdeoIndexItem()
        {
            EntriesHeadwordSimp = new List<int>();
            EntriesHeadwordTrad = new List<int>();
            EntriesText = new List<int>();
        }

        public IdeoIndexItem(BinReader br)
        {
            EntriesHeadwordSimp = new List<int>(br.ReadArray(brr => brr.ReadInt()));
            EntriesHeadwordTrad = new List<int>(br.ReadArray(brr => brr.ReadInt()));
            EntriesText = new List<int>(br.ReadArray(brr => brr.ReadInt()));
        }

        public void Serialize(BinWriter bw)
        {
            bw.WriteArray(EntriesHeadwordSimp, (i, bwr) => bwr.WriteInt(i));
            bw.WriteArray(EntriesHeadwordTrad, (i, bwr) => bwr.WriteInt(i));
            bw.WriteArray(EntriesText, (i, bwr) => bwr.WriteInt(i));
        }
    }
}
