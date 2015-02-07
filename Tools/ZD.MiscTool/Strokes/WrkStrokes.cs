using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Reflection;

namespace ZD.MiscTool
{
    /// <summary>
    /// <para>Creates a new strokes data file by:</para>
    /// <para>- Keeping only characters that actually occur in dictionary</para>
    /// <para>- Marking them as S / T / B based on where they actually occur in headwords</para>
    /// </summary>
    internal class WrkStrokes : IWorker
    {
        private readonly OptStrokes opt;
        private StreamReader srCharStats;
        private StreamReader srOrigStrokes;
        private StreamWriter swOut;

        private HashSet<char> hsSimp = new HashSet<char>();
        private HashSet<char> hsTrad = new HashSet<char>();
        private HashSet<char> hsBoth = new HashSet<char>();

        public WrkStrokes(OptStrokes opt)
        {
            this.opt = opt;
        }

        public void Init()
        {
            srCharStats = new StreamReader(opt.CharStatsFileName);
            srOrigStrokes = new StreamReader(opt.OrigStrokesFileName);
            swOut = new StreamWriter(opt.OutFileName);
        }

        public void Dispose()
        {
            if (srCharStats != null) srCharStats.Dispose();
            if (srOrigStrokes != null) srOrigStrokes.Dispose();
            if (swOut != null) swOut.Dispose();
        }

        public void Work()
        {
            doParseCharStats();
            doWriteBlurb();
            doStrokes();
        }

        public void Finish()
        {
            // Nothing, all is done in Work
        }

        /// <summary>
        /// Writes initial comments (copyright, history, format) at start of output file
        /// </summary>
        private void doWriteBlurb()
        {
            string blurb;
            Assembly a = Assembly.GetExecutingAssembly();
            using (Stream s = a.GetManifestResourceStream("ZD.MiscTool.Strokes.NewStrokesBlurb.txt"))
            using (StreamReader sr = new StreamReader(s))
            {
                blurb = sr.ReadToEnd();
            }
            swOut.Write(blurb);
            swOut.WriteLine();
        }

        /// <summary>
        /// Reads original strokes data; discards chars that don't occur in dictionary; outputs new format
        /// </summary>
        private void doStrokes()
        {
            // Process file line by line
            string line;
            while ((line = srOrigStrokes.ReadLine()) != null)
            {
                if (line == "" || line.StartsWith("//")) continue;
                // Character code and the rest
                string strHex = line.Substring(0, 4);
                string strRest = line.Substring(5);
                char c = (char)(int.Parse(strHex, NumberStyles.HexNumber));
                // Which type? Discard if none.
                string scriptType;
                if (hsSimp.Contains(c)) scriptType = "S";
                else if (hsTrad.Contains(c)) scriptType = "T";
                else if (hsBoth.Contains(c)) scriptType = "B";
                else continue;
                // Write output
                string lineOut = strHex + " " + scriptType + " ";
                lineOut += strRest;
                swOut.WriteLine(lineOut);
            }
        }

        /// <summary>
        /// Reads charstats file and puts each char that occurs in dictionary into right hash set
        /// </summary>
        private void doParseCharStats()
        {
            // Process file line by line
            string line;
            bool first = true;
            while ((line = srCharStats.ReadLine()) != null)
            {
                // First line contains headers; ignore empty lines
                if (first) { first = false; continue; }
                if (line == "") continue;
                // Split by tabs
                string[] parts = line.Split(new char[] { '\t' });
                // Char code in hexa; # in simplified headwords; # in traditional headwords
                string strHex = parts[0].Substring(2);
                string strSimpCount = parts[6];
                string strTradCount = parts[7];
                // Parse
                char c = (char)(int.Parse(strHex, NumberStyles.HexNumber));
                int simpCount = int.Parse(strSimpCount);
                int tradCount = int.Parse(strTradCount);
                // If char does not occur in dictionary, we skip it
                if (simpCount + tradCount == 0) continue;
                // Put into correct hash set
                if (simpCount == 0) hsTrad.Add(c);
                else if (tradCount == 0) hsSimp.Add(c);
                else hsBoth.Add(c);
            }
        }
    }
}
