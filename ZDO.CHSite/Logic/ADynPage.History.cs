using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.IO;
using System.Configuration;
using System.Text;

using ZD.Common;

namespace ZDO.CHSite
{
    public partial class ADynPage
    {
        private int histPageSize = -1;
        private int histPageIX = -1;
        private int histPageCount = 1;
        private List<SqlDict.ChangeItem> histChanges;

        private void doHistory(string lang, string rel)
        {
            // Diagnostics/doodling
            if (rel == "edit/history/x")
            {
                Res = makeResult(getFileName("en", "_historydoodle"));
                return;
            }

            // Load history to show on this page
            loadHistory(rel);
            // Navigation
            string histLinks = buildHistoryLinks(lang);
            // Add changes
            StringBuilder sbChanges = new StringBuilder();
            using (HtmlTextWriter wrChanges = new HtmlTextWriter(new StringWriter(sbChanges)))
            {
                for (int i = 0; i != histChanges.Count; ++i)
                {
                    SqlDict.ChangeItem ci = histChanges[i];
                    histRenderChange(wrChanges, ci, i != histChanges.Count - 1);
                }
            }

            // Render full page
            StringBuilder sbHtml;
            string title, keywords, description;
            readFile(getFileName(lang, "edit.history"), out sbHtml, out title, out keywords, out description);
            sbHtml.Replace("<!-- PAGELINKS -->", histLinks);
            sbHtml.Replace("<!-- CHANGELIST -->", sbChanges.ToString());
            Result res = new Result();
            res.Html = sbHtml.ToString();
            res.Title = title;
            res.Keywords = keywords;
            res.Description = description;
            Res = res;
        }

        private void loadHistory(string rel)
        {
            // Page size: from private config
            AppSettingsReader asr = new AppSettingsReader();
            histPageSize = (int)asr.GetValue("historyPageSize", typeof(int));

            // Page ID from URL
            rel = rel.Replace("edit/history", "");
            if (rel.StartsWith("/")) rel = rel.Substring(1);
            rel = rel.Replace("page-", "");
            if (rel != "") int.TryParse(rel, out histPageIX);
            if (histPageIX == -1) histPageIX = 0;
            else --histPageIX; // Humans count from 1

            // Retrieve data from DB
            using (SqlDict.History hist = new SqlDict.History())
            {
                histPageCount = hist.GetChangeCount() / histPageSize + 1;
                histChanges = hist.GetChangePage(histPageIX * histPageSize, histPageSize);
            }
        }

        private string buildHistPageLink(string lang, int ix)
        {
            string astr = "<a href='{0}' class='{1}'>{2}</a>\r\n";
            string strClass = "pageLink ajax";
            if (ix == histPageIX) strClass += " selected";
            string strUrl = "/" + lang + "/edit/history";
            if (ix > 0) strUrl += "/page-" + (ix + 1).ToString();
            string strText = (ix + 1).ToString();
            astr = string.Format(astr, strUrl, strClass, strText);
            return astr;
        }

        private string buildHistoryLinks(string lang)
        {
            StringBuilder sb = new StringBuilder();
            // Two main strategies. Not more than 10 page links: throw them all in.
            // Otherwise, improvise gaps; pattern:
            // 1 2 ... (n-1) *n* (n+1) ... (max-1) (max)
            // Omit gap if no numbers are skipped
            int lastRenderedIX = 0;
            for (int i = 0; i != histPageCount; ++i)
            {
                // Few pages: dump all
                if (histPageCount < 11)
                {
                    sb.Append(buildHistPageLink(lang, i));
                    continue;
                }
                // Otherwise: get smart
                // 1, 2,  (n-1), n, (n+1),  (max-1), (max) only
                if (i == 0 || i == 1 || i == histPageCount - 2 || i == histPageCount - 1 ||
                    i == histPageIX - 1 || i == histPageIX || i == histPageIX + 1)
                {
                    // If we just skipped a page, render dot-dot-dot
                    if (i > lastRenderedIX + 1)
                    {
                        string strSpan = "<span class='pageSpacer'>&middot; &middot; &middot;</span>\r\n";
                        sb.Append(strSpan);
                    }
                    // Render page link
                    sb.Append(buildHistPageLink(lang, i));
                    // Remember last rendered
                    lastRenderedIX = i;
                    continue;
                }
            }
            return sb.ToString();
        }

        private void histRenderChange(HtmlTextWriter writer, SqlDict.ChangeItem ci, bool trailingSeparator)
        {
            writer.WriteLine();
            writer.AddAttribute("class", "historyItem");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.AddAttribute("class", "changeHead");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.AddAttribute("class", "fa fa-lightbulb-o ctNew");
            writer.RenderBeginTag(HtmlTextWriterTag.I);
            writer.RenderEndTag();

            writer.AddAttribute("class", "changeSummary");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            string changeMsg;
            string changeCls = "changeType";
            if (ci.ChangeType == SqlDict.ChangeType.New) changeMsg = "Új szócikk";
            else if (ci.ChangeType == SqlDict.ChangeType.Edit) changeMsg = "Szerkesztve";
            else if (ci.ChangeType == SqlDict.ChangeType.Note) changeMsg = "Megjegyzés";
            else changeMsg = ci.ChangeType.ToString();
            changeMsg += ": ";
            writer.AddAttribute("class", changeCls);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.WriteEncodedText(changeMsg);
            writer.RenderEndTag();

            writer.AddAttribute("class", "changeUser");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.WriteEncodedText(ci.User);
            writer.RenderEndTag();

            writer.Write(" &bull; ");

            writer.AddAttribute("class", "changeTime");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            DateTime dt = TimeZoneInfo.ConvertTimeFromUtc(ci.When, Global.TimeZoneInfo);
            string dtFmt = "{0}-{1:00}-{2:00} {3:00}:{4:00}";
            dtFmt = string.Format(dtFmt, dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute);
            writer.WriteEncodedText(dtFmt);
            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.AddAttribute("class", "changeNote");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.WriteEncodedText(ci.Note);
            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.AddAttribute("class", "changeEntry");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.AddAttribute("class", "histEntryOps");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.AddAttribute("class", "opHistEdit fa fa-pencil-square-o");
            writer.RenderBeginTag(HtmlTextWriterTag.I);
            writer.RenderEndTag();
            writer.AddAttribute("class", "opHistComment fa fa-commenting-o");
            writer.RenderBeginTag(HtmlTextWriterTag.I);
            writer.RenderEndTag();
            writer.AddAttribute("class", "opHistFlag fa fa-flag-o");
            writer.RenderBeginTag(HtmlTextWriterTag.I);
            writer.RenderEndTag();
            writer.RenderEndTag();

            CedictEntry entry = SqlDict.BuildEntry(ci.EntryHead, ci.EntryBody);
            EntryRenderer er = new EntryRenderer(entry);
            er.OneLineHanziLimit = 12;
            er.Render(writer);

            writer.RenderEndTag();

            writer.RenderEndTag();

            if (trailingSeparator)
            {
                writer.AddAttribute("class", "historySep");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.RenderEndTag();
            }
        }
    }
}