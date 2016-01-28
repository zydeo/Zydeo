using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Xml;

using ZD.Common;

namespace Site
{
    public static class SitemapGenerator
    {
        public static void Generate()
        {
            List<string> smaps = generateMaps();
            // Generate sitemap index
            string fnIdx = Path.Combine(HttpRuntime.AppDomainAppPath, "xsitemap-idx.xml");
            using (XmlWriter xw = XmlWriter.Create(fnIdx))
            {
                xw.WriteStartDocument();
                xw.WriteWhitespace("\r\n");
                xw.WriteStartElement("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/0.9");
                xw.WriteWhitespace("\r\n");
                foreach (var sm in smaps)
                {
                    xw.WriteStartElement("sitemap");
                    xw.WriteStartElement("loc");
                    string href = "http://handedict.zydeo.net/" + sm;
                    xw.WriteString(href);
                    xw.WriteEndElement();
                    xw.WriteStartElement("lastmod");
                    xw.WriteString("2016-01-26");
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteWhitespace("\r\n");
                }
                xw.WriteEndElement();
            }
        }

        private static List<string> generateMaps()
        {
            // Our result: list of sitemap file names generated
            List<string> res = new List<string>();
            int ix = 0;
            int cnt = 0;
            string hanzi, target;
            Global.Dict.GetFirstWords(out hanzi, out target);
            // Generate links to hanzi headwords
            StreamWriter sw = null;
            while (true)
            {
                if (cnt == 8192)
                {
                    sw.Flush(); sw.Close(); sw.Dispose();
                    sw = null;
                    ++ix;
                    cnt = 0;
                }
                if (sw == null)
                {
                    string smapName = "xsitemap-Z{0:D2}.txt";
                    smapName = string.Format(smapName, ix);
                    res.Add(smapName);
                    smapName = Path.Combine(HttpRuntime.AppDomainAppPath, smapName);
                    sw = new StreamWriter(smapName, false, Encoding.UTF8);
                }
                ++cnt;
                string line = "http://handedict.zydeo.net/search/zho/";
                line += HttpUtility.UrlEncode(hanzi);
                sw.WriteLine(line);
                string prev, next;
                Global.Dict.GetPrevNextWords(hanzi, false, out prev, out next);
                hanzi = next;
                if (hanzi == null) break;
            }
            sw.Flush(); sw.Close(); sw.Dispose();
            sw = null; cnt = 0; ix = 0;
            // Generate links to target words
            while (true)
            {
                if (cnt == 8192)
                {
                    sw.Flush(); sw.Close(); sw.Dispose();
                    sw = null;
                    ++ix;
                    cnt = 0;
                }
                if (sw == null)
                {
                    string smapName = "xsitemap-T{0:D2}.txt";
                    smapName = string.Format(smapName, ix);
                    res.Add(smapName);
                    smapName = Path.Combine(HttpRuntime.AppDomainAppPath, smapName);
                    sw = new StreamWriter(smapName, false, Encoding.UTF8);
                }
                ++cnt;
                string line = "http://handedict.zydeo.net/search/trg/";
                line += HttpUtility.UrlEncode(target);
                sw.WriteLine(line);
                string prev, next;
                Global.Dict.GetPrevNextWords(target, true, out prev, out next);
                target = next;
                if (target == null) break;
            }
            sw.Flush(); sw.Close(); sw.Dispose();
            sw = null;

            // Done.
            return res;
        }
    }
}