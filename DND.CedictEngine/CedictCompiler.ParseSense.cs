using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.CedictEngine
{
    partial class CedictCompiler
    {
        /// <summary>
        /// <para>Parses one sense, to separate domain, equivalent, and note.</para>
        /// <para>In input, sense comes like this, with domain/note optional:</para>
        /// <para>(domain) (domain) equiv, equiv, equiv (note) (note)</para>
        /// </summary>
        private void parseSense(string sense, out string domain, out string equiv, out string note)
        {
            equiv = sense;
            domain = note = "";
        }
    }
}
