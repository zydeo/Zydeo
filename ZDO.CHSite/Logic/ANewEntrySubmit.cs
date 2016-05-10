using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

using ZD.Common;
using ZD.CedictEngine;

namespace ZDO.CHSite
{
    [ActionName("newentry_submit")]
    public class ANewEntrySubmit : ApiAction
    {
        /// <summary>
        /// Ctor: init. Boilerplate.
        /// </summary>
        public ANewEntrySubmit(HttpContext ctxt) : base(ctxt) { }

        [DataContract]
        public class Result
        {
            [DataMember(Name = "success")]
            public bool Success;
        }

        public override void Process()
        {
            string simp = Req.Params["simp"];
            if (simp == null) throw new ApiException(400, "Missing 'simp' parameter.");
            string trad = Req.Params["trad"];
            if (trad == null) throw new ApiException(400, "Missing 'trad' parameter.");
            string pinyin = Req.Params["pinyin"];
            if (pinyin == null) throw new ApiException(400, "Missing 'pinyin' parameter.");
            string trg = Req.Params["trg"];
            if (trg == null) throw new ApiException(400, "Missing 'trg' parameter.");
            string note = Req.Params["note"];
            if (note == null) throw new ApiException(400, "Missing 'note' parameter.");

            Result res = new Result { Success = true };
            SqlDict.SimpleBuilder builder = null;
            try
            {
                builder = new SqlDict.SimpleBuilder(0);
                CedictEntry entry = SqlDict.BuildEntry(simp, trad, pinyin, trg);
                builder.NewEntry(entry, note);
            }
            catch (Exception ex)
            {
                DiagLogger.LogError(ex);
                res.Success = false;
            }
            finally
            {
                if (builder != null) builder.Dispose();
            }

            // Tell our caller
            Res = res;
        }
    }
}