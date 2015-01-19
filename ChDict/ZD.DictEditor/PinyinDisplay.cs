using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZD.Common;

namespace ZD.DictEditor
{
    static class PinyinDisplay
    {
        public static string GetPinyinDisplay(string pinyin)
        {
            string[] parts = pinyin.Split(new char[] {' '});
            for (int i = 0; i != parts.Length; ++i)
            {
                parts[i] = parts[i].Replace("u:", "v");
                parts[i] = parts[i].Replace("ü", "v");
            }
            PinyinSyllable[] pss = new PinyinSyllable[parts.Length];
            for (int i = 0; i != parts.Length; ++i)
            {
                string part = parts[i];
                string notone = part.Substring(0, part.Length - 1);
                int tone = int.Parse(part.Substring(part.Length - 1));
                if (tone == 5) tone = 0;
                pss[i] = new PinyinSyllable(notone, tone);
            }
            string res = "";
            foreach (PinyinSyllable ps in pss)
            {
                string ds = ps.GetDisplayString(true);
                if (res != "" && ds != "r") res += " ";
                res += ds;
            }
            return res;
        }
    }
}
