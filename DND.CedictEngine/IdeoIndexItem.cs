using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DND.Common;

namespace DND.CedictEngine
{
    /// <summary>
    /// Index information for a single hanzi character.
    /// </summary>
    internal class IdeoIndexItem : IBinSerializable
    {
        /// <summary>
        /// Indexes/positions of entries where character occurs in simplified headword.
        /// </summary>
        public readonly List<int> EntriesHeadwordSimp;

        /// <summary>
        /// Indexes/positions of entries where character occurs in traditional headword.
        /// </summary>
        public readonly List<int> EntriesHeadwordTrad;

        /// <summary>
        /// Indexes/positions of entries where character occurs in a sense.
        /// </summary>
        public readonly List<int> EntriesSense;

        /// <summary>
        /// Ctor: initializes an empty instance.
        /// </summary>
        public IdeoIndexItem()
        {
            EntriesHeadwordSimp = new List<int>();
            EntriesHeadwordTrad = new List<int>();
            EntriesSense = new List<int>();
        }

        /// <summary>
        /// Ctor: deserializes data from binary stream.
        /// </summary>
        public IdeoIndexItem(BinReader br)
        {
            EntriesHeadwordSimp = new List<int>(br.ReadArray(brr => brr.ReadInt()));
            EntriesHeadwordTrad = new List<int>(br.ReadArray(brr => brr.ReadInt()));
            EntriesSense = new List<int>(br.ReadArray(brr => brr.ReadInt()));
        }

        /// <summary>
        /// Serializes object into a binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            bw.WriteArray(EntriesHeadwordSimp, (i, bwr) => bwr.WriteInt(i));
            bw.WriteArray(EntriesHeadwordTrad, (i, bwr) => bwr.WriteInt(i));
            bw.WriteArray(EntriesSense, (i, bwr) => bwr.WriteInt(i));
        }
    }
}
