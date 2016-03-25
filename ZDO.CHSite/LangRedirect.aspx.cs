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
            Response.RedirectPermanent("/hu/" + uri, true);
        }
    }
}