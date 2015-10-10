using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Site
{
    public partial class Site : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Disable "loading" class on body unless loading Default.aspx for the first time
            bool isDefault = Request.Path == @"/Default.aspx";
            if (!isDefault)
            {
                theBody.Attributes["class"] = theBody.Attributes["class"].Replace("loading", "");
            }
            // Make relevant menu item "active"
            if (Request.Path == @"/Default.aspx")
                navSearch.Attributes["class"] = navSearch.Attributes["class"] + " active";
            else if (Request.Path == @"/About.aspx")
                navAbout.Attributes["class"] = navAbout.Attributes["class"] + " active";
            else if (Request.Path == @"/Settings.aspx")
                navSettings.Attributes["class"] = navSettings.Attributes["class"] + " active";
        }
    }
}