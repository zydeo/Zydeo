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
            if (query == null)
            {
                resultsHolder.Visible = false;
                welcomeScreen.Visible = true;
            }
            else
            {
                resultsHolder.Visible = true;
                welcomeScreen.Visible = false;
                var lr = Global.Dict.Lookup(query, SearchScript.Both, SearchLang.Chinese);
                prov = lr.EntryProvider;
                for (int i = 0; i != lr.Results.Count; ++i)
                {
                    if (i >= 256) break;
                    var res = lr.Results[i];
                    OneResultCtrl resCtrl = new OneResultCtrl(res, lr.EntryProvider);
                    resultsHolder.Controls.Add(resCtrl);
                }
                txtSearch.Value = query;
            }
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            if (prov != null) prov.Dispose();
        }
    }
}