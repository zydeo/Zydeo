using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ZD.Common;

namespace Site
{
    public partial class OneResultCtrl : System.Web.UI.UserControl
    {
        private readonly CedictResult res;
        private readonly ICedictEntryProvider prov;

        public OneResultCtrl()
        { }

        public OneResultCtrl(CedictResult res, ICedictEntryProvider prov)
        {
            this.res = res;
            this.prov = prov;
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void Render(HtmlTextWriter writer)
        {
            CedictEntry entry = prov.GetEntry(res.EntryId);

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "entry");
            writer.RenderBeginTag(HtmlTextWriterTag.Div); // <div class="entry">

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "hw-simp");
            writer.RenderBeginTag(HtmlTextWriterTag.Span); // <span class="hw-simp">
            writer.WriteEncodedText(entry.ChSimpl);
            writer.RenderEndTag(); // <span class="hw-simp">
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "hw-sep");
            writer.RenderBeginTag(HtmlTextWriterTag.Span); // <span class="hw-sep">
            writer.WriteEncodedText("•");
            writer.RenderEndTag(); // <span class="hw-sep">
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "hw-trad");
            writer.RenderBeginTag(HtmlTextWriterTag.Span); // <span class="hw-trad">
            writer.WriteEncodedText(entry.ChTrad);
            writer.RenderEndTag(); // <span class="hw-trad">
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "hw-pinyin");
            writer.RenderBeginTag(HtmlTextWriterTag.Span); // <span class="hw-pinyin">
            bool firstSyll = true;
            foreach (var pinyin in entry.Pinyin)
            {
                if (!firstSyll) writer.WriteEncodedText(" ");
                firstSyll = false;
                writer.WriteEncodedText(pinyin.GetDisplayString(true));
            }
            writer.RenderEndTag(); // <span class="hw-pinyin">

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "senses");
            writer.RenderBeginTag(HtmlTextWriterTag.Div); // <div class="senses">
            for (int i = 0; i != entry.SenseCount; ++i)
            {
                renderSense(writer, entry.GetSenseAt(i), i, res.TargetHilites);
            }
            writer.RenderEndTag(); // <div class="senses">

            writer.RenderEndTag(); // <div class="entry">
        }

        private static string[] ixStrings = new string[]
        {
            "1", "2", "3", "4", "5", "6", "7", "8", "9",
            "a", "b", "c", "d", "e", "f", "g", "h", "i",
            "j", "k", "l", "m", "n", "o", "p", "q", "r",
            "s", "t", "u", "v", "w", "x", "y", "z"
        };

        private static string getIxString(int ix)
        {
            return ixStrings[ix % 35];
        }

        private static string[] splitFirstWord(string str)
        {
            int i = str.IndexOf(' ');
            if (i == -1) return new string[] { str };
            string[] res = new string[2];
            res[0] = str.Substring(0, i);
            res[1] = str.Substring(i);
            return res;
        }

        private void renderSense(HtmlTextWriter writer, CedictSense sense, int ix, IEnumerable<CedictTargetHighlight> hls)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sense");
            writer.RenderBeginTag(HtmlTextWriterTag.Span); // <span class="sense">
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sense-nobr");
            writer.RenderBeginTag(HtmlTextWriterTag.Span); // <span class="sense-nobr">
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sense-ix");
            writer.RenderBeginTag(HtmlTextWriterTag.Span); // <span class="sense-ix">
            writer.WriteEncodedText(getIxString(ix));
            writer.RenderEndTag(); // <span class="sense-ix">
            writer.WriteEncodedText(" ");

            bool needToSplit = true;
            string domain = sense.Domain.GetPlainText();
            string equiv = sense.Equiv.GetPlainText();
            string note = sense.Note.GetPlainText();
            if (domain != string.Empty)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "sense-meta");
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                string[] firstAndRest = splitFirstWord(domain);
                writer.WriteEncodedText(firstAndRest[0]);
                writer.RenderEndTag(); // sense-meta
                writer.RenderEndTag(); // sense-nobr
                if (firstAndRest.Length > 1)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "sense-meta");
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.WriteEncodedText(firstAndRest[1]);
                    writer.RenderEndTag(); // sense-meta
                }
                needToSplit = false;
            }
            if (equiv != string.Empty)
            {
                if (domain != string.Empty) writer.WriteEncodedText(" ");
                if (needToSplit)
                {
                    string[] firstAndRest = splitFirstWord(equiv);
                    writer.WriteEncodedText(firstAndRest[0]);
                    writer.RenderEndTag(); // sense-nobr
                    if (firstAndRest.Length > 1) writer.WriteEncodedText(firstAndRest[1]);
                    needToSplit = false;
                }
                else writer.WriteEncodedText(equiv);
            }
            if (note != string.Empty)
            {
                if (domain != string.Empty || equiv != string.Empty) writer.WriteEncodedText(" ");
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "sense-meta");
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                if (needToSplit)
                {
                    string[] firstAndRest = splitFirstWord(note);
                    writer.WriteEncodedText(firstAndRest[0]);
                    writer.RenderEndTag(); // sense-meta
                    writer.RenderEndTag(); // sense-nobr
                    if (firstAndRest.Length > 1)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "sense-meta");
                        writer.RenderBeginTag(HtmlTextWriterTag.Span);
                        writer.WriteEncodedText(firstAndRest[1]);
                        writer.RenderEndTag(); // sense-meta
                    }
                    needToSplit = false;
                }
                else
                {
                    writer.WriteEncodedText(note);
                    writer.RenderEndTag(); // sense-meta
                }
            }

            writer.RenderEndTag(); // <span class="sense">

        }
    }
}