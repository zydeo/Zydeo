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
        /// <para>All occurrences of this token, in our tokenized senses.</para>
        /// <para>When loaded from compiled data, only lists > 500 are kept in memory.</para>
        /// <para>Otherwise, deserialize list from file on-demand with <see cref="LoadInstances"/>.</para>
        /// </summary>
        public readonly List<SenseInfo> Instances;

        /// <summary>
        /// Instance list's position in binary data, or -1 if loaded.
        /// </summary>
        public readonly int FilePos;

        public List<SenseInfo> GetOrLoadInstances(BinReader br)
        {
            // List in memory? Great.
            if (FilePos == -1) return Instances;

            // Load list now
            List<SenseInfo> res;
            // Engine opens file on-demand, but separately for each lookup call
            // Accessing non-thread-safe serializer is therefore OK here
            br.Position = FilePos;
            int count = br.ReadInt();
            res = new List<SenseInfo>(count);
            for (int i = 0; i != count; ++i)
            {
                SenseInfo si;
                si.TokenizedSenseId = br.ReadInt();
                si.TokensInSense = br.ReadByte();
                res.Add(si);
            }
            return res;
        }

        /// <summary>
        /// Ctor: creates an empty instance.
        /// </summary>
        public SenseIndexItem()
        {
            Instances = new List<SenseInfo>();
            FilePos = -1;
        }

        /// <summary>
        /// Ctor: deserializes from binary data.
        /// </summary>
        public SenseIndexItem(BinReader br)
        {
            int filePos = br.Position;
            int count = br.ReadInt();
            if (count < 500)
            {
                Instances = null;
                FilePos = filePos;
            }
            else
            {
                Instances = new List<SenseInfo>();
                FilePos = -1;
            }
            for (int i = 0; i != count; ++i)
            {
                SenseInfo si;
                si.TokenizedSenseId = br.ReadInt();
                si.TokensInSense = br.ReadByte();
                if (FilePos == -1) Instances.Add(si);
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
