using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.Common
{
    /// <summary>
    /// One sense in an entry.
    /// </summary>
    public class CedictSense : IBinSerializable
    {
        /// <summary>
        /// Domain: text in parentheses at start.
        /// </summary>
        public readonly string Domain;
        /// <summary>
        /// Target-language equivalents ("translations").
        /// </summary>
        public readonly string Equiv;
        /// <summary>
        /// Note: text in parentheses at end.
        /// </summary>
        public readonly string Note;

        /// <summary>
        /// Ctor: init immutable instance.
        /// </summary>
        public CedictSense(string domain, string equiv, string note)
        {
            Domain = domain == null ? string.Empty : domain;
            Equiv = equiv == null ? string.Empty : equiv;
            Note = note == null ? string.Empty : note;

        }

        /// <summary>
        /// Ctor: read from binary stream.
        /// </summary>
        public CedictSense(BinReader br)
        {
            Domain = br.ReadString();
            Equiv = br.ReadString();
            Note = br.ReadString();
        }

        /// <summary>
        /// Serialize into binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            bw.WriteString(Domain);
            bw.WriteString(Equiv);
            bw.WriteString(Note);
        }
    }
}
