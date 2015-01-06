using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace ZD.WikiPages
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Takes these arguments:\r\n");
                Console.WriteLine("1: Pages files\r\n");
                Console.WriteLine("2: Redirects file\r\n");
                Console.WriteLine("3: Langlinks file\r\n");
                Console.WriteLine("4: Output file\r\n");
                if (Debugger.IsAttached) Console.ReadLine();
                return -1;
            }

            StreamReader pagesIn = null;
            StreamReader redirectsIn = null;
            StreamReader langlinksIn = null;
            StreamWriter output = null;
            try
            {
                pagesIn = new StreamReader(args[0]);
                redirectsIn = new StreamReader(args[1]);
                langlinksIn = new StreamReader(args[2]);
                output = new StreamWriter(args[3]);
                WrkWikiPages wrk = new WrkWikiPages(pagesIn, redirectsIn, langlinksIn);
                wrk.Work();
                wrk.Finish(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (Debugger.IsAttached) Console.ReadLine();
                return -1;
            }
            finally
            {
                if (pagesIn != null) pagesIn.Dispose();
                if (redirectsIn != null) redirectsIn.Dispose();
                if (langlinksIn != null) langlinksIn.Dispose();
                if (output != null) output.Dispose();
            }

            return 0;
        }
    }
}
