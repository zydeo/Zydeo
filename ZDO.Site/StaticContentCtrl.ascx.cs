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

        public string UILang;

        public StaticContentCtrl()
        { }

        public StaticContentCtrl(string page)
        {
            this.page = page;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected override void Render(HtmlTextWriter writer)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            string fileName = "Site.Statics." + UILang + "." + page + ".txt";
            using (Stream s = a.GetManifestResourceStream(fileName))
            using (StreamReader sr = new StreamReader(s))
            {
                string line = sr.ReadLine();
                writer.WriteLine(line);
            }
        }
    }
}