using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

namespace ZDO.CHSite
{
    [ActionName("dynpage")]
    public partial class ADynPage : ApiAction
    {
        /// <summary>
        /// Ctor: init. Boilerplate.
        /// </summary>
        public ADynPage(HttpContext ctxt) : base(ctxt) { }

        /// <summary>
        /// Everything relevant for a dynamic page: HTML, title, other meta-information.
        /// </summary>
        [DataContract]
        public class Result
        {
            [DataMember(Name = "html")]
            public string Html = "";
            [DataMember(Name = "title")]
            public string Title = "";
            [DataMember(Name = "keywords")]
            public string Keywords = "";
            [DataMember(Name = "description")]
            public string Description = "";
        }

        /// <summary>
        /// Assembles full absolute file name for language and relative path.
        /// </summary>
        private static string getFileName(string lang, string rel)
        {
            // Derive file name in Content folder from request
            string[] relParts = rel.Split('/');
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i != relParts.Length; ++i)
            {
                if (i > 0) sb.Append('.');
                sb.Append(relParts[i]);
            }
            sb.Append(".");
            sb.Append(lang);
            sb.Append(".html");
            string res = HttpRuntime.AppDomainAppPath;
            res = Path.Combine(res, "Content");
            res = Path.Combine(res, sb.ToString());
            // If derived file exists, return its full path.
            if (File.Exists(res)) return res;
            // If this file is not there, fall back to "hu"
            if (lang != "hu") return getFileName("hu", rel);
            // This IS "hu" - no such file, return null.
            return null;
        }

        /// <summary>
        /// Reads and interprets fully qualified HTML file.
        /// </summary>
        private static void readFile(string fname, out StringBuilder sbHtml, out string title,
            out string keywords, out string description)
        {
            sbHtml = new StringBuilder();
            title = "";
            keywords = "";
            description = "";
            using (StreamReader sr = new StreamReader(fname))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("<!-- #"))
                    {
                        int ix = line.IndexOf(':');
                        string a = line.Substring(6, ix - 6);
                        string b = line.Substring(ix + 2, line.Length - ix - 6);
                        if (a == "Title") title = HttpUtility.HtmlDecode(b);
                        else if (a == "Keywords") keywords = HttpUtility.HtmlDecode(b);
                        else if (a == "Description") keywords = HttpUtility.HtmlDecode(b);
                    }
                    sbHtml.AppendLine(line);
                }
            }
        }

        /// <summary>
        /// Makes result based on fully qualified static HTML file name.
        /// </summary>
        private static Result makeResult(string fname)
        {
            StringBuilder sbHtml;
            string title, keywords, description;
            readFile(fname, out sbHtml, out title, out keywords, out description);
            Result res = new Result();
            res.Html = sbHtml.ToString();
            res.Title = title;
            res.Keywords = keywords;
            res.Description = description;
            return res;
        }

        /// <summary>
        /// Provides content of welcome screen.
        /// </summary>
        private static Result makeWelcomeResult(string lang)
        {
            string fname = getFileName(lang, "_welcome");
            return makeResult(fname);
        }

        /// <summary>
        /// Provides static page for Default.aspx code-behind, without any AJAX handler machinery.
        /// </summary>
        /// <param name="lang">Page language from URL.</param>
        /// <param name="rel">Relative address from URL.</param>
        /// <returns>HTML and metas to return, or null if this page is not to be served without callback.</returns>
        public static Result StaticLoad(string lang, string rel)
        {
            // Pages we don't want to load statically (too long - callback better)
            if (rel.StartsWith("search/") || rel.StartsWith("edit/history"))
                return null;
            // Welcome page gets special treatment
            if (rel == "") return makeWelcomeResult(lang);
            // Just get HTML
            return getStaticResult(lang, rel);
        }

        /// <summary>
        /// Gets static page for a language and relative URL.
        /// </summary>
        public static Result getStaticResult(string lang, string rel)
        {
            // DBG: Show diagnostics on download page
            if (rel == "download")
                return makeResult(getFileName("en", "_diagnostics"));

            string fname = getFileName(lang, rel);
            // If requested file exists, feed it to caller
            if (fname != null)
                return makeResult(fname);
            // For "edit" menu, return WIP
            if (rel.StartsWith("edit"))
                return makeResult(getFileName(lang, "_work-in-progress"));
            return null;
        }

        /// <summary>
        /// Processes dynamic page request: result includes HTML, title, keywords, description.
        /// </summary>
        public override void Process()
        {
            string lang = Req.Params["lang"];
            string rel = Req.Params["rel"];

            // If request is for search, special treatment
            if (rel == "" || rel.StartsWith("search/"))
                doSearch(lang, rel);
            // If request is for history, special treatment
            else if (rel.StartsWith("edit/history"))
                doHistory(lang, rel);
            // Static page, nothing special
            else
            {
                Res = getStaticResult(lang, rel);
                if (Res == null) throw new Exception("Requested dynamic page not found.");
            }
        }
    }
}