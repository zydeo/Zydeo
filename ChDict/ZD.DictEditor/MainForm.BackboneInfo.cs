using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Xsl;
using System.Xml;
using System.IO;
using System.Reflection;

using ZD.ChDict.Common;

namespace ZD.DictEditor
{
    partial class MainForm
    {
        private void printBackbone(BackboneEntry be)
        {
            string xml = be.WriteToXmlStr();
            string currDir = Directory.GetCurrentDirectory();
            string fname = Path.Combine(currDir, "temp.xml");
            using (StreamWriter sw = new StreamWriter(fname))
            {
                sw.Write(xml);
                sw.Flush();
            }
            wcInfo.Navigated += onBrowserNavigated;
            wcInfo.Navigate(new Uri("file://" + fname));
        }

        private void onBrowserNavigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
        {
            wcInfo.Navigated -= onBrowserNavigated;
            string currDir = Directory.GetCurrentDirectory();
            string fname = Path.Combine(currDir, "temp.xml");
            File.Delete(fname);
        }
    }
}
