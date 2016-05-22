using System;
using System.Web;
using System.Reflection;

namespace ZDO.CHSite
{
    public partial class Default : System.Web.UI.Page
    {
        private string lang = null;
        public string Lang { get { return lang; } }

        private string rel = null;
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
            // Set language cookie now
            HttpCookie uilangCookie = new HttpCookie("uilang");
            uilangCookie.Value = lang;
            uilangCookie.Expires = DateTime.UtcNow.AddDays(365);
            Response.Cookies.Add(uilangCookie);
            // Add proprietary JS files for single-page app
            // We add all scripts for all pages
            litJS.Text += "<script src='/js-" + VerStr + "/strings-" + lang + ".js'></script>\r\n";
            litJS.Text += "<script src='/js-" + VerStr + "/page.js'></script>\r\n";
            litJS.Text += "<script src='/js-" + VerStr + "/newentry.js'></script>\r\n";
            litJS.Text += "<script src='/js-" + VerStr + "/strokeanim.js'></script>\r\n";
            litJS.Text += "<script src='/js-" + VerStr + "/lookup.js'></script>\r\n";
            litJS.Text += "<script src='/js-" + VerStr + "/history.js'></script>\r\n";
            // DBG
            litJS.Text += "<script src='/js-" + VerStr + "/diagnostics.js'></script>\r\n";
        }
    }
}