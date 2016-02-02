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
        private bool isStaticQuery = false;

        private class QueryInfo
        {
            public readonly DateTime DTStart = DateTime.UtcNow;
            public readonly string HostAddr;
            public readonly string Query;
            public DateTime DTLookup = DateTime.UtcNow;
            public int ResCount = -1;
            public int AnnCount = -1;
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
            // Only do this manually, and offline: generate sitemaps
            //SitemapGenerator.Generate();


            var tprov = TextProvider.Instance;

            // Server-side inline localization
            strokeClear.InnerText = tprov.GetString(Master.UILang, "BtnStrokeClear");
            strokeUndo.InnerText = tprov.GetString(Master.UILang, "BtnStrokeUndo");
            txtSearch.Attributes["placeholder"] = tprov.GetString(Master.UILang, "TxtSearchPlacholder");

            resultsHolder.Visible = false;
            soaBox.Visible = false;
            isStaticQuery = Request.RawUrl.StartsWith("/search/");
            string qlang = Request["lang"];
            string query = Request["query"];
            if (query != null) query = query.Trim();
            // No query: show welcome screen, and we're done.
            if (string.IsNullOrEmpty(query))
            {
                resultsHolder.Visible = false;
                welcomeScreen.Visible = true;
                welcomeScreen.InnerHtml = tprov.GetSnippet(Master.UILang, "welcome");
                Title = tprov.GetString(Master.UILang, "TitleMain");
                // Seed walkthrough
                Master.SetStaticQuery(null, SearchLang.Chinese, DateTime.UtcNow);
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
            queryInfo.AnnCount = lr.Annotations.Count;
            queryInfo.Lang = lr.ActualSearchLang;
            queryInfo.DTLookup = DateTime.UtcNow;
            prov = lr.EntryProvider;
            // Add regular results
            for (int i = 0; i != lr.Results.Count; ++i)
            {
                if (i >= 256) break;
                var res = lr.Results[i];
                OneResultCtrl resCtrl = new OneResultCtrl(res, lr.EntryProvider, Master.UiScript, Master.UiTones, isMobile);
                resultsHolder.Controls.Add(resCtrl);
            }
            // Add annotations (we never get both, so don't need to worry about the order)
            for (int i = 0; i != lr.Annotations.Count; ++i)
            {
                var ann = lr.Annotations[i];
                OneResultCtrl resCtrl = new OneResultCtrl(lr.Query, ann, lr.EntryProvider, Master.UiTones, isMobile);
                resultsHolder.Controls.Add(resCtrl);
            }
            txtSearch.Value = query;
            // No results
            if (lr.Results.Count == 0 && lr.Annotations.Count == 0)
            {
                resultsHolder.Visible = false;
                welcomeScreen.Visible = true;
                welcomeScreen.InnerHtml = tprov.GetSnippet(Master.UILang, "noresults");
                Title = tprov.GetString(Master.UILang, "TitleMain");
            }
            // We got results
            else
            {
                // Page title
                string title;
                // Regular lookup results
                if (lr.Results.Count != 0)
                {
                    if (lr.ActualSearchLang == SearchLang.Chinese)
                        title = tprov.GetString(Master.UILang, "TitleSearchChinese");
                    else
                        title = tprov.GetString(Master.UILang, "TitleSearchGerman");
                }
                // Annotation
                else title = tprov.GetString(Master.UILang, "TitleSearchAnnotation");
                title = string.Format(title, query);
                Title = title;
                // For annotatio mode, show notice at top
                if (lr.Annotations.Count != 0)
                {
                    topNotice.Visible = true;
                    tnTitle.InnerText = tprov.GetString(Master.UILang, "AnnotationTitle");
                    tnMessage.InnerText = tprov.GetString(Master.UILang, "AnnotationMessage");
                }
                // SOA BOX
                soaBox.Visible = true;
                soaTitle.InnerText = tprov.GetString(Master.UILang, "AnimPopupTitle");
                string attrLink = "<a href='https://github.com/skishore/makemeahanzi' target='_blank'>{0}</a>";
                attrLink = string.Format(attrLink, tprov.GetString(Master.UILang, "AnimPopupMMAH"));
                string attrHtml = tprov.GetString(Master.UILang, "AnimPopupAttr");
                attrHtml = string.Format(attrHtml, attrLink);
                soaFooter.InnerHtml = attrHtml;
                // Seed walkthrough - if query is static and we have regular results (not annotations)
                if (isStaticQuery && lr.Results.Count != 0) Master.SetStaticQuery(query, slang, queryInfo.DTStart);
            }
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            // If lookup returned a results provider, dispose it
            if (prov != null) prov.Dispose();
            // If we had a POST query, log it
            if (queryInfo != null && !isStaticQuery)
            {
                TimeSpan tsLookup = queryInfo.DTLookup.Subtract(queryInfo.DTStart);
                TimeSpan tsTotal = DateTime.UtcNow.Subtract(queryInfo.DTStart);
                QueryLogger.SearchMode smode = QueryLogger.SearchMode.Target;
                if (queryInfo.Lang == SearchLang.Target) smode = QueryLogger.SearchMode.Target;
                else if (queryInfo.ResCount > 0) smode = QueryLogger.SearchMode.Source;
                else if (queryInfo.AnnCount > 0) smode = QueryLogger.SearchMode.Annotate;
                int cnt = smode == QueryLogger.SearchMode.Annotate ? queryInfo.AnnCount : queryInfo.ResCount;
                QueryLogger.Instance.LogQuery(queryInfo.HostAddr, isMobile,
                    Master.UILang, Master.UiScript, Master.UiTones,
                    cnt, (int)tsLookup.TotalMilliseconds, (int)tsTotal.TotalMilliseconds,
                    smode, queryInfo.Query);
            }
        }
    }
}