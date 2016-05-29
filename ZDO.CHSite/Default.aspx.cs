using System;
using System.Web;
using System.Reflection;

namespace ZDO.CHSite
{
    public partial class Default : System.Web.UI.Page
    {
#if DEBUG
        public bool DebugMode { get { return true; } }
#else
        public bool DebugMode { get { return false; } }
#endif

        private string lang = null;
        /// <summary>
        /// Language of current page (extracted from URL by rewrite rule; always present).
        /// </summary>
        public string Lang { get { return lang; } }

        private string rel = null;
        /// <summary>
        /// Relative path to current page (part of URL after language code, w/o leadig or trailing slash).
        /// </summary>
        public string Rel {  get { return rel; } }

        /// <summary>
        /// The executing assembly's version, as string.
        /// </summary>
        private static string verStr = null;

        /// <summary>
        /// Gets the executing assembly's version, as string.
        /// </summary>
        public static string VerStr
        {
            get
            {
                if (verStr == null)
                {
                    string s = Assembly.GetExecutingAssembly().GetName().Version.Major.ToString();
                    s += ".";
                    s += Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();
                    verStr = s;
                }
                return verStr;
            }
        }

        private string htmlLang;
        /// <summary>
        /// Language code used in HTML element's lang attribute to declare page language.
        /// </summary>
        public string HtmlLang { get { return htmlLang; } }

        private string stTitle = "";
        /// <summary>
        /// Title of current page, if static content is fed directly to client at page load.
        /// </summary>
        public string StTitle { get { return Server.HtmlEncode(stTitle); } }
        private string stDescription = "";
        // Description of current page, if static content is fed directly to client at page load.
        /// <summary>
        /// Description of current page, if static content is fed directly to client at page load.
        /// </summary>
        public string StDescription { get { return Server.HtmlEncode(stDescription); } }
        private string stKeywords = "";
        /// <summary>
        /// Keywords of current page, if static content is fed directly to client at page load.
        /// </summary>
        public string StKeywords { get { return Server.HtmlEncode(stKeywords); } }

        /// <summary>
        /// Get localized string in HTML-escapted form.
        /// </summary>
        public string EscStr(string id)
        {
            string raw = TextProvider.Instance.GetString(lang, id);
            return Server.HtmlEncode(raw);
        }

        /// <summary>
        /// Gets the GA code to be inserted into page. Comes from config file so staging site doesn't interfere.
        /// </summary>
        protected string GetGACode()
        {
            return Global.GACode;
        }

        /// <summary>
        /// Initializes master page (site location, language etc.)
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            // What is our current language?
            // We always have this from URL b/c of rewrite rule
            lang = Request.Params["lang"];
            // Relative path (w/o language)
            rel = Request.RawUrl.Substring(3).TrimStart('/');
            // For now, HTML lang equals "lang", but we might need trickey if we use "jian" / "fan" later.
            htmlLang = lang;
            // Set language cookie now
            HttpCookie uilangCookie = new HttpCookie("uilang");
            uilangCookie.Value = lang;
            uilangCookie.Expires = DateTime.UtcNow.AddDays(365);
            Response.Cookies.Add(uilangCookie);
            // Add proprietary JS files for single-page app
            // We add all scripts for all pages
#if DEBUG
            // Keep include order in sync with bundling in bundleconfig.json
            litJS.Text += "<script src='/js-" + VerStr + "/strings-hu.js'></script>\r\n";
            litJS.Text += "<script src='/js-" + VerStr + "/strings-en.js'></script>\r\n";
            litJS.Text += "<script src='/js-" + VerStr + "/page.js'></script>\r\n";
            litJS.Text += "<script src='/js-" + VerStr + "/newentry.js'></script>\r\n";
            litJS.Text += "<script src='/js-" + VerStr + "/strokeanim.js'></script>\r\n";
            litJS.Text += "<script src='/js-" + VerStr + "/lookup.js'></script>\r\n";
            litJS.Text += "<script src='/js-" + VerStr + "/history.js'></script>\r\n";
            // DBG
            litJS.Text += "<script src='/js-" + VerStr + "/diagnostics.js'></script>\r\n";
#endif
            // If page is static, we feed it to client right away
            // We can retrieve it quickly, so it's better to avoid dynamic content callback
            ADynPage.Result pres = ADynPage.StaticLoad(lang, rel);
            if (pres != null)
            {
                // Mark up to indicate presence of content, so page script will not call back.
                string bodyClass = theBody.Attributes["class"];
                bodyClass += " has-initial-content";
                theBody.Attributes["class"] = bodyClass;
                // Fill in content
                dynPage.InnerHtml = pres.Html;
                stTitle = pres.Title;
                stDescription = pres.Description;
                stKeywords = pres.Keywords;
            }
            // Fill in some defaults, so page doesn't look silly while loading
            else
            {
                stTitle = TextProvider.Instance.GetString(lang, "DefaultTitle");
            }
        }
    }
}