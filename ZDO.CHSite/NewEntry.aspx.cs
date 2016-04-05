using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ZDO.CHSite
{
    public partial class NewEntry : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Add CSS files
            Master.AddCss("style.css");
            Master.AddCss("forms.css");
            Master.AddCss("newentry.css");
            Master.AddCss("entry.css");
            // Add JS includes
            Master.AddJS("newentry.js", false);
        }
    }
}