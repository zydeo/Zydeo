using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
