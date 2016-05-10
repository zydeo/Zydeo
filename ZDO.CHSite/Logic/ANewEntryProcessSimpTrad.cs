using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

using ZD.Common;

namespace ZDO.CHSite
{
    /// <summary>
    /// 
    /// </summary>
    [ActionName("newentry_processsimptrad")]
    public class ANewEntryProcessSimpTrad : ApiAction
    {
        /// <summary>
        /// Ctor: init. Boilerplate.
        /// </summary>
        public ANewEntryProcessSimpTrad(HttpContext ctxt) : base(ctxt) { }

        [DataContract]
        public class Result
        {
            [DataMember(Name = "pinyin")]
            public List<List<string>> Pinyin = new List<List<string>>();
            [DataMember(Name = "is_known_headword")]
            public bool IsKnownHeadword = false;
        }

        private static void addIfNew(List<string> lst, string item)
        {
            if (!lst.Contains(item)) lst.Add(item);
        }

        public override void Process()
        {
            string simp = Req.Params["simp"];
            if (simp == null) throw new ApiException(400, "Missing 'simp' parameter.");
            string trad = Req.Params["trad"];
            if (trad == null) throw new ApiException(400, "Missing 'trad' parameter.");
            if (simp.Length != trad.Length) throw new ApiException(400, "'simp' and 'trad' must be of equal length.");
            Result res = new Result();

            // Prepare result: as long as input; empty array for each position
            foreach (char c in simp)
            {
                res.Pinyin.Add(new List<string>());
            }

            // Do we have a CEDICT headword with this simplified and traditional?
            // If yes, fill in pinyin from these
            HeadwordSyll[][] chHeads = Global.HWInfo.GetPossibleHeadwords(simp, false);
            for (int i = 0; i != chHeads.Length; ++i)
            {
                HeadwordSyll[] sylls = chHeads[i];
                bool matches = true;
                for (int j = 0; j != trad.Length; ++j)
                {
                    if (sylls[j].Simp != simp[j] || sylls[j].Trad != trad[j])
                    { matches = false; break; }
                }
                if (matches)
                {
                    res.IsKnownHeadword = true;
                    for (int j = 0; j != trad.Length; ++j)
                        addIfNew(res.Pinyin[j], sylls[j].Pinyin);
                }
            }
            // At each position, add missing pinyins that match both simplified and traditional
            UniHanziInfo[] suhis = Global.HWInfo.GetUnihanInfo(simp);
            UniHanziInfo[] tuhis = Global.HWInfo.GetUnihanInfo(trad);
            for (int i = 0; i != simp.Length; ++i)
            {
                UniHanziInfo suhi = suhis[i];
                UniHanziInfo tuhi = tuhis[i];
                string[] spyarr = new string[suhi.Pinyin.Length];
                for (int j = 0; j != suhi.Pinyin.Length; ++j) spyarr[j] = suhi.Pinyin[j].GetDisplayString(true);
                string[] tpyarr = new string[tuhi.Pinyin.Length];
                for (int j = 0; j != tuhi.Pinyin.Length; ++j) tpyarr[j] = tuhi.Pinyin[j].GetDisplayString(true);
                foreach (string py in spyarr)
                {
                    if (Array.IndexOf(tpyarr, py) >= 0) addIfNew(res.Pinyin[i], py);
                }
            }            

            // Check if there are positions where we have no tradition or pinyin
            // For the purposes of this lookup, we just inject character from input there
            for (int i = 0; i != simp.Length; ++i)
            {
                char c = simp[i];
                if (res.Pinyin[i].Count == 0) res.Pinyin[i].Add(c.ToString());
            }

            // Tell our caller
            Res = res;
        }
    }
}