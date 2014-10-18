using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// One sense in an entry.
    /// </summary>
    public class CedictSense : IBinSerializable
    {
        /// <summary>
        /// Domain: text in parentheses at start.
        /// </summary>
        public readonly HybridText Domain;
        /// <summary>
        /// Target-language equivalents ("translations").
        /// </summary>
        public readonly HybridText Equiv;
        /// <summary>
        /// Note: text in parentheses at end.
        /// </summary>
        public readonly HybridText Note;

        /// <summary>
        /// Ctor: init immutable instance.
        /// </summary>
        public CedictSense(HybridText domain, HybridText equiv, HybridText note)
        {
            Domain = domain;
            Equiv = equiv;
            Note = note;

        }

        /// <summary>
        /// Ctor: read from binary stream.
        /// </summary>
        public CedictSense(BinReader br)
        {
            Domain = HybridText.Deserialize(br);
            Equiv = HybridText.Deserialize(br);
            Note = HybridText.Deserialize(br);
        }

        /// <summary>
        /// Serialize into binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            Domain.Serialize(bw);
            Equiv.Serialize(bw);
            Note.Serialize(bw);
        }
    }
}
