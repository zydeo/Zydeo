using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using ZD.Common;

namespace ZD.CedictEngine
{
    /// <summary>
    /// One item in list of headwords that contain a Hanzi.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct IdeoEntryPtr
    {
        /// <summary>
        /// Index/position of the entry.
        /// </summary>
        public int EntryIdx;
        /// <summary>
        /// Number of *different* Hanzi in headword.
        /// </summary>
        public byte HwCharCount;
    }

    /// <summary>
    /// Index information for a single hanzi character.
    /// </summary>
    internal class IdeoIndexItem : IBinSerializable
    {
        /// <summary>
        /// Indexes/positions of entries where character occurs in simplified headword.
        /// </summary>
        public readonly List<IdeoEntryPtr> EntriesHeadwordSimp;

        /// <summary>
        /// Indexes/positions of entries where character occurs in traditional headword.
        /// </summary>
        public readonly List<IdeoEntryPtr> EntriesHeadwordTrad;

        /// <summary>
        /// Indexes/positions of entries where character occurs in a sense.
        /// </summary>
        public readonly List<int> EntriesSense;

        /// <summary>
        /// Ctor: initializes an empty instance.
        /// </summary>
        public IdeoIndexItem()
        {
            EntriesHeadwordSimp = new List<IdeoEntryPtr>();
            EntriesHeadwordTrad = new List<IdeoEntryPtr>();
            EntriesSense = new List<int>();
        }

        /// <summary>
        /// Ctor: deserializes data from binary stream.
        /// </summary>
        public IdeoIndexItem(BinReader br)
        {
            int cntSimp = br.ReadInt();
            EntriesHeadwordSimp = new List<IdeoEntryPtr>(cntSimp);
            for (int i = 0; i != cntSimp; ++i)
            {
                IdeoEntryPtr iep = new IdeoEntryPtr { EntryIdx = br.ReadInt(), HwCharCount = br.ReadByte() };
                EntriesHeadwordSimp.Add(iep);
            }
            int cntTrad = br.ReadInt();
            EntriesHeadwordTrad = new List<IdeoEntryPtr>(cntTrad);
            for (int i = 0; i != cntTrad; ++i)
            {
                IdeoEntryPtr iep = new IdeoEntryPtr { EntryIdx = br.ReadInt(), HwCharCount = br.ReadByte() };
                EntriesHeadwordTrad.Add(iep);
            }
            EntriesSense = new List<int>(br.ReadArray(brr => brr.ReadInt()));
        }

        /// <summary>
        /// Serializes object into a binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            int cntSimp = EntriesHeadwordSimp.Count;
            bw.WriteInt(cntSimp);
            for (int i = 0; i != cntSimp; ++i)
            {
                IdeoEntryPtr iep = EntriesHeadwordSimp[i];
                bw.WriteInt(iep.EntryIdx);
                bw.WriteByte(iep.HwCharCount);
            }
            int cntTrad = EntriesHeadwordTrad.Count;
            bw.WriteInt(cntTrad);
            for (int i = 0; i != cntTrad; ++i)
            {
                IdeoEntryPtr iep = EntriesHeadwordTrad[i];
                bw.WriteInt(iep.EntryIdx);
                bw.WriteByte(iep.HwCharCount);
            }
            bw.WriteArray(EntriesSense, (i, bwr) => bwr.WriteInt(i));
        }
    }
}
