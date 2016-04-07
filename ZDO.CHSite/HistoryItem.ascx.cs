using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ZD.Common;

namespace ZDO.CHSite
{
    public partial class HistoryItem : System.Web.UI.UserControl
    {
        private readonly SqlDict.ChangeItem ci;

        public HistoryItem() { }

        public HistoryItem(SqlDict.ChangeItem ci)
        {
            this.ci = ci;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.WriteLine();
            writer.AddAttribute("class", "historyItem");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.AddAttribute("class", "changeHead");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.AddAttribute("class", "changeSummary");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.AddAttribute("class", "changeUser");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.WriteEncodedText(ci.User);
            writer.RenderEndTag();

            writer.Write(" &bull; ");

            writer.AddAttribute("class", "changeTime");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            DateTime dt = TimeZoneInfo.ConvertTimeFromUtc(ci.When, Global.TimeZoneInfo);
            string dtFmt = "{0}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}";
            dtFmt = string.Format(dtFmt, dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            writer.WriteEncodedText(dtFmt);
            writer.RenderEndTag();

            writer.Write(" &bull; ");

            string changeMsg;
            string changeCls = "changeType";
            if (ci.ChangeType == SqlDict.ChangeType.New)
            {
                changeMsg = "új szócikk";
                changeCls += " ctNew";
            }
            else if (ci.ChangeType == SqlDict.ChangeType.Edit)
            {
                changeMsg = "szerkesztve";
                changeCls += " ctEdit";
            }
            else if (ci.ChangeType == SqlDict.ChangeType.Note)
            {
                changeMsg = "megjegyzés";
                changeCls += " ctNote";
            }
            else changeMsg = ci.ChangeType.ToString();
            writer.AddAttribute("class", changeCls);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.WriteEncodedText(changeMsg);
            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.AddAttribute("class", "changeNote");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.WriteEncodedText("~~ " + ci.Note);
            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.AddAttribute("class", "entry");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            CedictEntry entry = SqlDict.BuildEntry(ci.EntryHead, ci.EntryBody);
            EntryRenderer er = new EntryRenderer(entry, null, null);
            er.Render(writer);

            writer.RenderEndTag();

            writer.RenderEndTag();
        }

    }
}