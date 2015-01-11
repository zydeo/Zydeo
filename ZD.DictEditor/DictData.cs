using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

using ZD.ChDict.Common;

namespace ZD.DictEditor
{
    public partial class DictData
    {
        private readonly HwBoundCollection hwColl;

        public static DictData InitFromXml(string xmlFileName)
        {
            return new DictData(xmlFileName);
        }

        private DictData(string xmlFileName)
        {
            List<HwData> hwList = new List<HwData>();
            int id = 0;
            using (StreamReader sr = new StreamReader(xmlFileName))
            using (XmlTextReader xr = new XmlTextReader(sr))
            {
                while (true)
                {
                    if (xr.NodeType != XmlNodeType.Element) { xr.Read(); continue; }
                    if (xr.Name != "backbone") continue;
                    break;
                }
                xr.Read();
                while (xr.NodeType != XmlNodeType.Element) xr.Read();
                BackboneEntry be;
                while ((be = BackboneEntry.ReadFromXml(xr)) != null)
                {
                    HwData hwd = new HwData(id, HwStatus.NotStarted, be.Simp, be.Trad, be.Pinyin, string.Empty);
                    hwList.Add(hwd);
                    id += 10;
                }
            }
            hwColl = new HwBoundCollection(new ReadOnlyCollection<HwData>(hwList));
        }

        public HwBoundCollection Headwords
        {
            get { return hwColl; }
        }
    }
}
