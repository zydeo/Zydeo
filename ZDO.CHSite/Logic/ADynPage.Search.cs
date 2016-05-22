using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

using ZD.Common;

namespace ZDO.CHSite
{
    public partial class ADynPage
    {
        private void doSearch(string lang, string rel)
        {
            if (rel == "")
            {
                string fname = getFileName(lang, "_welcome");
                feedResult(fname);
                return;
            }

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
                for (int i = 0; i != lr.Results.Count; ++i)
                {
                    if (i >= 256) break;
                    var lres = lr.Results[i];
                    EntryRenderer er = new EntryRenderer(lres, prov, UiScript.Both, UiTones.Pleco);
                    er.Render(writer);
                }
            }
            StringBuilder sbHtml;
            string title, keywords, description;
            readFile(getFileName(lang, "_search"), out sbHtml, out title, out keywords, out description);
            sbHtml.Replace("<!-- RESULTS -->", sb.ToString());
            Result res = new Result();
            res.Html = sbHtml.ToString();
            res.Title = title;
            res.Keywords = keywords;
            res.Description = description;
            Res = res;
        }
    }
}