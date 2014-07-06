using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DND.Common
{
    public class CedictEntry
    {
        private readonly string[] pinyin;
        private readonly CedictMeaning[] meanings;

        public readonly string ChSimpl;
        public readonly string ChTrad;
        public IEnumerable<string> Pinyin
        {
            get { return pinyin; }
        }
        public IEnumerable<CedictMeaning> Meanings
        {
            get { return meanings; }
        }

        public CedictEntry(string chSimpl, string chTrad,
            IReadOnlyList<string> pinyin,
            IReadOnlyList<CedictMeaning> meanings)
        {
            ChSimpl = chSimpl;
            ChTrad = chTrad;
            this.pinyin = new string[pinyin.Count];
            for (int i = 0; i != pinyin.Count; ++i) this.pinyin[i] = pinyin[i];
            this.meanings = new CedictMeaning[meanings.Count];
            for (int i = 0; i != meanings.Count; ++i) this.meanings[i] = meanings[i];
        }
    }
}
