using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace ZDO.CHSite
{
    public partial class Static : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Add CSS files
            Master.AddCss("style.css");

            // Render static content
            // TO-DO: auto-map to required file from raw query
            // TO-DO: substitute with HU where EN in missing
            string path = HttpRuntime.AppDomainAppPath;
            path = Path.Combine(path, "Content");
            path = Path.Combine(path, "WorkInProgress." + Master.Lang + ".html");
            using (StreamReader sr = new StreamReader(path))
            {
                lit.Text = sr.ReadToEnd();
            }
        }
    }
}