using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using System.IO;

namespace Site
{
    public partial class StaticContentCtrl : System.Web.UI.UserControl
    {
        private readonly string page;
        private readonly string lang;

        public StaticContentCtrl()
        { }

        public StaticContentCtrl(string page, string lang)
        {
            this.page = page;
            this.lang = lang;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected override void Render(HtmlTextWriter writer)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            string fileName = "Site.Statics." + lang + "." + page + ".txt";
            using (Stream s = a.GetManifestResourceStream(fileName))
            using (StreamReader sr = new StreamReader(s))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                    writer.WriteLine(line);
            }
        }
    }
}