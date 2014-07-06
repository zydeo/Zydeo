using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.Common
{
    public class CedictMeaning
    {
        public readonly string Domain;
        public readonly string Equiv;
        public readonly string Note;

        public CedictMeaning(string domain, string equiv, string note)
        {
            Domain = domain == null ? string.Empty : domain;
            Equiv = equiv == null ? string.Empty : equiv;
            Note = note == null ? string.Empty : note;

        }
    }
}
