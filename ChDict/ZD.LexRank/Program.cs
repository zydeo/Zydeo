using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace ZD.LexRank
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Takes these arguments:\r\n");
                Console.WriteLine("1: HSK cats file\r\n");
                Console.WriteLine("2: Wictionary rank file\r\n");
                Console.WriteLine("3: Subtlex rank file\r\n");
                Console.WriteLine("4: Output file: ordered headwords\r\n");
                if (Debugger.IsAttached) Console.ReadLine();
                return -1;
            }

            StreamReader srHsk = null;
            StreamReader srWict = null;
            StreamReader srSubtle = null;
            StreamWriter swOut = null;
            try
            {
                srHsk = new StreamReader(args[0]);
                srWict = new StreamReader(args[1]);
                srSubtle = new StreamReader(args[2]);
                swOut = new StreamWriter(args[3]);
                WrkLexRank wrk = new WrkLexRank(srHsk, srWict, srSubtle);
                wrk.Work();
                wrk.Finish(swOut);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (Debugger.IsAttached) Console.ReadLine();
                return -1;
            }
            finally
            {
                if (srHsk != null) srHsk.Dispose();
                if (srWict != null) srWict.Dispose();
                if (srSubtle != null) srSubtle.Dispose();
                if (swOut != null) swOut.Dispose();
            }

            return 0;
        }
    }
}
