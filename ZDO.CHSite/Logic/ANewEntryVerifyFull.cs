using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
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
    [ActionName("newentry_verifyfull")]
    public class ANewEntryVerifyFull : ApiAction
    {
        /// <summary>
        /// Ctor: init. Boilerplate.
        /// </summary>
        public ANewEntryVerifyFull(HttpContext ctxt) : base(ctxt) { }

        [DataContract]
        public class Result
        {
            [DataMember(Name = "passed")]
            public bool Passed;
            [DataMember(Name = "errors")]
            public List<string> Errors;
            [DataMember(Name = "preview_html")]
            public string Preview = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Process()
        {
            // Mucho TO-DO in this action:
            // - Escape slashes in senses
            // - Proper checking for all sorts of stuff

            string simp = Req.Params["simp"];
            if (simp == null) throw new ApiException(400, "Missing 'simp' parameter.");
            string trad = Req.Params["trad"];
            if (trad == null) throw new ApiException(400, "Missing 'trad' parameter.");
            string pinyin = Req.Params["pinyin"];
            if (pinyin == null) throw new ApiException(400, "Missing 'pinyin' parameter.");
            string trg = Req.Params["trg"];
            if (trg == null) throw new ApiException(400, "Missing 'trg' parameter.");

            Result res = new Result();
            res.Passed = true;

            CedictEntry entry = SqlDict.BuildEntry(simp, trad, pinyin, trg);
            StringBuilder sb = new StringBuilder();
            using (HtmlTextWriter writer = new HtmlTextWriter(new StringWriter(sb)))
            {
                EntryRenderer er = new EntryRenderer(entry, null, null);
                er.Render(writer);
            }
            res.Preview = sb.ToString();

            // Tell our caller
            Res = res;
        }
    }
}