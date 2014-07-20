using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using DND.Common;
using DND.CedictEngine;

namespace DND.CedictCompileTool
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Takes three arguments:\r\n");
                Console.WriteLine("1: CEDICT input file\r\n");
                Console.WriteLine("2: Compiled dictionary file\r\n");
                Console.WriteLine("3: Folder for diagnostics/log");
                return -1;
            }

            StreamReader cedictIn = null;
            StreamWriter logStream = null;
            try
            {
                cedictIn = new StreamReader(args[0]);
                string logFileName = Path.Combine(args[2], "ccomp.log");
                logStream = new StreamWriter(logFileName);
                CedictCompiler cc = new CedictCompiler();
                string line;
                while ((line = cedictIn.ReadLine()) != null)
                {
                    cc.ProcessLine(line, logStream);
                }
                cc.WriteResults(args[1], args[2]);
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
    }
}
