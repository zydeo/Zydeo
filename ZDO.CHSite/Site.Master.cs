using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Text;

namespace ZDO.CHSite
{
    public partial class SiteMaster : MasterPage
    {
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

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Which page are we showing?
            determinePage(Request.Path);
        }

        protected void Page_Init(object sender, EventArgs e)
        {
        }

        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}