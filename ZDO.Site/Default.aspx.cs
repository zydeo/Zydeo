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

        private class QueryInfo
        {
            public readonly DateTime DTStart = DateTime.UtcNow;
            public readonly string HostAddr;
            public readonly string Query;
            public DateTime DTLookup = DateTime.UtcNow;
            public int ResCount = -1;
            public SearchLang Lang;
            public QueryInfo(string hostAddr, string query)
            {
                HostAddr = hostAddr;
                Query = query;
            }
        }

        private QueryInfo queryInfo = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Server-side inline localization
            strokeClear.InnerText = TextProvider.Instance.GetString(Master.UILang, "BtnStrokeClear");
            strokeUndo.InnerText = TextProvider.Instance.GetString(Master.UILang, "BtnStrokeUndo");
            txtSearch.Attributes["placeholder"] = TextProvider.Instance.GetString(Master.UILang, "TxtSearchPlacholder");

            resultsHolder.Visible = false;
            string query = Request["query"];
            if (query != null) query = query.Trim();
            // No query: show welcome screen, and we're done.
            if (string.IsNullOrEmpty(query))
            {
                resultsHolder.Visible = false;
                welcomeScreen.Visible = true;
                welcomeScreen.InnerHtml = TextProvider.Instance.GetSnippet(Master.UILang, "welcome");
                Title = TextProvider.Instance.GetString(Master.UILang, "TitleMain");
                return;
            }

            // From here on ---> lookup
            queryInfo = new QueryInfo(Request.UserHostAddress, query);
            resultsHolder.Visible = true;
            welcomeScreen.Visible = false;
            var lr = Global.Dict.Lookup(query, SearchScript.Both, SearchLang.Chinese);

            queryInfo.ResCount = lr.Results.Count;
            queryInfo.Lang = lr.ActualSearchLang;
            queryInfo.DTLookup = DateTime.UtcNow;
            prov = lr.EntryProvider;
            for (int i = 0; i != lr.Results.Count; ++i)
            {
                if (i >= 256) break;
                var res = lr.Results[i];
                OneResultCtrl resCtrl = new OneResultCtrl(res, lr.EntryProvider);
                resultsHolder.Controls.Add(resCtrl);
            }
            txtSearch.Value = query;
            if (lr.Results.Count == 0)
                Title = TextProvider.Instance.GetString(Master.UILang, "TitleMain");
            else
            {
                string title;
                if (lr.ActualSearchLang == SearchLang.Chinese)
                    title = TextProvider.Instance.GetString(Master.UILang, "TitleSearchChinese");
                else
                    title = TextProvider.Instance.GetString(Master.UILang, "TitleSearchGerman");
                title = string.Format(title, query);
                Title = title;
            }
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            // If lookup returned a results provider, dispose it
            if (prov != null) prov.Dispose();
            // If we had a query, log it
            if (queryInfo != null)
            {
                TimeSpan tsLookup = queryInfo.DTLookup.Subtract(queryInfo.DTStart);
                TimeSpan tsTotal = DateTime.UtcNow.Subtract(queryInfo.DTStart);
                QueryLogger.Instance.LogQuery(queryInfo.HostAddr, queryInfo.ResCount,
                    (int)tsLookup.TotalMilliseconds, (int)tsTotal.TotalMilliseconds,
                    queryInfo.Lang, queryInfo.Query);
            }
        }
    }
}