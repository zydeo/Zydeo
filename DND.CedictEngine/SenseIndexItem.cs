using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using DND.Common;

namespace DND.CedictEngine
{
    /// <summary>
    /// Represents one occurrence of a token in a tokenized sense.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SenseInfo
    {
        /// <summary>
        /// The tokenized sense's ID (index during parse; file position after compilation).
        /// </summary>
        public int TokenizedSenseId;

        /// <summary>
        /// Number of tokens in sense.
        /// </summary>
        public byte TokensInSense;
    }

    /// <summary>
    /// Index information for a single target-word token.
    /// </summary>
    internal class SenseIndexItem : IBinSerializable
    {
        /// <summary>
        /// All occurrences of this token, in our tokenized senses.
        /// </summary>
        public readonly List<SenseInfo> Instances;

        /// <summary>
        /// Ctor: creates an empty instance.
        /// </summary>
        public SenseIndexItem()
        {
            Instances = new List<SenseInfo>();
        }

        /// <summary>
        /// Ctor: deserializes from binary data.
        /// </summary>
        public SenseIndexItem(BinReader br)
        {
            int count = br.ReadInt();
            Instances = new List<SenseInfo>();
            for (int i = 0; i != count; ++i)
            {
                SenseInfo si;
                si.TokenizedSenseId = br.ReadInt();
                si.TokensInSense = br.ReadByte();
                Instances.Add(si);
            }
        }

        /// <summary>
        /// Serializes object into a binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            bw.WriteInt(Instances.Count);
            foreach (SenseInfo si in Instances)
            {
                bw.WriteInt(si.TokenizedSenseId);
                bw.WriteByte(si.TokensInSense);
            }
        }
    }
}
