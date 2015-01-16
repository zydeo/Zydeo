using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

using ZD.ChDict.Common;

namespace ZD.DictEditor
{
    partial class MainForm
    {
        private static readonly string htmlSkeleton;

        private static string readHtmlSkeleton()
        {
            Assembly a = Assembly.GetExecutingAssembly();
            using (Stream s = a.GetManifestResourceStream("ZD.DictEditor.Resources.entry-skeleton.html"))
            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }

        private static string esc(string str)
        {
            str = str.Replace("&", "&amp;");
            str = str.Replace("<", "&lt;");
            str = str.Replace(">", "&gt;");
            return str;
        }

        private static string getWikis(BackboneEntry be)
        {
            string str = "";
            string hu = be.GetPart(BackbonePart.WikiHu) as string;
            if (hu != null)
            {
                str += "<span class='label'>Wiki-HU:</span> " + esc(hu) + "</br>\r\n";
            }
            TransTriple tt = be.GetPart(BackbonePart.WikiEn) as TransTriple;
            if (tt != null)
            {
                str += "<span class='label'>Wiki-EN:</span> " + esc(tt.Orig);
                str += " <span class='hu-xlated'>• " + esc(tt.Goog);
                str += " • " + esc(tt.Bing);
                str += "</span><br />\r\n";
            }
            tt = be.GetPart(BackbonePart.WikiDe) as TransTriple;
            if (tt != null)
            {
                str += "<span class='label'>Wiki-De:</span> " + esc(tt.Orig);
                str += " <span class='hu-xlated'>• " + esc(tt.Goog);
                str += " • " + esc(tt.Bing);
                str += "</span><br />\r\n";
            }
            return str;
        }

        private static string getSenses(TransTriple[] tts)
        {
            string str = "";
            foreach (TransTriple tt in tts)
            {
                str += "<li>" + esc(tt.Orig);
                if (!tt.Orig.StartsWith("CL:"))
                {
                    str += " • ";
                    str += "<span class='hu-xlated'>" + esc(tt.Goog) + " • " + esc(tt.Bing) + "</span><br/>\r\n";
                    str += "</li>\r\n";
                }

            }
            return str;
        }

        private static string getHanzi(BackboneEntry be)
        {
            string str = be.Simp;
            if (be.Trad != be.Simp)
            {
                str += " <span class='trad'>";
                str += be.Trad;
                str += "</span>";
            }
            return str;
        }

        private static string getPinyin(BackboneEntry be)
        {
            return esc(PinyinDisplay.GetPinyinDisplay(be.Pinyin));
        }

        private static string makeHtml(BackboneEntry be)
        {
            string str = htmlSkeleton;
            str = str.Replace("{hanzi}", getHanzi(be));
            str = str.Replace("{pinyin}", getPinyin(be));
            str = str.Replace("{rank}", be.Rank.ToString());
            str = str.Replace("{google}", esc(be.TransGoog));
            str = str.Replace("{bing}", esc(be.TransBing));
            string wikis = getWikis(be);
            str = str.Replace("{wikis}", wikis);
            // CEDICT
            TransTriple[] ttCedict = be.GetPart(BackbonePart.Cedict) as TransTriple[];
            if (ttCedict == null)
            {
                str = str.Replace("{cedict-cmt-start}", "<!--");
                str = str.Replace("{cedict-cmt-end}", "-->");
            }
            else
            {
                str = str.Replace("{cedict-cmt-start}", "");
                str = str.Replace("{cedict-cmt-end}", "");
                str = str.Replace("{cedict-senses}", getSenses(ttCedict));
            }
            // HanDeDict
            TransTriple[] ttHanDeDict = be.GetPart(BackbonePart.HanDeDict) as TransTriple[];
            if (ttHanDeDict == null)
            {
                str = str.Replace("{handedict-cmt-start}", "<!--");
                str = str.Replace("{handedict-cmt-end}", "-->");
            }
            else
            {
                str = str.Replace("{handedict-cmt-start}", "");
                str = str.Replace("{handedict-cmt-end}", "");
                str = str.Replace("{handedict-senses}", getSenses(ttHanDeDict));
            }
            // Done
            return str;
        }

        private void doPrintBackbone(BackboneEntry be)
        {
            string html = makeHtml(be);

            wcInfo.Navigate("about:blank");
            if (wcInfo.Document != null) wcInfo.Document.Write(string.Empty);
            wcInfo.DocumentText = html;
        }
    }
}
