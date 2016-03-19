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
    [ActionName("newentry_verifyhead")]
    public class ANewEntryVerifyHead : ApiAction
    {
        private static readonly string errDuplicate = "Nem egyszerűsített írásjegy:";

        /// <summary>
        /// Ctor: init. Boilerplate.
        /// </summary>
        public ANewEntryVerifyHead(HttpContext ctxt) : base(ctxt) { }

        [DataContract]
        public class Result
        {
            [DataMember(Name = "passed")]
            public bool Passed;
            [DataMember(Name = "ref_entries_html")]
            public string RefEntries = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Process()
        {
            string simp = Req.Params["simp"];
            if (simp == null) throw new ApiException(400, "Missing 'simp' parameter.");
            string trad = Req.Params["trad"];
            if (trad == null) throw new ApiException(400, "Missing 'trad' parameter.");
            string pinyin = Req.Params["pinyin"];
            if (pinyin == null) throw new ApiException(400, "Missing 'pinyin' parameter.");

            Result res = new Result();
            res.Passed = true;

            if (simp == "大家" || simp == "污染") res.Passed = false;

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