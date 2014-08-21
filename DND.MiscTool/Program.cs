using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DND.MiscTool
{
    class Program
    {
        private static void writeInfo()
        {
            Console.WriteLine("Invalid arguments. Usage:");
            Console.WriteLine();
            Console.WriteLine("--charstats <strokes file> <strokes-types-file> <cedict-file> <output-file>");
            Console.WriteLine("  Parses original strokes file and dictionary file");
            Console.WriteLine("  Gathers information about simplified/traditional usage and");
            Console.WriteLine("  occurrence count in headwords");
            Console.WriteLine();
            Console.WriteLine("--strokes   <char-stats-file> <original-strokes-file> <output-file>");
            Console.WriteLine("  Parses character statistics file and original strokes file");
            Console.WriteLine("  Compiles new strokes file, keeping only chars that occur in dictionary");
            Console.WriteLine("  Simplified/traditional/both comes from occurrence in headwords");
            Console.WriteLine();
        }

        private static object parseArgs(string[] args)
        {
            if (args[0] == "--charstats")
            {
                if (args.Length != 5) return null;
                OptCharStats opt = new OptCharStats(args[1], args[2], args[3], args[4]);
                return opt;
            }
            if (args[0] == "--strokes")
            {
                if (args.Length != 4) return null;
                OptStrokes opt = new OptStrokes(args[1], args[2], args[3]);
                return opt;
            }
            return null;
        }

        private static IWorker createWorker(object opt)
        {
            if (opt is OptCharStats) return new WrkCharStats(opt as OptCharStats);
            if (opt is OptStrokes) return new WrkStrokes(opt as OptStrokes);
            throw new Exception(opt.GetType().ToString() + " is not recognized as an options type.");
        }

        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                writeInfo();
                if (Debugger.IsAttached) { Console.WriteLine("Press Enter..."); Console.ReadLine(); }
                return -1;
            }

            object opt = parseArgs(args);
            if (opt == null)
            {
                writeInfo();
                if (Debugger.IsAttached) { Console.WriteLine("Press Enter..."); Console.ReadLine(); }
                return -1;
            }

            IWorker worker = createWorker(opt);
            try
            {
                worker.Init();
                worker.Work();
                worker.Finish();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (Debugger.IsAttached) { Console.WriteLine("Press Enter..."); Console.ReadLine(); }
                return -1;
            }
            finally
            {
                worker.Dispose();
            }
            return 0;
        }
    }
}
