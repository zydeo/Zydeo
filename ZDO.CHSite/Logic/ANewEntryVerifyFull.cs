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
            CedictEntry entry = Global.HWInfo.ParseFromText(can);
            StringBuilder sb = new StringBuilder();
            using (HtmlTextWriter writer = new HtmlTextWriter(new StringWriter(sb)))
            {
                EntryRenderer er = new EntryRenderer(entry, null, null);
                er.Render(writer);
            }
            res.Preview = sb.ToString();

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