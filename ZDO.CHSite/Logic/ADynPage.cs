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
    [ActionName("dynpage")]
    public partial class ADynPage : ApiAction
    {
        /// <summary>
        /// Ctor: init. Boilerplate.
        /// </summary>
        public ADynPage(HttpContext ctxt) : base(ctxt) { }

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

        private void readFile(string fname, out StringBuilder sbHtml, out string title, out string keywords, out string description)
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

        private void feedResult(string fname)
        {
            StringBuilder sbHtml;
            string title, keywords, description;
            readFile(fname, out sbHtml, out title, out keywords, out description);
            Result res = new Result();
            res.Html = sbHtml.ToString();
            res.Title = title;
            res.Keywords = keywords;
            res.Description = description;
            Res = res;
        }

        public override void Process()
        {
            string lang = Req.Params["lang"];
            string rel = Req.Params["rel"];

            // If request is for search, special treatment
            if (rel == "" || rel.StartsWith("search/"))
            {
                doSearch(lang, rel);
                return;
            }
            // If request is for history, special treatment
            if (rel.StartsWith("edit/history"))
            {
                doHistory(lang, rel);
                return;
            }
            // DBG: Show diagnostics on download page
            if (rel == "download")
            {
                feedResult(getFileName("en", "_diagnostics"));
                return;
            }

            string fname = getFileName(lang, rel);
            // If requested file exists, feed it to caller
            if (fname != null)
            {
                feedResult(fname);
                return;
            }
            // For "edit" menu, return WIP
            if (rel.StartsWith("edit"))
            {
                feedResult(getFileName(lang, "_work-in-progress"));
                return;
            }
            // Throw up in their face.
            throw new Exception("Requested dynamic page not found.");
        }
    }
}