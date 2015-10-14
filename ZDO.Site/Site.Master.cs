using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Site
{
    public partial class Master : System.Web.UI.MasterPage
    {
        private string uiLang = "de";

        public string UILang
        {
            get { return uiLang; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check language (from cookie)
            if (Request.Cookies["uilang"] != null)
            {
                string langFromCookie = Request.Cookies["uilang"].Value;
                if (langFromCookie == "de") uiLang = "de";
                else if (langFromCookie == "en") uiLang = "en";
                else if (langFromCookie == "jian") uiLang = "jian";
                else if (langFromCookie == "fan") uiLang = "fan";
            }
            // Hilite language in selector
            if (uiLang == "de") langselDe.Attributes["class"] = langselDe.Attributes["class"] + " active";
            else if (uiLang == "en") langselEn.Attributes["class"] = langselEn.Attributes["class"] + " active";
            else if (uiLang == "jian") langselJian.Attributes["class"] = langselJian.Attributes["class"] + " active";
            else if (uiLang == "fan") langselFan.Attributes["class"] = langselFan.Attributes["class"] + " active";

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