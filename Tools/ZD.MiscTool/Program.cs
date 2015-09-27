using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ZD.MiscTool
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
            Console.WriteLine("--pinyin    <cedict-file> <output-file>");
            Console.WriteLine("  Parses dictionary file and collects pinyin syllables");
            Console.WriteLine();
            Console.WriteLine("--fontscope <cedict-file> <gb-file> <big5-file> <uni-file> <z-file> <out-chars-file> <out-simp-to-trad-file>");
            Console.WriteLine("  Parses CEDICT headwords and font coverage files");
            Console.WriteLine("  Produces statistics/font coverage of characters found in headwords");
            Console.WriteLine("  Produces simplified > traditional forms list from headwords");
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
            if (args[0] == "--pinyin")
            {
                if (args.Length != 3) return null;
                OptPinyinSyllables opt = new OptPinyinSyllables(args[1], args[2]);
                return opt;
            }
            if (args[0] == "--fontscope")
            {
                if (args.Length != 8) return null;
                OptFontScope opt = new OptFontScope(args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
                return opt;
            }
            return null;
        }

        private static IWorker createWorker(object opt)
        {
            if (opt is OptCharStats) return new WrkCharStats(opt as OptCharStats);
            if (opt is OptStrokes) return new WrkStrokes(opt as OptStrokes);
            if (opt is OptPinyinSyllables) return new WrkPinyinSyllables(opt as OptPinyinSyllables);
            if (opt is OptFontScope) return new WrkFontScope(opt as OptFontScope);
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
