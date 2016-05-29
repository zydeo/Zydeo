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
    [ActionName("history_commententry")]
    public class AHistCommentEntry : ApiAction
    {
        /// <summary>
        /// Ctor: init. Boilerplate.
        /// </summary>
        public AHistCommentEntry(HttpContext ctxt) : base(ctxt) { }

        [DataContract]
        public class Result
        {
        }

        public override void Process()
        {
            string strId = Req.Params["entry_id"];
            if (strId == null) throw new ApiException(400, "Missing 'entry_id' parameter.");
            int entryId = int.Parse(strId);

            // We done; result is a dummy.
            Res = new Result();
        }
    }
}