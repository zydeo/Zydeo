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
    [ActionName("newentry_verifyhead")]
    public class ANewEntryVerifyHead : ApiAction
    {
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

            // DBG
            if (simp == "大家" || simp == "污染") res.Passed = false;

            // Prepare pinyin as list of proper syllables
            List<PinyinSyllable> pyList = new List<PinyinSyllable>();
            string[] pyRawArr = pinyin.Split(' ');
            foreach (string pyRaw in pyRawArr) pyList.Add(PinyinSyllable.FromDisplayString(pyRaw));
            
            // Return all entries, CEDICT and HanDeDict, rendered as HTML
            CedictEntry[] ced, hdd;
            Global.HWInfo.GetEntries(simp, out ced, out hdd);
            StringBuilder sb = new StringBuilder();
            using (HtmlTextWriter writer = new HtmlTextWriter(new StringWriter(sb)))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, "newEntryRefCED");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                foreach (CedictEntry entry in ced)
                {
                    EntryRenderer er = new EntryRenderer(entry, trad, pyList);
                    er.Render(writer);
                }
                writer.RenderEndTag();
                writer.AddAttribute(HtmlTextWriterAttribute.Id, "newEntryRefHDD");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                foreach (CedictEntry entry in hdd)
                {
                    EntryRenderer er = new EntryRenderer(entry, trad, pyList);
                    er.Render(writer);
                }
                writer.RenderEndTag();
            }
            res.RefEntries = sb.ToString();

            // Tell our caller
            Res = res;
        }
    }
}