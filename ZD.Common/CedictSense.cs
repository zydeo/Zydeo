using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// One sense in an entry.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{GetPlainText()}")]
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

        /// <summary>
        /// Returns this sense, without the enclosing slashes, in CEDICT format.
        /// </summary>
        public string GetCedict()
        {
            StringBuilder sb = new StringBuilder();
            string domainCedict = Domain.GetCedict();
            if (!string.IsNullOrEmpty(domainCedict)) sb.Append(domainCedict);
            string equivCedict = Equiv.GetCedict();
            if (!string.IsNullOrEmpty(equivCedict))
            {
                if (sb.Length != 0) sb.Append(' ');
                sb.Append(equivCedict);
            }
            string noteCedict = Note.GetCedict();
            if (!string.IsNullOrEmpty(noteCedict))
            {
                if (sb.Length != 0) sb.Append(' ');
                sb.Append(noteCedict);
            }
            sb.Replace('/', '\\');
            return sb.ToString();
        }

        /// <summary>
        /// Gets sense in plain text.
        /// </summary>
        public string GetPlainText()
        {
            StringBuilder sb = new StringBuilder();
            string domainPlain = Domain.GetPlainText();
            if (!string.IsNullOrEmpty(domainPlain)) sb.Append(domainPlain);
            string equivPlain = Equiv.GetPlainText();
            if (!string.IsNullOrEmpty(equivPlain))
            {
                if (sb.Length != 0) sb.Append(' ');
                sb.Append(equivPlain);
            }
            string notePlain = Note.GetPlainText();
            if (!string.IsNullOrEmpty(notePlain))
            {
                if (sb.Length != 0) sb.Append(' ');
                sb.Append(notePlain);
            }
            return sb.ToString();
        }
    }
}
