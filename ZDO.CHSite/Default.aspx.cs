using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

using ZD.Common;

namespace ZDO.CHSite
{
    public partial class Default : System.Web.UI.Page
    {
        private ICedictEntryProvider prov = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Add CSS files
            Master.AddCss("tooltipster.css");
            Master.AddCss("style.css");
            Master.AddCss("entry.css");
            Master.AddCss("lookup.css");
            // Add JS includes
            Master.AddJS("jquery.tooltipster.min.js", true);
            Master.AddJS("common.js", false);
            Master.AddJS("lookup.js", false);
            Master.AddJS("strokeanim.js", false);

            if (string.IsNullOrEmpty(Request.Params["query"]))
            {
                loadStatic("Welcome");
                return;
            }

            string query = Request.Params["query"].Replace('+', ' ');
            txtSearch.Value = query;
            CedictLookupResult lr;
            using (SqlDict.Query q = new SqlDict.Query())
            {
                lr = q.Lookup(query);
            }
            // No results
            if (lr.Results.Count == 0 && lr.Annotations.Count == 0)
            {
                loadStatic("NoResults");
                return;
            }
            prov = lr.EntryProvider;
            // Add regular results
            for (int i = 0; i != lr.Results.Count; ++i)
            {
                if (i >= 256) break;
                var res = lr.Results[i];
                OneResultCtrl resCtrl = new OneResultCtrl(res, lr.EntryProvider,
                    Master.UiScript, Master.UiTones, false);
                resultsHolder.Controls.Add(resCtrl);
            }
            // SOA BOX
            soaBox.Visible = true;
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            // If lookup returned a results provider, dispose it
            if (prov != null) prov.Dispose();
            // TO-DO: Log query
        }

        private void loadStatic(string pageName)
        {
            resultsHolder.Visible = false;
            welcomeScreen.Visible = true;
            string path = HttpRuntime.AppDomainAppPath;
            path = Path.Combine(path, "Content");
            path = Path.Combine(path,pageName + ".html");
            using (StreamReader sr = new StreamReader(path))
            {
                this.litWelcomeScreen.Text = sr.ReadToEnd();
            }
        }
    }
}