using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DND.Common
{
    public class CedictEntry : IBinSerializable
    {
        private readonly string[] pinyin;
        private readonly CedictSense[] senses;

        public readonly string ChSimpl;

        public readonly string ChTrad;

        public IEnumerable<string> Pinyin
        {
            get { return pinyin; }
        }

        public IEnumerable<CedictSense> Senses
        {
            get { return senses; }
        }

        public int PinyinCompare(CedictEntry other)
        {
            // TO-DO: correct lexicographical order by pinyin
            StringBuilder pinyinInOne = new StringBuilder();
            foreach (string str in pinyin)
            {
                if (pinyinInOne.Length != 0) pinyinInOne.Append(' ');
                pinyinInOne.Append(str);
            }
            StringBuilder otherPinyinInOne = new StringBuilder();
            foreach (string str in other.pinyin)
            {
                if (otherPinyinInOne.Length != 0) otherPinyinInOne.Append(' ');
                otherPinyinInOne.Append(str);
            }
            return pinyinInOne.ToString().CompareTo(otherPinyinInOne.ToString());
        }

        public CedictEntry(string chSimpl, string chTrad,
            ReadOnlyCollection<string> pinyin,
            ReadOnlyCollection<CedictSense> senses)
        {
            ChSimpl = chSimpl;
            ChTrad = chTrad;
            this.pinyin = new string[pinyin.Count];
            for (int i = 0; i != pinyin.Count; ++i) this.pinyin[i] = pinyin[i];
            this.senses = new CedictSense[senses.Count];
            for (int i = 0; i != senses.Count; ++i) this.senses[i] = senses[i];
        }

        public CedictEntry(BinReader br)
        {
            pinyin = br.ReadArray(brr => brr.ReadString());
            ChSimpl = br.ReadString();
            ChTrad = br.ReadString();
            senses = br.ReadArray(brr => new CedictSense(brr));
        }

        public void Serialize(BinWriter bw)
        {
            bw.WriteArray(pinyin, (str, bwr) => bwr.WriteString(str));
            bw.WriteString(ChSimpl);
            bw.WriteString(ChTrad);
            bw.WriteArray(senses);
        }
    }
}
