using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.Common
{
    public class CedictSense : IBinSerializable
    {
        public readonly string Domain;
        public readonly string Equiv;
        public readonly string Note;

        public CedictSense(string domain, string equiv, string note)
        {
            Domain = domain == null ? string.Empty : domain;
            Equiv = equiv == null ? string.Empty : equiv;
            Note = note == null ? string.Empty : note;

        }

        public CedictSense(BinReader br)
        {
            Domain = br.ReadString();
            Equiv = br.ReadString();
            Note = br.ReadString();
        }

        public void Serialize(BinWriter bw)
        {
            bw.WriteString(Domain);
            bw.WriteString(Equiv);
            bw.WriteString(Note);
        }
    }
}
