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
            // Server-side inline localization
            strokeClear.InnerText = TextProvider.Instance.GetString(Master.UILang, "BtnStrokeClear");
            strokeUndo.InnerText = TextProvider.Instance.GetString(Master.UILang, "BtnStrokeUndo");
            txtSearch.Attributes["placeholder"] = TextProvider.Instance.GetString(Master.UILang, "TxtSearchPlacholder");

            resultsHolder.Visible = false;
            string query = Request["query"];
            if (query != null) query = query.Trim();
            if (string.IsNullOrEmpty(query))
            {
                resultsHolder.Visible = false;
                welcomeScreen.Visible = true;
                welcomeScreen.InnerHtml = TextProvider.Instance.GetSnippet(Master.UILang, "welcome");
                Title = TextProvider.Instance.GetString(Master.UILang, "TitleMain");
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
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            if (prov != null) prov.Dispose();
        }
    }
}