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

        protected void Page_Load(object sender, EventArgs e)
        {
            // What UI language are we going by?
            determineLanguage();

            // Hilite language in selector
            if (uiLang == "de") langselDe.Attributes["class"] = langselDe.Attributes["class"] + " active";
            else if (uiLang == "en") langselEn.Attributes["class"] = langselEn.Attributes["class"] + " active";
            else if (uiLang == "jian") langselJian.Attributes["class"] = langselJian.Attributes["class"] + " active";
            else if (uiLang == "fan") langselFan.Attributes["class"] = langselFan.Attributes["class"] + " active";

            // Server-side localized UI in master
            TextProvider prov = TextProvider.Instance;
            navSearch.InnerText = prov.GetString(uiLang, "MenuSearch");
            navAbout.InnerText = prov.GetString(uiLang, "MenuInfo");

            // Disable "loading" class on body unless loading Default.aspx for the first time
            bool isDefault = Request.Path == @"/Default.aspx";
            if (!isDefault)
            {
                theBody.Attributes["class"] = theBody.Attributes["class"].Replace("loading", "");
            }
            // Make relevant menu item "active"
            if (Request.Path == @"/Default.aspx")
                navSearch.Attributes["class"] = navSearch.Attributes["class"] + " active";
            else if (Request.Path == @"/Statics.aspx")
            {
                string page = Request.Params["page"];
                if (page == "about")
                    navAbout.Attributes["class"] = navAbout.Attributes["class"] + " active";
            }
        }
    }
}