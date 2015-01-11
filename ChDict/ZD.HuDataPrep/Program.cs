using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace ZD.HuDataPrep
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 6)
            {
                Console.WriteLine("Takes these arguments:");
                Console.WriteLine("1: Lexical scope file");
                Console.WriteLine("2: Wikipedia page titles file");
                Console.WriteLine("3: CEDICT file");
                Console.WriteLine("4: HanDeDict file\r\n");
                Console.WriteLine("5: Output file: Lexical backbone XML");
                Console.WriteLine("6: Output file: Summary statistics");
                if (Debugger.IsAttached) Console.ReadLine();
                return -1;
            }

            StreamReader srScope = null;
            StreamReader srWiki = null;
            StreamReader srCedict = null;
            StreamReader srHanDeDict = null;
            StreamWriter swOut = null;
            StreamWriter swStats = null;
            try
            {
                srScope = new StreamReader(args[0]);
                srWiki = new StreamReader(args[1]);
                srCedict = new StreamReader(args[2]);
                srHanDeDict = new StreamReader(args[3]);
                swOut = new StreamWriter(args[4]);
                swStats = new StreamWriter(args[5]);
                WrkHuData wrk = new WrkHuData(srScope, srWiki, srCedict, srHanDeDict);
                wrk.Work();
                wrk.Finish(swOut, swStats);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (Debugger.IsAttached) Console.ReadLine();
                return -1;
            }
            finally
            {
                if (srScope != null) srScope.Dispose();
                if (srWiki != null) srWiki.Dispose();
                if (srCedict != null) srCedict.Dispose();
                if (srHanDeDict != null) srHanDeDict.Dispose();
                if (swOut != null) swOut.Dispose();
                if (swStats != null) swStats.Dispose();
            }

            return 0;
        }
    }
}
