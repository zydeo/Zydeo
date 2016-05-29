using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

using ZD.Common;

namespace ZDO.CHSite
{
    public partial class ADynPage
    {
        [DataContract]
        public class SearchResult : Result
        {
            [DataMember(Name = "query")]
            public string Query = "";
        }

        private void doSearch(string lang, string rel)
        {
            // No search query in URL: show welcome page.
            if (rel == "")
            {
                Res = makeWelcomeResult(lang);
                return;
            }

            // Seach's special parameters
            string strScript = Req.Params["searchScript"];
            string strTones = Req.Params["searchTones"];
            UiScript uiScript = UiScript.Both;
            if (strScript == "simp") uiScript = UiScript.Simp;
            else if (strScript == "trad") uiScript = UiScript.Trad;
            UiTones uiTones = UiTones.Pleco;
            if (strTones == "dummitt") uiTones = UiTones.Dummitt;
            else if (strTones == "none") uiTones = UiTones.None;

            // Perform query
            string query = rel.Replace("search/", "");
            query = HttpUtility.UrlDecode(query);
            CedictLookupResult lr;
            using (SqlDict.Query q = new SqlDict.Query())
            {
                lr = q.Lookup(query);
            }
            // No results
            if (lr.Results.Count == 0 && lr.Annotations.Count == 0)
            {
                return;
            }
            // Render results
            var prov = lr.EntryProvider;
            StringBuilder sb = new StringBuilder();
            using (HtmlTextWriter writer = new HtmlTextWriter(new StringWriter(sb)))
            {
                int max = Math.Min(lr.Results.Count, 256);
                for (int i = 0; i != max; ++i)
                {
                    var lres = lr.Results[i];
                    EntryRenderer er = new EntryRenderer(lres, prov, uiScript, uiTones);
                    er.OneLineHanziLimit = 9;
                    er.Render(writer);
                    if (i != max - 1)
                    {
                        writer.AddAttribute("class", "resultSep");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div);
                        writer.RenderEndTag();
                    }
                }
            }
            // Assemble HTML for result
            StringBuilder sbHtml;
            string title, keywords, description;
            readFile(getFileName(lang, "_search"), out sbHtml, out title, out keywords, out description);
            sbHtml.Replace("<!-- RESULTS -->", sb.ToString());
            // Special treatment of metainfo
            title = TextProvider.Instance.GetString(lang, "TitleSearchZho");
            title = string.Format(title, lr.Query);
            // Build result
            SearchResult res = new SearchResult();
            res.Html = sbHtml.ToString();
            res.Title = title;
            res.Keywords = keywords;
            res.Description = description;
            res.Query = lr.Query;
            Res = res;
        }
    }
}