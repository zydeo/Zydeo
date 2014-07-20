using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using DND.Common;

namespace DND.CedictEngine
{
    public class DictEngine : ICedictEngine
    {
        private readonly Index index;

        public DictEngine(string dictFileName)
        {
            using (BinReader br = new BinReader(dictFileName))
            {
                int idxPos = br.ReadInt();
                br.Position = idxPos;
                index = new Index(br);
            }
        }

        public CedictLookupResult Lookup(string what, SearchScript script, SearchLang lang)
        {
            List<CedictResult> res = new List<CedictResult>();
            return new CedictLookupResult(new ReadOnlyCollection<CedictResult>(res), lang);
        }
    }
}
