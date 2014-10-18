using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace ZD.MiscTool
{
    /// <summary>
    /// Parses CEDICT data and collects all syllables
    /// </summary>
    internal class WrkPinyinSyllables : IWorker
    {
        private readonly OptPinyinSyllables opt;
        private StreamReader srCedict;
        private StreamWriter swOut;

        private HashSet<string> goodSylls = new HashSet<string>();
        private HashSet<string> weirdSylls = new HashSet<string>();

        public WrkPinyinSyllables(OptPinyinSyllables opt)
        {
            this.opt = opt;
        }

        public void Init()
        {
            srCedict = new StreamReader(opt.CedictFileName);
            swOut = new StreamWriter(opt.OutFileName);
        }

        public void Dispose()
        {
            if (srCedict != null) srCedict.Dispose();
            if (swOut != null) swOut.Dispose();
        }

        private Regex reLine = new Regex(@"[^ ]+ [^ ]+ \[([^\]]+)\]");

        public void Work()
        {
            // Process file line by line
            string line;
            while ((line = srCedict.ReadLine()) != null)
            {
                if (line == "" || line.StartsWith("#")) continue;
                Match m = reLine.Match(line);
                string pinyin = m.Groups[1].Value;
                // Split pinyin syllables and process
                string[] sylls = pinyin.Split(new char[] { ' ' });
                foreach (string ps in sylls) doSyllable(ps);
            }
        }

        private Regex reSyll = new Regex(@"^([^12345]+)[12345]$");

        private void doSyllable(string syll)
        {
            Match m = reSyll.Match(syll);
            if (!m.Success)
                weirdSylls.Add(syll);
            else
                goodSylls.Add(m.Groups[1].Value.ToLower());
        }

        public void Finish()
        {
            List<string> weirdList = new List<string>();
            List<string> goodList = new List<string>();
            foreach (string s in weirdSylls) weirdList.Add(s);
            foreach (string s in goodSylls) goodList.Add(s);
            weirdList.Sort((a, b) => a.CompareTo(b));
            goodList.Sort((a, b) => a.CompareTo(b));
            foreach (string s in weirdList) swOut.WriteLine(s);
            swOut.WriteLine("--------");
            foreach (string s in goodList) swOut.WriteLine(s);
        }
    }
}
