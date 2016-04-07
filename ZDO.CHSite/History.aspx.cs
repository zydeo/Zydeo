using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Configuration;
using System.Text;

namespace ZDO.CHSite
{
    public partial class History : System.Web.UI.Page
    {
        private int pageSize = -1;
        private int pageIX = -1;
        private int pageCount = 1;
        private List<SqlDict.ChangeItem> changes;

        private void loadData()
        {
            // Page size: from private config
            AppSettingsReader asr = new AppSettingsReader();
            pageSize = (int)asr.GetValue("historyPageSize", typeof(int));

            // Page ID from request?
            string pageId = Request.Params["page"];
            if (pageId != null) int.TryParse(pageId, out pageIX);
            if (pageIX == -1) pageIX = 0;
            else --pageIX; // Humans count from 1

            // Retrieve data from DB
            using (SqlDict.History hist = new SqlDict.History())
            {
                pageCount = hist.GetChangeCount() / pageSize + 1;
                changes = hist.GetChangePage(pageIX * pageSize, pageSize);
            }
        }

        private void addOneLink(int ix)
        {
            string astr = "<a href='{0}' class='{1}'>{2}</a>\r\n";
            string strClass = "pageLink";
            if (ix == pageIX) strClass += " selected";
            string strUrl = "/" + Master.Lang + "/edit/history";
            if (ix > 0) strUrl += "/" + (ix + 1).ToString();
            string strText = (ix + 1).ToString();
            astr = string.Format(astr, strUrl, strClass, strText);
            litLinks.Text += astr;
        }

        private void buildPageLinks()
        {
            // Two main strategies. Not more than 10 page links: throw them all in.
            // Otherwise, improvise gaps; pattern:
            // 1 2 ... (n-1) *n* (n+1) ... (max-1) (max)
            // Omit gap if no numbers are skipped
            int lastRenderedIX = 0;
            for (int i = 0; i != pageCount; ++i)
            {
                // Few pages: dump all
                if (pageCount < 11)
                {
                    addOneLink(i);
                    continue;
                }
                // Otherwise: get smart
                // 1, 2,  (n-1), n, (n+1),  (max-1), (max) only
                if (i == 0 || i == 1 || i == pageCount - 2 || i == pageCount - 1 ||
                    i == pageIX - 1 || i == pageIX || i == pageIX + 1)
                {
                    // If we just skipped a page, render dot-dot-dot
                    if (i > lastRenderedIX + 1)
                    {
                        string strSpan = "<span class='pageSpacer'>&middot; &middot; &middot;</span>\r\n";
                        litLinks.Text += strSpan;
                    }
                    // Render page link
                    addOneLink(i);
                    // Remember last rendered
                    lastRenderedIX = i;
                    continue;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Add CSS files
            Master.AddCss("style.css");
            Master.AddCss("history.css");
            Master.AddCss("entry.css");

            // Identify our position; load data
            loadData();
            // Navigation
            buildPageLinks();
            // Add changes
            for (int i = 0; i != changes.Count; ++i)
            {
                SqlDict.ChangeItem ci = changes[i];
                changeList.Controls.Add(new HistoryItem(ci));
            }

            //// Render static content
            //string path = HttpRuntime.AppDomainAppPath;
            //path = Path.Combine(path, "Content");
            //path = Path.Combine(path, "HistoryDoodle.html");
            //using (StreamReader sr = new StreamReader(path))
            //{
            //    lit.Text = sr.ReadToEnd();
            //}
        }
    }
}