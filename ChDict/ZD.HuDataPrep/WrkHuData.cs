using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;

using ZD.ChDict.Common;

namespace ZD.HuDataPrep
{
    internal class WrkHuData
    {
        private readonly StreamReader srScope;
        private readonly StreamReader srWiki;
        private readonly StreamReader srCedict;
        private readonly StreamReader srHanDeDict;

        public WrkHuData(StreamReader srScope, StreamReader srWiki, StreamReader srCedict, StreamReader srHanDeDict)
        {
            this.srScope = srScope;
            this.srWiki = srWiki;
            this.srCedict = srCedict;
            this.srHanDeDict = srHanDeDict;
        }

        private class HeadKey
        {
            public readonly string Simp;
            public readonly string Trad;
            public readonly string Pinyin;
            public HeadKey(string simp, string trad, string pinyin)
            {
                Simp = simp;
                Trad = trad;
                Pinyin = pinyin;
            }

            public override int GetHashCode()
            {
                return Simp.GetHashCode() + Trad.GetHashCode() + Pinyin.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                HeadKey other = obj as HeadKey;
                return Simp == other.Simp && Trad == other.Trad && Pinyin == other.Pinyin;
            }
        }

        private class DictInfo
        {
            public string[] CedictSenses;
            public string[] HanDeSenses;
        }

        private class ScopeItem
        {
            public readonly int Rank;
            public readonly Dictionary<HeadKey, DictInfo> Dict = new Dictionary<HeadKey, DictInfo>();
            public string WikiHu = null;
            public string WikiEn = null;
            public string WikiDe = null;
            public ScopeItem(int rank)
            {
                Rank = rank;
            }
        }

        private readonly Dictionary<string, ScopeItem> simpToItem = new Dictionary<string,ScopeItem>();
        private readonly List<string> scopeKeys = new List<string>();

        public void Work()
        {
            doReadScope();
            doReadDict(true);
            doReadDict(false);
            doReadWiki();
        }

        private void doReadWiki()
        {
            string line;
            while ((line = srWiki.ReadLine()) != null)
            {
                line = line.Trim('\t');
                string[] parts = line.Split(new char[] { '\t' });
                List<string> hws = new List<string>(parts.Length);
                hws.Add(parts[0]);
                for (int i = 4; i < parts.Length; ++i) hws.Add(parts[i]);
                foreach (string str in hws)
                {
                    if (!simpToItem.ContainsKey(str)) continue;
                    ScopeItem si = simpToItem[str];
                    if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1])) si.WikiEn = parts[1];
                    if (parts.Length > 2 && !string.IsNullOrEmpty(parts[2])) si.WikiDe = parts[2];
                    if (parts.Length > 3 && !string.IsNullOrEmpty(parts[3])) si.WikiHu = parts[3];
                }
            }
        }

        private Regex reHead = new Regex(@"^([^ ]+) ([^ ]+) \[([^\]]+)$");

        private void doReadDict(bool cedict)
        {
            StreamReader sr = cedict ? srCedict : srHanDeDict;
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                string[] parts = line.Split(new string[] { "] /" }, StringSplitOptions.None);
                // Simplified and traditional
                Match m = reHead.Match(parts[0]);
                string trad = m.Groups[1].Value;
                string simp = m.Groups[2].Value;
                string pinyin = m.Groups[3].Value;
                // Senses
                parts[1] = parts[1].Trim('/');
                string[] senses = parts[1].Split(new char[] { '/' });
                // Store
                HeadKey hk = new HeadKey(simp, trad, pinyin);
                if (!simpToItem.ContainsKey(simp)) continue;
                DictInfo di;
                if (simpToItem[simp].Dict.ContainsKey(hk)) di = simpToItem[simp].Dict[hk];
                else
                {
                    di = new DictInfo();
                    simpToItem[simp].Dict[hk] = di;
                }
                if (cedict) di.CedictSenses = senses;
                else di.HanDeSenses = senses;
            }
        }

        private void doReadScope()
        {
            string line;
            bool first = true;
            while ((line = srScope.ReadLine()) != null)
            {
                if (first) { first = false; continue; }
                string[] parts = line.Split(new char[] { '\t' });
                ScopeItem si = new ScopeItem(int.Parse(parts[0]));
                simpToItem[parts[1]] = si;
                scopeKeys.Add(parts[1]);
            }
        }

        private void doInfuseWiki(ScopeItem si, Dictionary<BackbonePart, object> storage)
        {
            if (si.WikiHu != null) storage[BackbonePart.WikiHu] = si.WikiHu;
            if (si.WikiEn != null)
            {
                TransTriple tt = new TransTriple(si.WikiEn);
                storage[BackbonePart.WikiEn] = tt;
            }
            if (si.WikiDe != null)
            {
                TransTriple tt = new TransTriple(si.WikiDe);
                storage[BackbonePart.WikiDe] = tt;
            }
        }

        public void Finish(StreamWriter swOut, StreamWriter swStats)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            settings.NewLineChars = "\r\n";
            settings.NewLineHandling = NewLineHandling.Replace;
            using (XmlWriter xw = XmlWriter.Create(swOut, settings))
            {
                xw.WriteStartElement("backbone");
                foreach (string simp in scopeKeys)
                {
                    ScopeItem si = simpToItem[simp];
                    // Not in any dictionary: don't care
                    if (si.Dict.Count == 0)
                    {
                        continue;
                    }
                    // Infuse senses from dictionary; may end up creating multiple entries (trad ambiguity)
                    foreach (var hw in si.Dict)
                    {
                        Dictionary<BackbonePart, object> storage = new Dictionary<BackbonePart, object>();
                        if (hw.Value.CedictSenses != null)
                        {
                            TransTriple[] ttCedict = new TransTriple[hw.Value.CedictSenses.Length];
                            for (int i = 0; i != hw.Value.CedictSenses.Length; ++i)
                                ttCedict[i] = new TransTriple(hw.Value.CedictSenses[i]);
                            storage[BackbonePart.Cedict] = ttCedict;
                        }
                        if (hw.Value.HanDeSenses != null)
                        {
                            TransTriple[] ttHanDe = new TransTriple[hw.Value.HanDeSenses.Length];
                            for (int i = 0; i != hw.Value.HanDeSenses.Length; ++i)
                                ttHanDe[i] = new TransTriple(hw.Value.HanDeSenses[i]);
                            storage[BackbonePart.HanDeDict] = ttHanDe;
                        }
                        doInfuseWiki(si, storage);
                        BackboneEntry be = new BackboneEntry(simp, hw.Key.Trad, hw.Key.Pinyin, si.Rank, simp, simp,
                            storage);
                        be.WriteToXml(xw);
                    }
                }
                xw.WriteEndElement();
            }
        }
    }
}
