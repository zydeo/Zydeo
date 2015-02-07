using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ZD.Common;
using ZD.CedictEngine;

namespace ZD.CedictCompileTool
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Takes these arguments:\r\n");
                Console.WriteLine("1: CEDICT input file\r\n");
                Console.WriteLine("   Tool looks for simplified/traditional coverage files in same folder\r\n");
                Console.WriteLine("   fnt-simp-coverage.txt\r\n");
                Console.WriteLine("   fnt-trad-coverage.txt\r\n");
                Console.WriteLine("   If found, only those headwords are kept that have coverage\r\n");
                Console.WriteLine("2: Compiled dictionary file\r\n");
                Console.WriteLine("3: Date of CEDICT release in YYYY-MM-DD format\r\n");
                Console.WriteLine("4: Folder for diagnostics/log");
                return -1;
            }

            StreamReader cedictIn = null;
            StreamWriter logStream = null;
            StreamWriter coverageDropStream = null;
            try
            {
                cedictIn = new StreamReader(args[0]);
                DateTime date = parseDate(args[2]);
                string logFileName = Path.Combine(args[3], "ccomp.log");
                logStream = new StreamWriter(logFileName);
                string coverageDropFileName = Path.Combine(args[3], "cdrop.txt");
                coverageDropStream = new StreamWriter(coverageDropFileName);
                HashSet<char> covSimp = null;
                HashSet<char> covTrad = null;
                parseCoverage(args[0], out covSimp, out covTrad);
                CedictCompiler cc = new CedictCompiler(covSimp, covTrad);
                string line;
                while ((line = cedictIn.ReadLine()) != null)
                {
                    cc.ProcessLine(line, logStream, coverageDropStream);
                }
                cc.WriteResults(date, args[1], args[3]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return -1;
            }
            finally
            {
                if (cedictIn != null) cedictIn.Dispose();
                if (logStream != null) logStream.Dispose();
            }

            return 0;
        }

        private static DateTime parseDate(string yyyymmdd)
        {
            string[] parts = yyyymmdd.Split(new char[] { '-' });
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);
            return new DateTime(year, month, day);
        }

        private static void parseCoverage(string cedictFileName, out HashSet<char> covSimp, out HashSet<char> covTrad)
        {
            covSimp = covTrad = null;
            string fp = Path.GetFullPath(cedictFileName);
            string dir = Path.GetDirectoryName(fp);
            string fnSimp = Path.Combine(dir, "fnt-simp-coverage.txt");
            string fnTrad = Path.Combine(dir, "fnt-trad-coverage.txt");
            if (File.Exists(fnSimp)) covSimp = parseCoverage(fnSimp);
            if (File.Exists(fnTrad)) covTrad = parseCoverage(fnTrad);
        }

        private static HashSet<char> parseCoverage(string fileName)
        {
            HashSet<char> res = new HashSet<char>();
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    string[] parts = line.Split(new char[] { '\t' });
                    string codeHex = parts[1].Substring(2);
                    int code = int.Parse(codeHex, System.Globalization.NumberStyles.HexNumber);
                    res.Add((char)code);
                }
            }
            return res;
        }
    }
}
