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
        /// Index of run in sense's equiv in which this token is found.
        /// </summary>
        public int RunIx;

        /// <summary>
        /// Start of corresponding text in run, or 0 if token is placeholder for a Chinese run.
        /// </summary>
        public int StartInRun;

        /// <summary>
        /// Length of corresponding text in run, or 0 if token is placeholder for a Chinese run (full run).
        /// </summary>
        public int LengthInRun;
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
                eqt.RunIx = (int)br.ReadByte();
                eqt.StartInRun = (int)br.ReadShort();
                eqt.LengthInRun = (int)br.ReadShort();
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
                if (eqt.RunIx < byte.MinValue || eqt.RunIx > byte.MaxValue)
                    throw new Exception("RangeIx value out of byte range: " + eqt.StartInRun.ToString());
                if (eqt.StartInRun < short.MinValue || eqt.StartInRun > short.MaxValue)
                    throw new Exception("StartInSense value out of short range: " + eqt.StartInRun.ToString());
                if (eqt.LengthInRun < short.MinValue || eqt.LengthInRun > short.MaxValue)
                    throw new Exception("LengthInSense value out of short range: " + eqt.LengthInRun.ToString());
                byte rangeIx = (byte)eqt.RunIx;
                short startInSense = (short)eqt.StartInRun;
                short lengthInSense = (short)eqt.LengthInRun;
                bw.WriteInt(eqt.TokenId);
                bw.WriteByte(rangeIx);
                bw.WriteShort(startInSense);
                bw.WriteShort(lengthInSense);
            }
        }
    }
}
