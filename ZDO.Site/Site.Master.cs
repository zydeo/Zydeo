using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Text;

using ZD.Common;

namespace Site
{
    public partial class Master : System.Web.UI.MasterPage
    {
        private string uiLang = "de";
        public string UILang { get { return uiLang; } }

        private void determineLanguage()
        {
            // Got language parameter?
            string uiParam = Request.Params["ui"];
            string uiFromParam = null;
            if (uiParam == "de") uiFromParam = "de";
            else if (uiParam == "en") uiFromParam = "en";
            else if (uiParam == "jian") uiFromParam = "jian";
            else if (uiParam == "fan") uiFromParam = "fan";
            // We have a language from a parameter: set cookie now
            if (uiFromParam != null)
            {
                uiLang = uiFromParam;
                HttpCookie uilangCookie = new HttpCookie("uilang");
                uilangCookie.Value = uiFromParam;
                uilangCookie.Expires = DateTime.UtcNow.AddDays(365);
                Response.Cookies.Add(uilangCookie);
            }
            // Nothing from a param: see if we have a cookie
            else
            {
                if (Request.Cookies["uilang"] != null)
                {
                    string langFromCookie = Request.Cookies["uilang"].Value;
                    if (langFromCookie == "de") uiLang = "de";
                    else if (langFromCookie == "en") uiLang = "en";
                    else if (langFromCookie == "jian") uiLang = "jian";
                    else if (langFromCookie == "fan") uiLang = "fan";
                }
            }
        }

        private string pageName = null;
        public string PageName { get { return pageName; } }

        private void determinePage(string requestPath)
        {
            if (requestPath == @"/Default.aspx") pageName = "search";
            else if (requestPath == @"/Statics.aspx")
            {
                string page = Request.Params["page"];
                if (page == "about") pageName = "about";
                else if (page == "options") pageName = "options";
                else if (page == "cookies") pageName = "cookies";
            }
        }

        private UiScript uiScript = UiScript.Both;
        private UiTones uiTones = UiTones.Pleco;
        public UiScript UiScript { get { return uiScript; } }
        public UiTones UiTones { get { return uiTones; } }

        private void determineCookieOptions()
        {
            if (Request.Cookies["uiscript"] != null)
            {
                if (Request.Cookies["uiscript"].Value == "both") uiScript = UiScript.Both;
                else if (Request.Cookies["uiscript"].Value == "simp") uiScript = UiScript.Simp;
                else if (Request.Cookies["uiscript"].Value == "trad") uiScript = UiScript.Trad;
            }
            if (Request.Cookies["uitones"] != null)
            {
                if (Request.Cookies["uitones"].Value == "none") uiTones = UiTones.None;
                else if (Request.Cookies["uitones"].Value == "pleco") uiTones = UiTones.Pleco;
                else if (Request.Cookies["uitones"].Value == "dummitt") uiTones = UiTones.Dummitt;
            }

        }

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

        /// <summary>
        /// Resolve the URL of a JS file, taking into account current site version.
        /// </summary>
        /// <param name="namePure">Name of the JS file, without folder etc.</param>
        /// <returns>The resolved URL to be included in page.</returns>
        public string ResolveMyJS(string namePure)
        {
            string res = "~/js-{0}/{1}";
            res = string.Format(res, VerStr, namePure);
            return ResolveUrl(res);
        }

        /// <summary>
        /// Gets the GA code to be inserted into page. Comes from config file so staging site doesn't interfere.
        /// </summary>
        protected string GetGACode()
        {
            return Global.GACode;
        }

        public void SetMobile()
        {
            theBody.Attributes["class"] = theBody.Attributes["class"] + " mobile";
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // What UI language are we going by?
            determineLanguage();
            // Which page are we showing?
            determinePage(Request.Path);
            // Search options (from cookies)
            determineCookieOptions();
        }

        private bool descrAndKeywSet = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Current version and year in footer
            footerVer.InnerText = VerStr;
            footerYear.InnerText = DateTime.Now.Year.ToString();

            // Hilite language in selector
            if (uiLang == "de") langselDe.Attributes["class"] = langselDe.Attributes["class"] + " active";
            else if (uiLang == "en") langselEn.Attributes["class"] = langselEn.Attributes["class"] + " active";
            else if (uiLang == "jian") langselJian.Attributes["class"] = langselJian.Attributes["class"] + " active";
            else if (uiLang == "fan") langselFan.Attributes["class"] = langselFan.Attributes["class"] + " active";

            // Server-side localized UI in master
            TextProvider prov = TextProvider.Instance;
            if (!descrAndKeywSet)
            {
                Page.MetaDescription = prov.GetString(uiLang, "MetaDescription");
                Page.MetaKeywords = prov.GetString(uiLang, "MetaKeywords");
            }
            linkSearch.InnerText = prov.GetString(uiLang, "MenuSearch");
            linkOptions.InnerText = prov.GetString(uiLang, "MenuOptions");
            linkAbout.InnerText = prov.GetString(uiLang, "MenuInfo");
            navImprint.InnerText = prov.GetString(uiLang, "MenuImprint");
            linkFooterImprint.InnerText = prov.GetString(uiLang, "MenuImprint");
            bitterCookieTalks.Text = prov.GetString(uiLang, "CookieNotice");
            swallowbitterpill.InnerText = prov.GetString(uiLang, "CookieAccept");
            cookierecipe.InnerText = prov.GetString(uiLang, "CookieLearnMore");

            // Make relevant menu item "active"
            if (pageName == "search")
                navSearch.Attributes["class"] = navSearch.Attributes["class"] + " active";
            else if (pageName == "about")
                navAbout.Attributes["class"] = navAbout.Attributes["class"] + " active";
            else if (pageName == "options")
                navOptions.Attributes["class"] = navAbout.Attributes["class"] + " active";
        }

        private string getWalkPara(string query, bool isZho)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<a href='/");
            sb.Append("search/");
            sb.Append(isZho ? "zho/" : "trg/");
            sb.Append(HttpUtility.UrlEncode(query));
            sb.Append("'>");
            sb.Append(HttpUtility.HtmlEncode(query));
            sb.Append("</a>");
            string walkLink = sb.ToString();

            string str = HttpUtility.HtmlEncode(TextProvider.Instance.GetString(uiLang, isZho ? "WalkthroughZho" : "WalkthroughTrg"));
            str = string.Format(str, walkLink);
            str = "<p>" + str + "</p>\r\n";

            return str;
        }

        /// <summary>
        /// Infuses walkthrough links at bottom of page.
        /// </summary>
        public void SetStaticQuery(string query, SearchLang slang, DateTime dtStart)
        {
            // No query: seeding walkthrough from start page
            if (query == null)
            {
                string hanzi, target;
                Global.Dict.GetFirstWords(out hanzi, out target);
                string inner = getWalkPara(hanzi, true);
                inner += getWalkPara(target, false);
                walkthroughDiv.InnerHtml = inner;
            }
            // This was a static query; walk down hanzi headwords or german words
            else
            {
                // Link back and forth
                bool isTarget = slang == SearchLang.Target;
                string prev, next;
                Global.Dict.GetPrevNextWords(query, isTarget, out prev, out next);
                string inner = "";
                if (prev != null) inner += getWalkPara(prev, !isTarget);
                if (next != null) inner += getWalkPara(next, !isTarget);
                walkthroughDiv.InnerHtml = inner;
                // Keywords and decription
                string keyw = TextProvider.Instance.GetString(uiLang, isTarget ? "MetaKeywordsTrg" : "MetaKeywordsZho");
                keyw = string.Format(keyw, query);
                Page.MetaKeywords = keyw;
                string desc = TextProvider.Instance.GetString(uiLang, isTarget ? "MetaDescriptionTrg" : "MetaDescriptionZho");
                desc = string.Format(desc, query);
                Page.MetaDescription = desc;
                descrAndKeywSet = true;
                // Log that we've been crawled
                QueryLogger.Instance.LogStatic(query, slang, Request.UserAgent, DateTime.UtcNow.Subtract(dtStart));
            }
        }
    }
}