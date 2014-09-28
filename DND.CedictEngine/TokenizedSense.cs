using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using DND.Common;

namespace DND.CedictEngine
{
    /// <summary>
    /// One tokenized content word in a sense's "equiv" (indexed) section.
    /// </summary>
    internal struct EquivToken
    {
        /// <summary>
        /// This token's ID.
        /// </summary>
        public int TokenId;

        /// <summary>
        /// Index of range in sense's equiv in which this token is found.
        /// </summary>
        public int RangeIx;

        /// <summary>
        /// Start of corresponding text in range, or 0 if token is placeholder for a Chinese range.
        /// </summary>
        public int StartInRange;

        /// <summary>
        /// Length of corresponding text in range, or 0 if token is placeholder for a Chinese range (full range).
        /// </summary>
        public int LengthInRange;
    }

    /// <summary>
    /// Tokenized form of one sense's equiv, with pointers to host entry.
    /// </summary>
    internal class TokenizedSense : IBinSerializable
    {
        /// <summary>
        /// Index of entry to which this tokenized sense belongs. (ID during parse; file position in finalized data.)
        /// </summary>
        public int EntryId;
        /// <summary>
        /// Index of sense within entry.
        /// </summary>
        public readonly int SenseIx;
        /// <summary>
        /// Sequence of tokens that make up the sense's equiv.
        /// </summary>
        public readonly List<EquivToken> EquivTokens;

        /// <summary>
        /// Ctor: intialize immutable instance.
        /// </summary>
        public TokenizedSense(int entryId, int senseIx, ReadOnlyCollection<EquivToken> equivTokens)
        {
            EntryId = entryId;
            SenseIx = senseIx;
            EquivTokens = new List<EquivToken>(equivTokens);
        }

        /// <summary>
        /// Ctor: read from binary stream.
        /// </summary>
        public TokenizedSense(BinReader br)
        {
            EntryId = br.ReadInt();
            SenseIx = br.ReadInt();
            int equivTokenCount = br.ReadInt();
            EquivTokens = new List<EquivToken>(equivTokenCount);
            for (int i = 0; i != equivTokenCount; ++i)
            {
                EquivToken eqt = new EquivToken();
                eqt.TokenId = br.ReadInt();
                eqt.RangeIx = (int)br.ReadByte();
                eqt.StartInRange = (int)br.ReadByte();
                eqt.LengthInRange = (int)br.ReadByte();
                EquivTokens.Add(eqt);
            }
        }

        /// <summary>
        /// Serialize to binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            bw.WriteInt(EntryId);
            bw.WriteInt(SenseIx);
            int equivTokenCount = EquivTokens.Count;
            bw.WriteInt(equivTokenCount);
            for (int i = 0; i != equivTokenCount; ++i)
            {
                EquivToken eqt = EquivTokens[i];
                if (eqt.RangeIx < byte.MinValue || eqt.RangeIx > byte.MaxValue)
                    throw new Exception("RangeIx value out of byte range: " + eqt.StartInRange.ToString());
                if (eqt.StartInRange < byte.MinValue || eqt.StartInRange > byte.MaxValue)
                    throw new Exception("StartInSense value out of byte range: " + eqt.StartInRange.ToString());
                if (eqt.LengthInRange < byte.MinValue || eqt.LengthInRange > byte.MaxValue)
                    throw new Exception("LengthInSense value out of byte range: " + eqt.LengthInRange.ToString());
                byte rangeIx = (byte)eqt.RangeIx;
                byte startInSense = (byte)eqt.StartInRange;
                byte lengthInSense = (byte)eqt.LengthInRange;
                bw.WriteInt(eqt.TokenId);
                bw.WriteByte(rangeIx);
                bw.WriteByte(startInSense);
                bw.WriteByte(lengthInSense);
            }
        }
    }
}
