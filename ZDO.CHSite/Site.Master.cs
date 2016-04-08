using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Reflection;
using System.Text;

namespace ZDO.CHSite
{
    public partial class SiteMaster : MasterPage
    {
        private string rawUrl = null;
        public string RawUrl { get { return rawUrl; } }

        private string lang = null;
        public string Lang { get { return lang; } }

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

        private UiScript uiScript = UiScript.Both;
        private UiTones uiTones = UiTones.Pleco;
        public UiScript UiScript { get { return uiScript; } }
        public UiTones UiTones { get { return uiTones; } }

        /// <summary>
        /// Safely adds a CSS class to an HtmlControl.
        /// </summary>
        private static void addClass(HtmlControl ctrl, string cls)
        {
            string curr = ctrl.Attributes["class"];
            string[] parts = curr.Split(' ');
            bool alreadyThere = false;
            string newVal = "";
            bool first = true;
            foreach (string old in parts)
            {
                if (old == "") continue;
                if (!first) newVal += " ";
                else first = false;
                newVal += old;
                if (old == cls) alreadyThere = true;
            }
            if (!alreadyThere)
            {
                if (newVal != "") newVal += " ";
                newVal += cls;
            }
            ctrl.Attributes["class"] = newVal;
        }

        /// <summary>
        /// Updates menu states to show current location.
        /// </summary>
        private void updateMenu()
        {
            // Language selector links
            langSelHu.HRef = "/hu" + rawUrl;
            langSelEn.HRef = "/en" + rawUrl;
            // Selected language
            if (lang == "en") addClass(langSelEn, "selected");
            else if (lang == "hu") addClass(langSelHu, "selected");
            // Selected top menu
            if (rawUrl == "") addClass(topMenuSearch, "selected");
            else if (rawUrl.StartsWith("/search")) addClass(topMenuSearch, "selected");
            else if (rawUrl.StartsWith("/settings")) addClass(topMenuSettings, "selected");
            else if (rawUrl.StartsWith("/edit")) addClass(topMenuEdit, "selected");
            else if (rawUrl.StartsWith("/read")) addClass(topMenuRead, "selected");
            else if (rawUrl.StartsWith("/download")) addClass(topMenuDownload, "selected");
            // Selected submenu
            if (rawUrl.StartsWith("/edit/new")) addClass(subMenuNewEntry, "selected");
            else if (rawUrl.StartsWith("/edit/history")) addClass(subMenuHistory, "selected");
            else if (rawUrl.StartsWith("/edit/change")) addClass(subMenuChange, "selected");
            else if (rawUrl.StartsWith("/read/key")) addClass(subMenuReadKey, "selected");
            else if (rawUrl.StartsWith("/read/articles")) addClass(subMenuArticles, "selected");
            else if (rawUrl.StartsWith("/read/etc")) addClass(subMenuEtc, "selected");
        }

        /// <summary>
        /// Resolves a raw link (infuses it with current language prefix).
        /// </summary>
        public string ResolveLink(string link)
        {
            return "/" + lang + link;
        }

        /// <summary>
        /// Gets the GA code to be inserted into page. Comes from config file so staging site doesn't interfere.
        /// </summary>
        protected string GetGACode()
        {
            return Global.GACode;
        }

        /// <summary>
        /// Adds a CSS file to be linked from HEAD.
        /// </summary>
        public void AddCss(string cssName)
        {
            string href = "/style-{0}/{1}";
            href = string.Format(href, VerStr, cssName);
            HtmlLink link = new HtmlLink();
            link.Href = href;
            link.Attributes.Add("rel", "stylesheet");
            link.Attributes.Add("type", "text/css");
            link.Attributes.Add("media", "all");
            this.Page.Header.Controls.Add(link);
        }

        /// <summary>
        /// Adds a JS file to be linked from end of BODY.
        /// </summary>
        public void AddJS(string jsName, bool fromLib)
        {
            if (fromLib)
                litLibJS.Text += "<script src='/lib/" + jsName + "'></script>\r\n";
            else
                litMyJS.Text += "<script src='/js-" + VerStr + "/" + jsName + "'></script>\r\n";
        }

        /// <summary>
        /// Initializes master page (site location, language etc.)
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Raw URL, without the language prefix
            rawUrl = Request.RawUrl.Substring(3);
            // What is our current language?
            // We always have this from URL b/c of rewrite rule
            lang = Request.Params["lang"];
            // Set language cookie now
            HttpCookie uilangCookie = new HttpCookie("uilang");
            uilangCookie.Value = lang;
            uilangCookie.Expires = DateTime.UtcNow.AddDays(365);
            Response.Cookies.Add(uilangCookie);
            // Infuse menu with "selected" marks
            updateMenu();

            AddJS("jquery-2.1.4.min.js", true);
            AddJS("common.js", false);
        }

        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}