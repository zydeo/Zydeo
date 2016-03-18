using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

using ZD.Common;

namespace ZD.UnihanCompiler
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine("Takes 5 arguments:");
                Console.WriteLine("1: Unihan_Readings.txt");
                Console.WriteLine("2: Unihan_Variants.txt");
                Console.WriteLine("3: CEDICT dictionary file");
                Console.WriteLine("4: HanDeDict dictionary file");
                Console.WriteLine("5: compiled data file");
                if (Debugger.IsAttached)
                {
                    Console.Write("Press Enter...");
                    Console.ReadLine();
                }
                return -1;
            }
            StreamReader readingsIn = null;
            StreamReader variantsIn = null;
            StreamReader cedictIn = null;
            StreamReader hanDeDictIn = null;
            BinWriter bw = null;
            try
            {
                readingsIn = new StreamReader(args[0]);
                variantsIn = new StreamReader(args[1]);
                cedictIn = new StreamReader(args[2]);
                hanDeDictIn = new StreamReader(args[3]);
                bw = new BinWriter(args[4]);

                UnihanCompiler uhc = new UnihanCompiler();
                // Compile Unihan data
                string line;
                while ((line = readingsIn.ReadLine()) != null) uhc.ReadingLine(line);
                uhc.PurgeReadingless();
                while ((line = variantsIn.ReadLine()) != null) uhc.VariantLine(line);
                uhc.WriteUnihanData(bw);
                // Compile dictionaries
                while ((line = cedictIn.ReadLine()) != null) uhc.DictLine(line, true, bw);
                while ((line = hanDeDictIn.ReadLine()) != null) uhc.DictLine(line, false, bw);
                // Finalize
                uhc.FinalizeDict(bw);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (Debugger.IsAttached)
                {
                    Console.Write("Press Enter...");
                    Console.ReadLine();
                }
                return -1;
            }
            finally
            {
                if (bw != null) bw.Dispose();
                if (hanDeDictIn != null) hanDeDictIn.Dispose();
                if (cedictIn != null) cedictIn.Dispose();
                if (variantsIn != null) variantsIn.Dispose();
                if (readingsIn != null) readingsIn.Dispose();
            }
            if (Debugger.IsAttached)
            {
                Console.Write("Press Enter...");
                Console.ReadLine();
            }
            return 0;
        }
    }
}
