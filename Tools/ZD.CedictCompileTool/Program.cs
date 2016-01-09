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
            if (args.Length != 5)
            {
                Console.WriteLine("Takes these arguments:");
                Console.WriteLine("1: CEDICT input file");
                Console.WriteLine("2: MakeMeAHanzi input file");
                Console.WriteLine("3: Compiled dictionary file");
                Console.WriteLine("4: Date of CEDICT release in YYYY-MM-DD format");
                Console.WriteLine("5: Folder for diagnostics/log/kept/dropped data");
                if (Debugger.IsAttached) { Console.WriteLine("Press Enter..."); Console.ReadLine(); }
                return -1;
            }

            StreamReader cedictIn = null;
            StreamReader mmahIn = null;
            StreamWriter logStream = null;
            StreamWriter outKept = null;
            StreamWriter outDropped = null;
            CedictCompiler cc = null;
            try
            {
                cedictIn = new StreamReader(args[0]);
                mmahIn = new StreamReader(args[1]);
                DateTime date = parseDate(args[3]);
                string logFileName = Path.Combine(args[4], "ccomp.log");
                logStream = new StreamWriter(logFileName);
                string outKeptName = Path.Combine(args[4], "cc-kept.txt");
                string outDroppedName = Path.Combine(args[4], "cc-drop.txt");
                outKept = new StreamWriter(outKeptName, false, Encoding.UTF8);
                outDropped = new StreamWriter(outDroppedName, false, Encoding.UTF8);
                cc = new CedictCompiler();
                string line;
                // Compile dictionary proper
                while ((line = cedictIn.ReadLine()) != null)
                {
                    cc.ProcessLine(line, logStream, outKept, outDropped);
                }
                // Compike MakeMeAHanzi
                while ((line = mmahIn.ReadLine()) != null)
                {
                    cc.ProcessHanziLine(line, logStream);
                }
                cc.WriteResults(date, args[2], args[4]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (Debugger.IsAttached) { Console.WriteLine("Press Enter..."); Console.ReadLine(); }
                return -1;
            }
            finally
            {
                if (cc != null) cc.Dispose();
                if (cedictIn != null) cedictIn.Dispose();
                if (mmahIn != null) mmahIn.Dispose();
                if (logStream != null) logStream.Dispose();
                if (outKept != null) outKept.Dispose();
                if (outDropped != null) outDropped.Dispose();
            }
            // Double-check result: does dictionary open?
            try
            {
                DictEngine de = new DictEngine(args[2], new FontCoverageFull());
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
