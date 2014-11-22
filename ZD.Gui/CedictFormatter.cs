using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

using ZD.Common;

namespace ZD.Gui
{
    /// <summary>
    /// Formats pinyin and full Cedict entries for the UI and Clipboard.
    /// </summary>
    internal static class CedictFormatter
    {
        private readonly static string templateOuter;
        private readonly static string templateSense;
        private readonly static string template1;
        private readonly static string template2;
        private readonly static string template3;
        private readonly static string template4;
        private readonly static string templateSenseHanziOpen = "<span style=\"font-family: SimSun;\">";
        private readonly static string templateSenseHanziClose = "</span>";
        private readonly static string templateItalicsOpen = "<i>";
        private readonly static string templateItalicsClose = "</i>";
        private readonly static string templateDiamond = "⋄";
        private readonly static string templateBullet = "•";

        /// <summary>
        /// Static ctor: loads embedded resources (HTML templates).
        /// </summary>
        static CedictFormatter()
        {
            Assembly a = Assembly.GetExecutingAssembly();
            templateOuter = loadResourceString(a, "html-template-outer.html");
            templateSense = loadResourceString(a, "html-template-sense.html");
            template1 = loadResourceString(a, "html-template-1.html");
            template2 = loadResourceString(a, "html-template-2.html");
            template3 = loadResourceString(a, "html-template-3.html");
            template4 = loadResourceString(a, "html-template-4.html");
        }

        /// <summary>
        /// Reads one embedded string resource.
        /// </summary>
        private static string loadResourceString(Assembly a, string name)
        {
            using (Stream s = a.GetManifestResourceStream("ZD.Gui.Resources." + name))
            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// Gets entry's pinyin as a single string, with diacritics for tone.
        /// </summary>
        /// <param name="entry">The entry whose pinyin is retrieved.</param>
        /// <param name="syllLimit">Maximum number of syllables before ellipsis, or -1 for no limit.</param>
        /// <returns>The pinyin as a single string.</returns>
        public static string GetPinyinString(ReadOnlyCollection<PinyinSyllable> pinyinFull, int syllLimit = -1)
        {
            //var pinyinFull = entry.GetPinyinForDisplay(true);
            List<PinyinSyllable> pinyinList = new List<PinyinSyllable>();
            bool ellipsed = false;
            if (syllLimit == -1) pinyinList.AddRange(pinyinFull);
            else
            {
                int i;
                for (i = 0; i < pinyinFull.Count && i < syllLimit; ++i)
                    pinyinList.Add(pinyinFull[i]);
                if (i != pinyinFull.Count) ellipsed = true;
            }
            string res = "";
            foreach (var x in pinyinList)
            {
                if (res.Length > 0 && !SticksLeft(x.Text)) res += " ";
                res += x.GetDisplayString(true);
            }
            if (ellipsed) res += " …";
            return res;
        }

        /// <summary>
        /// Returns whether string sticks left because it is a punctuation mark like a comma.
        /// </summary>
        public static bool SticksLeft(string str)
        {
            if (str.Length == 0) return true;
            return char.IsPunctuation(str[0]) && str[0] != '·';
        }

        /// <summary>
        /// Gets Pinyin syllables as a single string, unnormalized, with numbers for tone marks.
        /// </summary>
        public static string GetPinyinCedict(IEnumerable<PinyinSyllable> sylls)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (PinyinSyllable ps in sylls)
            {
                if (!first && !SticksLeft(ps.Text)) sb.Append(' ');
                first = false;
                sb.Append(ps.Text);
                if (ps.Tone != -1) sb.Append(ps.Tone.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts a hybrid text to CEDICT-formatted plain text (marking up hanzi+pinyin sections).
        /// </summary>
        public static string HybridToCedict(HybridText ht)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            for (int i = 0; i != ht.RunCount; ++i)
            {
                TextRun tr = ht.GetRunAt(i);
                if (tr is TextRunLatin)
                {
                    string strRun = tr.GetPlainText();
                    if (!first && strRun != string.Empty && !char.IsPunctuation(strRun[0])) sb.Append(' ');
                    sb.Append(strRun);
                }
                else
                {
                    if (!first) sb.Append(' ');
                    TextRunZho trz = tr as TextRunZho;
                    if (!string.IsNullOrEmpty(trz.Simp)) sb.Append(trz.Simp);
                    if (trz.Trad != trz.Simp && !string.IsNullOrEmpty(trz.Trad))
                    {
                        sb.Append('|');
                        sb.Append(trz.Trad);
                    }
                    if (trz.Pinyin != null)
                    {
                        sb.Append('[');
                        sb.Append(GetPinyinCedict(trz.Pinyin));
                        sb.Append(']');
                    }
                }
                first = false;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the entry formatted a single CEDICT plain text line.
        /// </summary>
        public static string GetCedict(CedictEntry entry)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(entry.ChTrad);
            sb.Append(' ');
            sb.Append(entry.ChSimpl);
            sb.Append(" [");
            sb.Append(GetPinyinCedict(entry.Pinyin));
            sb.Append("] /");
            foreach (var sense in entry.Senses)
            {
                string strDomain = HybridToCedict(sense.Domain);
                string strEquiv = HybridToCedict(sense.Equiv);
                string strNote = HybridToCedict(sense.Note);
                sb.Append(strDomain);
                if (strDomain != string.Empty && strDomain != "CL:")
                    if (strEquiv != string.Empty || strNote != string.Empty)
                        sb.Append(' ');
                sb.Append(strEquiv);
                if (strEquiv != string.Empty && strNote != string.Empty)
                    sb.Append(' ');
                sb.Append(strNote);
                sb.Append('/');
            }

            // Done.
            return sb.ToString();
        }

        /// <summary>
        /// Escapes lt, gt and ampersand in HTML.
        /// </summary>
        private static string escape(string str)
        {
            str = str.Replace("&", "&amp;");
            str = str.Replace("<", "&lt;");
            str = str.Replace(">", "&gt;");
            return str;
        }

        /// <summary>
        /// Converts a hybrid text to HTML (marking up hanzi+pinyin sections).
        /// </summary>
        public static string hybridToHtml(HybridText ht, SearchScript script)
        {
            StringBuilder sb = new StringBuilder();

            bool first = true;
            for (int i = 0; i != ht.RunCount; ++i)
            {
                TextRun tr = ht.GetRunAt(i);
                if (tr is TextRunLatin)
                {
                    string strRun = tr.GetPlainText();
                    if (!first && strRun != string.Empty && !char.IsPunctuation(strRun[0])) sb.Append(' ');
                    sb.Append(strRun);
                }
                else
                {
                    if (!first) sb.Append(' ');
                    TextRunZho trz = tr as TextRunZho;

                    string hanzi1 = (script == SearchScript.Traditional) ? trz.Trad : trz.Simp;
                    if (string.IsNullOrEmpty(hanzi1)) hanzi1 = null;
                    string hanzi2 = null;
                    if (hanzi1 != null && script == SearchScript.Both && !string.IsNullOrEmpty(trz.Trad))
                        hanzi2 = trz.Trad;
                    if (hanzi1 != null) hanzi1 = escape(hanzi1);
                    if (hanzi2 != null) hanzi2 = escape(hanzi2);

                    if (hanzi1 != null || hanzi2 != null) sb.Append(templateSenseHanziOpen);
                    if (hanzi1 != null) sb.Append(hanzi1);
                    if (hanzi2 != null)
                    {
                        sb.Append(' ');
                        sb.Append(templateBullet);
                        sb.Append(' ');
                        sb.Append(hanzi2);
                    }
                    if (hanzi1 != null || hanzi2 != null) sb.Append(templateSenseHanziClose);

                    if (trz.Pinyin != null)
                    {
                        if (hanzi1 != null) sb.Append(' ');
                        sb.Append('[');
                        sb.Append(escape(trz.GetPinyinInOne(true)));
                        sb.Append(']');
                    }
                }
                first = false;
            }
            return sb.ToString();
        }


        /// <summary>
        /// Gets the HTML for a single sense, not including enclosing paragraph etc., only inline markup.
        /// </summary>
        private static string getSenseHtmlPure(ITextProvider tprov, CedictSense sense, SearchScript script)
        {
            StringBuilder sb = new StringBuilder();

            string strDomain = hybridToHtml(sense.Domain, script);
            string strEquiv = hybridToHtml(sense.Equiv, script);
            string strNote = hybridToHtml(sense.Note, script);
            if (sense.Domain != HybridText.Empty)
            {
                sb.Append(templateItalicsOpen);
                if (sense.Domain.EqualsPlainText("CL:"))
                    sb.Append(escape(tprov.GetString("ResultCtrlClassifier")) + " ");
                else
                    sb.Append(strDomain);
                sb.Append(templateItalicsClose);
            }
            if (sense.Domain != HybridText.Empty && !sense.Domain.EqualsPlainText("CL:"))
                if (sense.Equiv != HybridText.Empty || sense.Note != HybridText.Empty)
                    sb.Append(' ');
            sb.Append(strEquiv);
            if (sense.Equiv != HybridText.Empty && sense.Note != HybridText.Empty)
                sb.Append(' ');
            if (sense.Note != HybridText.Empty)
            {
                sb.Append(templateItalicsOpen);
                sb.Append(strNote);
                sb.Append(templateItalicsClose);
            }

            // Done
            return sb.ToString();
        }

        /// <summary>
        /// Gets the entry formatted in HTML.
        /// </summary>
        public static string GetHtml(ITextProvider tprov, CedictEntry entry, SearchScript script)
        {
            StringBuilder bodyHtml = new StringBuilder();

            // Are we showing one or two Hanzi headwords?
            string hanzi1 = script == SearchScript.Traditional ? entry.ChTrad : entry.ChSimpl;
            string hanzi2 = null;
            if (script == SearchScript.Both && entry.ChSimpl != entry.ChTrad)
                hanzi2 = entry.ChTrad;
            // Find simplest possible template, work with that
            // Only one hanzi, no longer than 2 chars, only one sense
            bool mustDoSenses = true;
            if (hanzi2 == null && hanzi1.Length <= 2 && entry.SenseCount == 1)
            {
                mustDoSenses = false;
                bodyHtml.Append(template1);
                bodyHtml.Replace("{hanzi}", escape(hanzi1));
                bodyHtml.Replace("{pinyin}", escape(GetPinyinString(entry.GetPinyinForDisplay(true))));
                bodyHtml.Replace("{sense}", getSenseHtmlPure(tprov, entry.GetSenseAt(0), script));
            }
            // Only one script, no more than 6 chars
            else if (hanzi2 == null && hanzi1.Length <= 6)
            {
                bodyHtml.Append(template2);
                bodyHtml.Replace("{hanzi}", escape(hanzi1));
                bodyHtml.Replace("{pinyin}", escape(GetPinyinString(entry.GetPinyinForDisplay(true))));
            }
            // Only one script
            else if (hanzi2 == null)
            {
                bodyHtml.Append(template3);
                bodyHtml.Replace("{hanzi}", escape(hanzi1));
                bodyHtml.Replace("{pinyin}", escape(GetPinyinString(entry.GetPinyinForDisplay(true))));
            }
            // Everything else: very full-fledged entry
            else
            {
                bodyHtml.Append(template4);
                bodyHtml.Replace("{hanzi1}", escape(hanzi1));
                bodyHtml.Replace("{hanzi2}", escape(hanzi2));
                bodyHtml.Replace("{pinyin}", escape(GetPinyinString(entry.GetPinyinForDisplay(true))));
            }
            // In all but the first, simplest case, dealing with senses is the same
            if (mustDoSenses)
            {
                StringBuilder sbSenses = new StringBuilder();
                foreach (CedictSense sense in entry.Senses)
                {
                    string senseHtml = "";
                    if (!sense.Domain.EqualsPlainText("CL:"))
                    {
                        senseHtml += templateDiamond;
                        senseHtml += " ";
                    }
                    senseHtml += getSenseHtmlPure(tprov, sense, script);
                    senseHtml = templateSense.Replace("{sense}", senseHtml);
                    sbSenses.Append(senseHtml);
                }
                bodyHtml.Replace("{senses}", sbSenses.ToString());
            }

            // Assemble the whole HTML
            StringBuilder sb = new StringBuilder();
            sb.Append(templateOuter);
            sb.Replace("{body}", bodyHtml.ToString());
            // Purge new lines and tabs: this avoids extra spaces e.g. when pasting into Word
            sb.Replace("\r\n", "");
            sb.Replace("\t", "");
            // Done
            return sb.ToString();
        }

        private static Regex reBr= new Regex(@"\<br[^\<]+\>|\<\/p\>");
        private static Regex reTag = new Regex(@"\<[^\<]+\>");

        /// <summary>
        /// Gets entry "formatted" in plain text, for alternative Clipboard content next to HTML.
        /// </summary>
        public static string StripPlainFromHtml(string html)
        {
            html = reBr.Replace(html, "\r\n");
            html = reTag.Replace(html, "");
            html = html.Replace("{{entry}}", "");
            html = html.Replace(templateDiamond, "-");
            return html;
        }
    }
}
