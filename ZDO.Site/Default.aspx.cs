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
        private bool isMobile = false;

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
            soaBox.Visible = false;
            bool isStaticQuery = Request.RawUrl.StartsWith("/search/");
            string qlang = Request["lang"];
            string query = Request["query"];
            if (query != null) query = query.Trim();
            // No query: show welcome screen, and we're done.
            if (string.IsNullOrEmpty(query))
            {
                resultsHolder.Visible = false;
                welcomeScreen.Visible = true;
                welcomeScreen.InnerHtml = TextProvider.Instance.GetSnippet(Master.UILang, "welcome");
                Title = TextProvider.Instance.GetString(Master.UILang, "TitleMain");
                // Seed walkthrough
                Master.SetStaticQuery(null, SearchLang.Chinese);
                return;
            }

            // From here on ---> lookup
            string strMobile = Request["mobile"];
            isMobile = strMobile == "yes";

            // Auto-add "mobile" class to body if we know it already
            if (isMobile) Master.SetMobile();

            queryInfo = new QueryInfo(Request.UserHostAddress, query);
            resultsHolder.Visible = true;
            welcomeScreen.Visible = false;
            SearchLang slang = qlang == "trg" ? SearchLang.Target : SearchLang.Chinese;
            var lr = Global.Dict.Lookup(query, SearchScript.Both, slang);

            queryInfo.ResCount = lr.Results.Count;
            queryInfo.Lang = lr.ActualSearchLang;
            queryInfo.DTLookup = DateTime.UtcNow;
            prov = lr.EntryProvider;
            for (int i = 0; i != lr.Results.Count; ++i)
            {
                if (i >= 256) break;
                var res = lr.Results[i];
                OneResultCtrl resCtrl = new OneResultCtrl(res, lr.EntryProvider, Master.UiScript, Master.UiTones, isMobile);
                resultsHolder.Controls.Add(resCtrl);
            }
            txtSearch.Value = query;
            // No results
            if (lr.Results.Count == 0)
            {
                resultsHolder.Visible = false;
                welcomeScreen.Visible = true;
                welcomeScreen.InnerHtml = TextProvider.Instance.GetSnippet(Master.UILang, "noresults");
                Title = TextProvider.Instance.GetString(Master.UILang, "TitleMain");
            }
            // We got results
            else
            {
                // Page title
                string title;
                if (lr.ActualSearchLang == SearchLang.Chinese)
                    title = TextProvider.Instance.GetString(Master.UILang, "TitleSearchChinese");
                else
                    title = TextProvider.Instance.GetString(Master.UILang, "TitleSearchGerman");
                title = string.Format(title, query);
                Title = title;
                // SOA BOX
                soaBox.Visible = true;
                soaTitle.InnerText = TextProvider.Instance.GetString(Master.UILang, "AnimPopupTitle");
                string attrLink = "<a href='https://github.com/skishore/makemeahanzi' target='_blank'>{0}</a>";
                attrLink = string.Format(attrLink, TextProvider.Instance.GetString(Master.UILang, "AnimPopupMMAH"));
                string attrHtml = TextProvider.Instance.GetString(Master.UILang, "AnimPopupAttr");
                attrHtml = string.Format(attrHtml, attrLink);
                soaFooter.InnerHtml = attrHtml;
                // Seed walkthrough
                Master.SetStaticQuery(query, slang);
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
                QueryLogger.Instance.LogQuery(queryInfo.HostAddr, isMobile,
                    Master.UILang, Master.UiScript, Master.UiTones,
                    queryInfo.ResCount, (int)tsLookup.TotalMilliseconds, (int)tsTotal.TotalMilliseconds,
                    queryInfo.Lang, queryInfo.Query);
            }
        }
    }
}