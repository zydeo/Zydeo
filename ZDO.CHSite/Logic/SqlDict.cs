using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

using ZD.Common;

namespace ZDO.CHSite
{
    public partial class SqlDict
    {
        public class HeadAndTrg
        {
            public readonly string Head;
            public readonly string Trg;
            public HeadAndTrg(string head, string trg)
            {
                Head = head;
                Trg = trg;
            }
        }

        public static bool IsHanzi(char c)
        {
            return (c >= 0x4E00 && c <= 0x9FFF) || (c >= 0x3400 && c <= 0x4DFF) ||
                (c >= 0xF900 && c <= 0xFAFF);
        }

        public static bool DoesHeadExist(string head)
        {
            using (MySqlConnection conn = DB.GetConn())
            using (MySqlCommand cmd = DB.GetCmd(conn, "SelCountHead"))
            {
                cmd.Parameters["@hw"].Value = head;
                Int64 count = (Int64)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public static CedictEntry BuildEntry(string headword, string trg)
        {
            Regex re = new Regex(@"([^ ]+) ([^ ]+) \[([^\]]+)\]");
            var m = re.Match(headword);
            return BuildEntry(m.Groups[2].Value, m.Groups[1].Value, m.Groups[3].Value, trg);
        }

        public static CedictEntry BuildEntry(string simp, string trad, string pinyin, string trg)
        {
            // Prepare pinyin as list of proper syllables
            List<PinyinSyllable> pyList = new List<PinyinSyllable>();
            string[] pyRawArr = pinyin.Split(' ');
            foreach (string pyRaw in pyRawArr)
            {
                PinyinSyllable ps = PinyinSyllable.FromDisplayString(pyRaw);
                if (ps == null) ps = new PinyinSyllable(pyRaw, -1);
                pyList.Add(ps);
            }

            // Build TRG entry in "canonical" form; parse; render
            trg = trg.Replace("\r\n", "\n");
            string[] senses = trg.Split('\n');
            string can = trad + " " + simp + " [";
            for (int i = 0; i != pyList.Count; ++i)
            {
                if (i != 0) can += " ";
                can += pyList[i].GetDisplayString(false);
            }
            can += "] /";
            foreach (string str in senses) can += str + "/";
            return Global.HWInfo.ParseFromText(can);
        }

        public static List<HeadAndTrg> GetEntriesBySimp(string simp)
        {
            return null;
        }
    }
}