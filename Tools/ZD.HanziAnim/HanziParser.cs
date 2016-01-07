using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

using ZD.Common;

namespace ZD.HanziAnim
{
    /// <summary>
    /// <para>Shameless, regex-based "parser" for Makemeahanzi's JSON format.</para>
    /// <para>A throw-away object, use one to parse each line from input.</para>
    /// </summary>
    class HanziParser
    {
        private static Regex reFix1 = new Regex("{\"character\":\"(.)\".+\"decomposition\":\"([^\"]+)\".+\"radical\":\"(.)\"");
        private static Regex reFix2 = new Regex("\"phonetic\":\"(.)\",\"semantic\":\"(.)\"");
        private static Regex reSM = new Regex("\"strokes\":\\[(\"[^\\]]+)\\],\"medians\":\\[([^\"]+)\\],\"");

        private string json;
        private List<string> strokes = new List<string>();
        private List<List<Tuple<short, short>>> medians = new List<List<Tuple<short, short>>>();
        
        private char hanzi;
        /// <summary>
        /// This Hanzi, as a Unicode character.
        /// </summary>
        public char Hanzi
        {
            get { return hanzi; }
        }

        private char radical;
        /// <summary>
        /// This Hanzi's radical, as a Unicode character.
        /// </summary>
        public char Radical
        {
            get { return radical; }
        }

        private char phon;
        /// <summary>
        /// This Hanzi's (optional) phonetic component.
        /// </summary>
        public char Phon
        {
            get { return phon; }
        }

        private char seman;
        /// <summary>
        /// This Hanzi's (optional) semantic component.
        /// </summary>
        public char Seman
        {
            get { return seman; }
        }

        private string decomp;
        /// <summary>
        /// This Hanzi's decomposition, as a Unicode string.
        /// </summary>
        public string Decomp
        {
            get { return decomp; }
        }

        /// <summary>
        /// Ctor: take JSON to parse.
        /// </summary>
        public HanziParser(string json)
        {
            this.json = json;
        }

        /// <summary>
        /// Parse JSON.
        /// </summary>
        public void Parse()
        {
            Match m1 = reFix1.Match(json);
            hanzi = m1.Groups[1].Value[0];
            decomp = m1.Groups[2].Value;
            radical = m1.Groups[3].Value[0];
            Match m2 = reFix2.Match(json);
            if (m2.Success)
            {
                phon = m2.Groups[1].Value[0];
                seman = m2.Groups[2].Value[0];
            }
            Match ms = reSM.Match(json);
            bool b = ms.Success;
            string strokesAll = ms.Groups[1].Value;
            string[] strokesSplit = strokesAll.Split(new string[] { "\",\"" }, StringSplitOptions.None);
            foreach (string stroke in strokesSplit) strokes.Add(stroke.Trim('"'));
            string mediansAll = ms.Groups[2].Value;
            string[] mediansSplit = mediansAll.Split(new string[] { "]],[[" }, StringSplitOptions.None);
            List<Tuple<short, short>> oneParsedMedian = new List<Tuple<short, short>>();
            foreach (string oneMedian in mediansSplit)
            {
                string x = oneMedian.Replace("[[", "");
                x = x.Replace("]]", "");
                string[] oneSplit = x.Split(new string[] { "],[" }, StringSplitOptions.None);
                foreach (string pair in oneSplit)
                {
                    oneParsedMedian.Clear();
                    string[] pairSplit = pair.Split(',');
                    oneParsedMedian.Add(new Tuple<short, short>(short.Parse(pairSplit[0]), short.Parse(pairSplit[1])));
                }
                medians.Add(oneParsedMedian);
            }
        }

        /// <summary>
        /// Assemble/retrieve HanziInfo object after parsing input.
        /// </summary>
        public HanziInfo GetHanziInfo()
        {
            OneStroke[] combStrokes = new OneStroke[strokes.Count];
            for (int i = 0; i != combStrokes.Length; ++i)
                combStrokes[i] = new OneStroke(strokes[i], medians[i]);
            return new HanziInfo(combStrokes, decomp, radical, phon, seman);
        }
    }
}
