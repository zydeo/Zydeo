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
        private static bool hanim = true;

        private readonly string query;
        private readonly CedictResult res;
        private readonly CedictAnnotation ann;
        private readonly ICedictEntryProvider prov;
        private readonly UiScript script;
        private readonly UiTones tones;
        private readonly bool isMobile;

        public OneResultCtrl()
        { }

        /// <summary>
        /// Ctor: regular lookup result
        /// </summary>
        public OneResultCtrl(CedictResult res, ICedictEntryProvider prov,
            UiScript script, UiTones tones, bool isMobile)
        {
            this.res = res;
            this.prov = prov;
            this.script = script;
            this.tones = tones;
            this.isMobile = isMobile;
        }

        /// <summary>
        /// Ctor: annotated Hanzi
        /// </summary>
        public OneResultCtrl(string query, CedictAnnotation ann, ICedictEntryProvider prov, UiTones tones, bool isMobile)
        {
            this.query = query;
            this.ann = ann;
            this.prov = prov;
            this.tones = tones;
            this.isMobile = isMobile;
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (res != null) renderResult(writer);
            else renderAnnotation(writer);
        }

        private void renderAnnotation(HtmlTextWriter writer)
        {
            CedictEntry entry = prov.GetEntry(ann.EntryId);
            string entryClass = "entry";
            if (tones == UiTones.Pleco) entryClass += " toneColorsPleco";
            else if (tones == UiTones.Dummitt) entryClass += " toneColorsDummitt";
            writer.AddAttribute(HtmlTextWriterAttribute.Class, entryClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Div); // <div class="entry">

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "hw-ann");
            writer.RenderBeginTag(HtmlTextWriterTag.Span); // <span class="hw-simp">
            renderHanzi(query, entry, ann.StartInQuery, ann.LengthInQuery, writer);
            writer.RenderEndTag(); // <span class="hw-ann">

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
                renderSense(writer, entry.GetSenseAt(i), i, null);
            writer.RenderEndTag(); // <div class="senses">

            writer.RenderEndTag(); // <div class="entry">
        }

        /// <summary>
        /// Render HW's Hanzi in annotation mode
        /// </summary>
        private void renderHanzi(string query, CedictEntry entry, int annStart, int annLength, HtmlTextWriter writer)
        {
            for (int i = 0; i != query.Length; ++i)
            {
                char c = query[i];
                PinyinSyllable py = null;
                if (i >= annStart && i < annStart + annLength)
                {
                    int pyIx = entry.HanziPinyinMap[i - annStart];
                    if (pyIx != -1) py = entry.Pinyin[pyIx];
                }
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
                // Whatever's outside annotation is faint
                if (i < annStart || i >= annStart + annLength) cls += " faint";
                // Mark up character for stroke order animation
                if (hanim) cls += " hanim";
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

        private void renderResult(HtmlTextWriter writer)
        {
            int hanziLimit = isMobile ? 4 : 6;
            CedictEntry entry = prov.GetEntry(res.EntryId);

            Dictionary<int, CedictTargetHighlight> senseHLs = new Dictionary<int, CedictTargetHighlight>();
            foreach (CedictTargetHighlight hl in res.TargetHilites)
                senseHLs[hl.SenseIx] = hl;

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
                CedictTargetHighlight thl = null;
                if (senseHLs.ContainsKey(i)) thl = senseHLs[i];
                renderSense(writer, entry.GetSenseAt(i), i, thl);
            }
            writer.RenderEndTag(); // <div class="senses">

            writer.RenderEndTag(); // <div class="entry">
        }

        /// <summary>
        /// Render HW's Hanzi in normal lookup result
        /// </summary>
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
                // Mark up character for stroke order animation
                if (hanim) cls += " hanim";
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

        private void renderSense(HtmlTextWriter writer, CedictSense sense, int ix, CedictTargetHighlight hl)
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
                renderEquiv(writer, sense.Equiv, hl, needToSplit);
                needToSplit = false;
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

        private class HybridTextConsumer
        {
            private readonly HybridText txt;
            private readonly CedictTargetHighlight hl;
            private int runIX = 0;
            private int runPos = 0;
            private string runTxt;
            public HybridTextConsumer(HybridText txt, CedictTargetHighlight hl)
            {
                this.txt = txt;
                this.hl = hl;
                runTxt = txt.GetRunAt(0).GetPlainText();
            }
            public void GetNext(out char c, out bool inHL)
            {
                if (runPos >= runTxt.Length)
                {
                    ++runIX;
                    runPos = 0;
                    if (runIX < txt.RunCount) runTxt = txt.GetRunAt(runIX).GetPlainText();
                    else runTxt = null;
                }
                if (runTxt == null)
                {
                    c = (char)0;
                    inHL = false;
                    return;
                }
                c = runTxt[runPos];
                if (hl == null || hl.RunIx != runIX) inHL = false;
                else inHL = runPos >= hl.HiliteStart && runPos < hl.HiliteStart + hl.HiliteLength;
                ++runPos;
            }
            public bool IsNextSpaceInHilite()
            {
                int nextSpaceIX = -1;
                for (int i = runIX; i < runTxt.Length; ++i)
                {
                    if (runTxt[i] == ' ') { nextSpaceIX = i; break; }
                }
                if (nextSpaceIX == -1) return false;
                return nextSpaceIX >= hl.HiliteStart && nextSpaceIX < hl.HiliteStart + hl.HiliteLength;
            }
        }

        private void renderEquiv(HtmlTextWriter writer, HybridText equiv, CedictTargetHighlight hl, bool nobr)
        {
            HybridTextConsumer htc = new HybridTextConsumer(equiv, hl);
            bool firstWordOver = false;
            bool hlOn = false;
            char c;
            bool inHL;
            while (true)
            {
                htc.GetNext(out c, out inHL);
                if (c == (char)0) break;
                // Highlight starts?
                if (inHL && !hlOn)
                {
                    // Very first word gets special highlight if hilite goes beyond first space, and we're in nobr mode
                    if (!firstWordOver && nobr && htc.IsNextSpaceInHilite())
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "sense-hl-start");
                        writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    }
                    // Plain old hilite start everywhere else
                    else
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "sense-hl");
                        writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    }
                    hlOn = true;
                }
                // Highlight ends?
                else if (!inHL && hlOn)
                {
                    writer.RenderEndTag();
                    hlOn = false;
                }
                // Space - close "nobr" span if first word's just over
                if (c == ' ' && !firstWordOver && nobr)
                {
                    firstWordOver = true;
                    writer.RenderEndTag();
                    if (hlOn)
                    {
                        writer.RenderEndTag();
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "sense-hl-end");
                        writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    }
                }
                // Render character
                writer.WriteEncodedText(c.ToString());
            }
            // Close hilite and nobr that we may have open
            if (!firstWordOver && nobr) writer.RenderEndTag();
            if (hlOn) writer.RenderEndTag();
        }
    }
}