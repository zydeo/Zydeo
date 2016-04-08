using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ZDO.CHSite
{
    /// <summary>
    /// Redirects caller to proper language URL if requests didn't include language prefix.
    /// </summary>
    public partial class LangRedirect : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string uri = Request.Params["uri"];
            if (uri == null) uri = "";
            // If we have a language cookie, go there
            // Otherwise, default to "hu"
            string lang = "hu";
            if (Request.Cookies["uilang"] != null)
            {
                string langFromCookie = Request.Cookies["uilang"].Value;
                if (langFromCookie == "hu") lang = "hu";
                else if (langFromCookie == "en") lang = "en";
            }
            // Redirect to fully URL with language
            Response.RedirectPermanent("/" + lang + "/" + uri, true);
        }
    }
}