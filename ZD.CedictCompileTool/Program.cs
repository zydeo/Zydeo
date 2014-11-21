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
                Console.WriteLine("2: Compiled dictionary file\r\n");
                Console.WriteLine("3: Date of CEDICT release in YYYY-MM-DD format\r\n");
                Console.WriteLine("4: Folder for diagnostics/log");
                return -1;
            }

            StreamReader cedictIn = null;
            StreamWriter logStream = null;
            try
            {
                cedictIn = new StreamReader(args[0]);
                DateTime date = parseDate(args[2]);
                string logFileName = Path.Combine(args[3], "ccomp.log");
                logStream = new StreamWriter(logFileName);
                CedictCompiler cc = new CedictCompiler();
                string line;
                while ((line = cedictIn.ReadLine()) != null)
                {
                    cc.ProcessLine(line, logStream);
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
    }
}
