using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;

namespace ZD.WikiPages
{
    internal class WrkWikiPages
    {
        private readonly StreamReader srPages;
        private readonly StreamReader srRedirects;
        private readonly StreamReader srLanglinks;

        public WrkWikiPages(StreamReader srPages, StreamReader srRedirects, StreamReader srLanglinks)
        {
            this.srPages = srPages;
            this.srRedirects = srRedirects;
            this.srLanglinks = srLanglinks;
        }

        /// <summary>
        /// Holds page titles in Chinese and other languages.
        /// </summary>
        private class PageInfo
        {
            /// <summary>
            /// Empty collection of redirecting pages.
            /// </summary>
            private static readonly ReadOnlyCollection<string> emptyAlts;
            /// <summary>
            /// Page title in Chinese.
            /// </summary>
            public readonly string Zh;
            /// <summary>
            /// Page title in Hungarian, or empty.
            /// </summary>
            public string Hu = string.Empty;
            /// <summary>
            /// Page title in English, or empty.
            /// </summary>
            public string En = string.Empty;
            /// <summary>
            /// Page title in German, or empty.
            /// </summary>
            public string De = string.Empty;
            /// <summary>
            /// Pages that redirect to this one.
            /// </summary>
            private List<string> alts = null;
            /// <summary>
            /// Gets collection of pages redirecting to this one, i.e., alternatives.
            /// </summary>
            public ReadOnlyCollection<string> Alts
            {
                get { if (alts == null) return emptyAlts; return new ReadOnlyCollection<string>(alts); }
            }
            /// <summary>
            /// Gets count of pages redirecting to this one.
            /// </summary>
            public int AltCount
            {
                get { return alts == null ? 0 : alts.Count; }
            }
            /// <summary>
            /// Static ctor.
            /// </summary>
            static PageInfo()
            {
                emptyAlts = new ReadOnlyCollection<string>(new string[0]);
            }
            /// <summary>
            /// Inits instances: stores Chinese page title.
            /// </summary>
            public PageInfo(string zh)
            {
                Zh = zh;
            }
            /// <summary>
            /// Records a page that redirects to this one.
            /// </summary>
            public void AddAlt(string altTitle)
            {
                if (alts == null) alts = new List<string>();
                alts.Add(altTitle);
            }
        }

        /// <summary>
        /// Maps page IDs to info about page (title, language alternatives; only master pages).
        /// </summary>
        private readonly Dictionary<int, PageInfo> idToInfo = new Dictionary<int, PageInfo>();

        /// <summary>
        /// Maps master page titles to their IDs.
        /// </summary>
        private readonly Dictionary<string, int> titleToId = new Dictionary<string, int>();

        /// <summary>
        /// Maps page IDs to title (for redirecting pages).
        /// </summary>
        private readonly Dictionary<int, string> idToRedirTitle = new Dictionary<int, string>();

        /// <summary>
        /// Number of redirects whose source cannot be found in <see cref="idToRedirTitle"/>.
        /// </summary>
        private int redirSourceMissing = 0;

        /// <summary>
        /// Number of redirects whose target cannot be found in <see cref="titleToId"/>.
        /// </summary>
        private int redirTargetMissing = 0;

        /// <summary>
        /// Process input.
        /// </summary>
        public void Work()
        {
            // Parse page titles
            doParseTitles();
            // Parse redirects so we know each master page's alternatives
            doParseRedirects();
            // Parse language links, get ones relevant for us
            doParseLangs();
        }

        private void doParseTitles()
        {
            string line = null;
            while ((line = srPages.ReadLine()) != null)
            {
                line = line.Replace("INSERT INTO `page` VALUES (", "");
                line = line.Replace(");", "");
                string[] parts = line.Split(new string[] { "),(" }, StringSplitOptions.None);
                if (parts.Length < 2) continue;
                foreach (string part in parts)
                {
                    if (!string.IsNullOrEmpty(part)) doParseOnePage(part);
                }
            }
        }

        /// <summary>
        /// Parses the parts of a comma-separated VALUE, paying attention to commas within quotes (and strippging qupotes).
        /// </summary>
        private static string[] doParseValues(string str)
        {
            bool inQuote = false;
            List<string> res = new List<string>();
            StringBuilder currVal = new StringBuilder();
            for (int i = 0; i != str.Length; ++i)
            {
                char c = str[i];
                if (c == '\\')
                {
                    if (!inQuote) throw new Exception("SQL VALUE parse error: \\ outside quote.");
                    ++i;
                    c = str[i];
                    currVal.Append(c);
                }
                else if (c == '\'')
                {
                    if (!inQuote)
                    {
                        if (currVal == null || currVal.Length > 0) throw new Exception("SQL VALUE parse error: closed quote followed by something other than a comma.");
                        inQuote = true;
                    }
                    else
                    {
                        inQuote = false;
                        res.Add(currVal.ToString());
                        currVal = null; // This makes sure we only accept comma after a closing quote
                    }
                }
                else if (c == ',')
                {
                    if (inQuote) currVal.Append(c);
                    else
                    {
                        if (currVal != null)
                        {
                            res.Add(currVal.ToString());
                            currVal.Clear();
                        }
                        else currVal = new StringBuilder();
                    }
                }
                else currVal.Append(c);
            }
            // Last item
            if (currVal != null) res.Add(currVal.ToString());
            // Done.
            return res.ToArray();
        }

        private void doParseOnePage(string str)
        {
            // Make sense of SQL VALUE
            string[] parts = doParseValues(str);
            if (parts.Length != 13) throw new Exception("Page parse error: 13 values are expected.");
            // Get the fields relevant for us
            int nsId = int.Parse(parts[1]);
            int redirectVal = int.Parse(parts[5]);
            // Only care about namespace 0
            if (nsId != 0) return;
            int pageId = int.Parse(parts[0]);
            string title = parts[2];
            // Page is a redirect? Record as such.
            if (redirectVal != 0) idToRedirTitle[pageId] = title;
            // Not a redirect: record as page.
            else
            {
                titleToId[title] = pageId;
                idToInfo[pageId] = new PageInfo(title);
            }
        }

        private void doParseRedirects()
        {
            string line = null;
            while ((line = srRedirects.ReadLine()) != null)
            {
                line = line.Replace("INSERT INTO `redirect` VALUES (", "");
                line = line.Replace(");", "");
                string[] parts = line.Split(new string[] { "),(" }, StringSplitOptions.None);
                if (parts.Length < 2) continue;
                foreach (string part in parts)
                {
                    if (!string.IsNullOrEmpty(part)) doParseOneRedirect(part);
                }
            }
        }

        private void doParseOneRedirect(string str)
        {
            // Make sense of SQL VALUE
            string[] parts = doParseValues(str);
            if (parts.Length != 5) throw new Exception("Redirect parse error: 5 values are expected.");
            // Only care about namespace 0
            if (int.Parse(parts[1]) != 0) return;
            // Get the fields relevant for us
            int pageId = int.Parse(parts[0]);
            string titleTo = parts[2];
            // Who is redirecting?
            if (!idToRedirTitle.ContainsKey(pageId))
            {
                ++redirSourceMissing;
                return;
            }
            string titleFrom = idToRedirTitle[pageId];
            // Add this page as a redirecting alternative to my master page
            if (titleToId.ContainsKey(titleTo))
            {
                int masterId = titleToId[titleTo];
                idToInfo[masterId].AddAlt(titleFrom);
            }
            else ++redirTargetMissing;
        }

        private void doParseLangs()
        {
            string line = null;
            while ((line = srLanglinks.ReadLine()) != null)
            {
                line = line.Replace("INSERT INTO `langlinks` VALUES (", "");
                line = line.Replace(");", "");
                string[] parts = line.Split(new string[] { "),(" }, StringSplitOptions.None);
                if (parts.Length < 2) continue;
                foreach (string part in parts)
                {
                    if (!string.IsNullOrEmpty(part)) doParseOneLanglink(part);
                }
            }
        }

        private void doParseOneLanglink(string str)
        {
            // Make sense of SQL VALUE
            string[] parts = doParseValues(str);
            if (parts.Length != 3) throw new Exception("Langlink parse error: 3 values are expected.");
            // Only care about EN, DE, HU
            string lang = parts[1];
            if (lang != "en" && lang != "de" && lang != "hu") return;
            // Page ID and target
            int pageId = int.Parse(parts[0]);
            string target = parts[2];
            // Record info - if page linking to other language is present
            if (!idToInfo.ContainsKey(pageId)) return;
            PageInfo pi = idToInfo[pageId];
            if (lang == "en") pi.En = target;
            else if (lang == "de") pi.De = target;
            else if (lang == "hu") pi.Hu = target;
        }

        /// <summary>
        /// Compute whatever's needed in-memory, write output.
        /// </summary>
        public void Finish(StreamWriter swOut)
        {
            // Find max number of alternatives, so we can produce well-behaved TSV
            int maxAlts = 0;
            foreach (var x in idToInfo)
            {
                if (x.Value.AltCount > maxAlts) maxAlts = x.Value.AltCount;
            }
            // Serialize TSV output
            StringBuilder sb = new StringBuilder();
            foreach (var x in idToInfo)
            {
                PageInfo pi = x.Value;
                sb.Clear();

                sb.Append(pi.Zh);
                sb.Append('\t');
                sb.Append(pi.En);
                sb.Append('\t');
                sb.Append(pi.De);
                sb.Append('\t');
                sb.Append(pi.Hu);
                ReadOnlyCollection<string> alts = pi.Alts;
                for (int i = 0; i != maxAlts; ++i)
                {
                    sb.Append('\t');
                    if (i < alts.Count) sb.Append(alts[i]);
                }
                swOut.WriteLine(sb.ToString());
            }
        }
    }
}
