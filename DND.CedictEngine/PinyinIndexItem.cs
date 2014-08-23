using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DND.Common;

namespace DND.CedictEngine
{
    /// <summary>
    /// Index information for a single pinyin syllable (w/o tone)
    /// </summary>
    public class PinyinIndexItem : IBinSerializable
    {
        /// <summary>
        /// Indexes/positions of entries where syllable occurs WIHTOUT tone.
        /// </summary>
        public readonly List<int> EntriesNT;

        /// <summary>
        /// Indexes/positions of entries where syllable occurs with NEUTRAL tone.
        /// </summary>
        public readonly List<int> Entries0;

        /// <summary>
        /// Indexes/positions of entries where syllable occurs with FIRST tone.
        /// </summary>
        public readonly List<int> Entries1;

        /// <summary>
        /// Indexes/positions of entries where syllable occurs with SECOND tone.
        /// </summary>
        public readonly List<int> Entries2;

        /// <summary>
        /// Indexes/positions of entries where syllable occurs with THIRD tone.
        /// </summary>
        public readonly List<int> Entries3;

        /// <summary>
        /// Indexes/positions of entries where syllable occurs with FOURTH tone.
        /// </summary>
        public readonly List<int> Entries4;

        /// <summary>
        /// Ctor: initializes an empty item.
        /// </summary>
        public PinyinIndexItem()
        {
            EntriesNT = new List<int>();
            Entries0 = new List<int>();
            Entries1 = new List<int>();
            Entries2 = new List<int>();
            Entries3 = new List<int>();
            Entries4 = new List<int>();
        }

        /// <summary>
        /// Ctor: deserializes data from binary stream.
        /// </summary>
        public PinyinIndexItem(BinReader br)
        {
            EntriesNT = new List<int>(br.ReadArray(brr => brr.ReadInt()));
            Entries0 = new List<int>(br.ReadArray(brr => brr.ReadInt()));
            Entries1 = new List<int>(br.ReadArray(brr => brr.ReadInt()));
            Entries2 = new List<int>(br.ReadArray(brr => brr.ReadInt()));
            Entries3 = new List<int>(br.ReadArray(brr => brr.ReadInt()));
            Entries4 = new List<int>(br.ReadArray(brr => brr.ReadInt()));
        }

        /// <summary>
        /// Serializes data into binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            bw.WriteArray(EntriesNT, (i, bwr) => bwr.WriteInt(i));
            bw.WriteArray(Entries0, (i, bwr) => bwr.WriteInt(i));
            bw.WriteArray(Entries1, (i, bwr) => bwr.WriteInt(i));
            bw.WriteArray(Entries2, (i, bwr) => bwr.WriteInt(i));
            bw.WriteArray(Entries3, (i, bwr) => bwr.WriteInt(i));
            bw.WriteArray(Entries4, (i, bwr) => bwr.WriteInt(i));
        }
    }
}
