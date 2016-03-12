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
            if (args.Length != 3)
            {
                Console.WriteLine("Takes three arguments:");
                Console.WriteLine("1: Unihan_Readings.txt");
                Console.WriteLine("2: Unihan_Variants.txt");
                Console.WriteLine("3: compiled data file");
                if (Debugger.IsAttached)
                {
                    Console.Write("Press Enter...");
                    Console.ReadLine();
                }
                return -1;
            }
            StreamReader readingsIn = null;
            StreamReader variantsIn = null;
            BinWriter bw = null;
            try
            {
                readingsIn = new StreamReader(args[0]);
                variantsIn = new StreamReader(args[1]);
                bw = new BinWriter(args[2]);

                UnihanCompiler uhc = new UnihanCompiler();
                string line;
                while ((line = readingsIn.ReadLine()) != null) uhc.ReadingLine(line);
                uhc.PurgeReadingless();
                while ((line = variantsIn.ReadLine()) != null) uhc.VariantLine(line);
                uhc.WriteData(bw);
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
