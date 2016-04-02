using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace ZDO.CHSite
{
    public partial class Diagnostics : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Add CSS files
            Master.AddCss("style.css");
            Master.AddCss("diagnostics.css");
            // Add JS includes
            Master.AddJS("jquery-2.1.4.min.js", true);
            Master.AddJS("diagnostics.js", false);

            // Render static content
            string path = HttpRuntime.AppDomainAppPath;
            path = Path.Combine(path, "Content");
            path = Path.Combine(path, "Diagnostics.html");
            using (StreamReader sr = new StreamReader(path))
            {
                lit.Text = sr.ReadToEnd();
            }
        }
    }
}