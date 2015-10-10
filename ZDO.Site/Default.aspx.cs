using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ZD.Common;

namespace Site
{
    public partial class Default : System.Web.UI.Page
    {
        private ICedictEntryProvider prov = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            resultsHolder.Visible = false;
            string query = Request["query"];
            if (query != null)
            {
                var lr = Global.Dict.Lookup(query, SearchScript.Both, SearchLang.Target);
                prov = lr.EntryProvider;
                resultsHolder.Visible = true;
                foreach (var res in lr.Results)
                {
                    OneResultCtrl resCtrl = new OneResultCtrl(res, lr.EntryProvider);
                    resultsHolder.Controls.Add(resCtrl);
                }
            }
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            if (prov != null) prov.Dispose();
        }
    }
}