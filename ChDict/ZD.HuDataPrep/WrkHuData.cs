using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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

        public void Work()
        {
        }

        public void Finish(StreamWriter swOut, StreamWriter swStats)
        {

        }
    }
}
