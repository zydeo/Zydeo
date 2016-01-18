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
        private readonly UiScript script;
        private readonly UiTones tones;
        private readonly bool isMobile;

        public OneResultCtrl()
        { }

        public OneResultCtrl(CedictResult res, ICedictEntryProvider prov,
            UiScript script, UiTones tones, bool isMobile)
        {
            this.res = res;
            this.prov = prov;
            this.script = script;
            this.tones = tones;
            this.isMobile = isMobile;
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void Render(HtmlTextWriter writer)
        {
            int hanziLimit = isMobile ? 4 : 6;
            CedictEntry entry = prov.GetEntry(res.EntryId);

            string entryClass = "entry";
            if (tones == UiTones.Pleco) entryClass += " toneColorsPleco";
            else if (tones == UiTones.Dummitt) entryClass += " toneColorsDummitt";
            writer.AddAttribute(HtmlTextWriterAttribute.Class, entryClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Div); // <div class="entry">

            if (script != UiScript.Trad)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "hw-simp");
                writer.RenderBeginTag(HtmlTextWriterTag.Span); // <span class="hw-simp">
                renderHanzi(entry, true, false, writer);
                writer.RenderEndTag(); // <span class="hw-simp">
            }
            if (script == UiScript.Both)
            {
                // Up to 6 hanzi: on a single line
                if (entry.ChSimpl.Length <= hanziLimit)
                {
                    string clsSep = "hw-sep";
                    if (tones != UiTones.None) clsSep = "hw-sep faint";
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, clsSep);
                    writer.RenderBeginTag(HtmlTextWriterTag.Span); // <span class="hw-sep">
                    writer.WriteEncodedText("•");
                    writer.RenderEndTag(); // <span class="hw-sep">
                }
                // Otherwise, line break
                else
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Br);
                    writer.RenderEndTag();
                }
            }
            if (script != UiScript.Simp)
            {
                string clsTrad = "hw-trad";
                // Need special class so traditional floats left after line break
                if (script == UiScript.Both && entry.ChSimpl.Length > hanziLimit)
                    clsTrad = "hw-trad break";
                writer.AddAttribute(HtmlTextWriterAttribute.Class, clsTrad);
                writer.RenderBeginTag(HtmlTextWriterTag.Span); // <span class="hw-trad">
                renderHanzi(entry, false, script == UiScript.Both, writer);
                writer.RenderEndTag(); // <span class="hw-trad">
            }
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

        private void renderHanzi(CedictEntry entry, bool simp, bool faintIdentTrad, HtmlTextWriter writer)
        {
            string hzStr = simp ? entry.ChSimpl : entry.ChTrad;
            for (int i = 0; i != hzStr.Length; ++i)
            {
                char c = hzStr[i];
                int pyIx = entry.HanziPinyinMap[i];
                PinyinSyllable py = null;
                if (pyIx != -1) py = entry.Pinyin[pyIx];
                // Class to put on hanzi
                string cls = "";
                // We mark up tones if needed
                if (tones != UiTones.None && py != null)
                {
                    if (py.Tone == 1) cls = "tone1";
                    else if (py.Tone == 2) cls = "tone2";
                    else if (py.Tone == 3) cls = "tone3";
                    else if (py.Tone == 4) cls = "tone4";
                    // -1 for unknown, and 0 for neutral: we don't mark up anything
                }
                // If we're rendering both scripts, then show faint traditional chars where same as simp
                if (faintIdentTrad && c == entry.ChSimpl[i]) cls += " faint";
                // Mark up existence of stroke order animation
                cls += " hanim";
                // Render with enclosing span if we have a relevant class
                if (!string.IsNullOrEmpty(cls))
                {
                    writer.AddAttribute("class", cls);
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                }
                writer.WriteEncodedText(c.ToString());
                if (!string.IsNullOrEmpty(cls)) writer.RenderEndTag();
            }
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