using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;

using ZD.Common;

namespace ZD.CedictEngine
{
    partial class CedictCompiler
    {
        /// <summary>
        /// Indexes of each covered hanzi in temporary file.
        /// </summary>
        private readonly int[] hanziInfoIdx = new int[65536];

        /// <summary>
        /// Current line number in Makemeahanzi input file.
        /// </summary>
        private int hanziLineNum = 0;

        /// <summary>
        /// Name of temporary file for storing hanzi info in first phase of compilation.
        /// </summary>
        private readonly string hanziTempFileName;

        /// <summary>
        /// Temporary file with binary serialized hanzi info.
        /// </summary>
        private BinWriter hanziTempWriter;

        /// <summary>
        /// Appends Hanzi repo from temp file to end of dictionary.
        /// </summary>
        private void writeHanziRepo(BinWriter bw, int hrepoIdxPos)
        {
            // Where are we now? Update pointer at start of compiled file.
            bw.MoveToEnd();
            int hanziRepoIdx = bw.Position;
            bw.Position = hrepoIdxPos;
            bw.WriteInt(hanziRepoIdx);
            bw.MoveToEnd();
            // Serialize info index - first pass, with unadjusted indexes
            int cnt = 0;
            foreach (int x in hanziInfoIdx) if (x != 0) ++cnt;
            bw.WriteInt(cnt);
            for (int i = 0; i != hanziInfoIdx.Length; ++i)
            {
                if (hanziInfoIdx[i] == 0) continue;
                bw.WriteChar((char)i);
                bw.WriteInt(hanziInfoIdx[i]);
            }
            // Update index of each HanziInfo
            int ofs = bw.Position;
            for (int i = 0; i != hanziInfoIdx.Length; ++i)
            {
                if (hanziInfoIdx[i] == 0) continue;
                hanziInfoIdx[i] += ofs;
            }
            // Serialize info index - second pass, overwrite existing data, now with updated indexes
            bw.Position = hanziRepoIdx;
            bw.WriteInt(cnt);
            for (int i = 0; i != hanziInfoIdx.Length; ++i)
            {
                if (hanziInfoIdx[i] == 0) continue;
                bw.WriteChar((char)i);
                bw.WriteInt(hanziInfoIdx[i]);
            }
            // Finish off temp file writer; append temp file's content.
            hanziTempWriter.Dispose();
            hanziTempWriter = null;
            using (FileStream fs = new FileStream(hanziTempFileName, FileMode.Open, FileAccess.Read))
            using (BinaryReader brr = new BinaryReader(fs))
            {
                while (true)
                {
                    byte[] buf = brr.ReadBytes(4096);
                    if (buf.Length == 0) break;
                    bw.WriteBytes(buf);
                    if (buf.Length != 4096) break;
                }
            }
        }

        /// <summary>
        /// Parses one line from Makemeahanzi input file.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="logStream"></param>
        public void ProcessHanziLine(string line, StreamWriter logStream)
        {
            ++hanziLineNum;
            if (!line.StartsWith("{")) return;
            HanziParser hp = new HanziParser(line);
            HanziInfo hi = null;
            char c;
            try
            {
                hp.Parse();
                c = hp.Hanzi;
                hi = hp.GetHanziInfo();
                hanziInfoIdx[(int)c] = hanziTempWriter.Position;
                hi.Serialize(hanziTempWriter);
            }
            catch (Exception ex)
            {
                string msg = "Hanzi Line {0}: ERROR: Failed to parse: {1}: {2}";
                msg = string.Format(msg, lineNum, ex.GetType().Name, ex.Message);
                logStream.WriteLine(msg);
            }
        }

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
                foreach (string stroke in strokesSplit)
                {
                    string snorm = stroke.Trim('"');
                    //while (true)
                    //{
                    //    string spaceNorm = snorm.Replace("  ", " ");
                    //    if (spaceNorm.Length == snorm.Length) break;
                    //    snorm = spaceNorm;
                    //}
                    strokes.Add(snorm);
                }
                string mediansAll = ms.Groups[2].Value;
                string[] mediansSplit = mediansAll.Split(new string[] { "]],[[" }, StringSplitOptions.None);
                foreach (string oneMedian in mediansSplit)
                {
                    List<Tuple<short, short>> oneParsedMedian = new List<Tuple<short, short>>();
                    string x = oneMedian.Replace("[[", "");
                    x = x.Replace("]]", "");
                    string[] oneSplit = x.Split(new string[] { "],[" }, StringSplitOptions.None);
                    foreach (string pair in oneSplit)
                    {
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
}
