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
    /// Returns information about a Hanzi (stroke order, decomposition etc.)
    /// </summary>
    public class ANewEntryProcessSimp : ApiAction
    {
        /// <summary>
        /// Ctor: init. Boilerplate.
        /// </summary>
        public ANewEntryProcessSimp(HttpContext ctxt) : base(ctxt) { }

        [DataContract]
        public class Result
        {
            [DataMember(Name = "trad")]
            public List<List<string>> Trad = new List<List<string>>();
            [DataMember(Name = "pinyin")]
            public List<List<string>> Pinyin = new List<List<string>>();
        }

        /// <summary>
        /// Retrieves information about (simplified) hanzi.
        /// </summary>
        public override void Process()
        {
            // TO-DO:
            // - When input is a CEDICT headword, give that precedence and just fill the rest
            // - Always note combination of Simp+Trad, and restrict Pinyin to intersection
            // - When no dictionary entry, pick first Trad hanzi that has most common Pinyin

            string simp = Req.Params["simp"];
            if (simp == null) throw new ApiException(400, "Missing 'simp' parameter.");
            Result res = new Result();
            char[] arr = new char[simp.Length];
            for (int i = 0; i != simp.Length; ++i) arr[i] = simp[i];
            UniHanziInfo[] uhis = Global.UHRepo.GetUnihanInfo(arr);
            for (int i = 0; i != uhis.Length; ++i)
            {
                UniHanziInfo uhi = uhis[i];
                List<string> trad = new List<string>();
                List<string> pinyin = new List<string>();
                // If we have Unihan-source info about this Hanzi, serve that
                if (uhi != null)
                {
                    foreach (char c in uhi.TradVariants) trad.Add(c.ToString());
                    foreach (PinyinSyllable syll in uhi.Pinyin) pinyin.Add(syll.GetDisplayString(true));
                }
                // Otherwise, for the purposes of this lookup, return character itself
                else
                {
                    trad.Add(arr[i].ToString());
                    pinyin.Add(arr[i].ToString());
                }
                // Store in result
                res.Trad.Add(trad);
                res.Pinyin.Add(pinyin);
            }
            // Serialize to JSON
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(Result));
            MemoryStream ms = new MemoryStream();
            js.WriteObject(ms, res);
            ms.Position = 0;
            StreamReader sr = new StreamReader(ms);
            Json = sr.ReadToEnd();
        }
    }
}