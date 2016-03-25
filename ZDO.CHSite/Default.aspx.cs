using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ZD.Common;

namespace ZDO.CHSite
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Add CSS files
            Master.AddCss("style.css");
            
            //DB.CreateTables();
            //using (SqlDict.Importer importer = new SqlDict.Importer())
            //using (StreamReader sr = new StreamReader(@"D:\Development\Zydeo\ZDO.CHSite\_data\handedict_nb_sani03.u8"))
            //{
            //    string line;
            //    while ((line = sr.ReadLine()) != null) importer.AddEntry(line);
            //    importer.CommitRest();
            //}
            //using (SqlDict.Query lookup = new SqlDict.Query())
            //{
            //    lookup.Lookup("zhi dao", SearchScript.Both, SearchLang.Chinese);
            //    lookup.Lookup("hai1", SearchScript.Both, SearchLang.Chinese);
            //    lookup.Lookup("zhi", SearchScript.Both, SearchLang.Chinese);
            //    lookup.Lookup("dao", SearchScript.Both, SearchLang.Chinese);
            //    lookup.Lookup("yi", SearchScript.Both, SearchLang.Chinese);
            //    lookup.Lookup("zhi dao yi", SearchScript.Both, SearchLang.Chinese);
            //}
            //using (SqlDict.Query lookup = new SqlDict.Query())
            //{
            //    lookup.Lookup("zhi dao", SearchScript.Both, SearchLang.Chinese);
            //    lookup.Lookup("hai1", SearchScript.Both, SearchLang.Chinese);
            //    lookup.Lookup("zhi", SearchScript.Both, SearchLang.Chinese);
            //    lookup.Lookup("dao", SearchScript.Both, SearchLang.Chinese);
            //    lookup.Lookup("yi", SearchScript.Both, SearchLang.Chinese);
            //    lookup.Lookup("zhi dao yi", SearchScript.Both, SearchLang.Chinese);
            //}
        }
    }
}