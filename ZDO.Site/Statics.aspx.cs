using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Site
{
    public partial class Statics : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string page = Request.Params["page"];
            staticContent.Controls.Add(new StaticContentCtrl(page, Master.UILang));
            if (page == "about")
                Title = TextProvider.Instance.GetString(Master.UILang, "TitleAbout");
            else if (page == "options")
                Title = TextProvider.Instance.GetString(Master.UILang, "TitleOptions");
            else if (page == "cookies")
                Title = TextProvider.Instance.GetString(Master.UILang, "TitleCookies");
            pageHeader.Text = Title;
        }
    }
}