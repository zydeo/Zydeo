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
        StaticContentCtrl scc;

        protected void Page_Load(object sender, EventArgs e)
        {
            string page = Request.Params["page"];
            scc = new StaticContentCtrl(page);
            theContent.Controls.Add(scc);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            scc.UILang = Master.UILang;
            base.Render(writer);
        }
    }
}