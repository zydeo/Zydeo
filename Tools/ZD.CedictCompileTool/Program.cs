using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

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
                Console.WriteLine("4: Folder for diagnostics/log/kept/dropped data");
                return -1;
            }

            StreamReader cedictIn = null;
            StreamWriter logStream = null;
            StreamWriter outKept = null;
            StreamWriter outDropped = null;
            try
            {
                cedictIn = new StreamReader(args[0]);
                DateTime date = parseDate(args[2]);
                string logFileName = Path.Combine(args[3], "ccomp.log");
                logStream = new StreamWriter(logFileName);
                string outKeptName = Path.Combine(args[3], "cc-kept.txt");
                string outDroppedName = Path.Combine(args[3], "cc-drop.txt");
                outKept = new StreamWriter(outKeptName, false, Encoding.UTF8);
                outDropped = new StreamWriter(outDroppedName, false, Encoding.UTF8);
                CedictCompiler cc = new CedictCompiler();
                string line;
                while ((line = cedictIn.ReadLine()) != null)
                {
                    cc.ProcessLine(line, logStream, outKept, outDropped);
                }
                cc.WriteResults(date, args[1], args[3]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (Debugger.IsAttached) { Console.WriteLine("Press Enter..."); Console.ReadLine(); }
                return -1;
            }
            finally
            {
                if (cedictIn != null) cedictIn.Dispose();
                if (logStream != null) logStream.Dispose();
                if (outKept != null) outKept.Dispose();
                if (outDropped != null) outDropped.Dispose();
            }
            // Double-check result: does dictionary open?
            try
            {
                DictEngine de = new DictEngine(args[1], new FontCoverageFull());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (Debugger.IsAttached) { Console.WriteLine("Press Enter..."); Console.ReadLine(); }
                return -1;
            }
            if (Debugger.IsAttached) { Console.WriteLine("Press Enter..."); Console.ReadLine(); }
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
