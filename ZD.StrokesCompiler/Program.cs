using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ZD.HanziLookup;

namespace ZD.StrokesCompiler
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Takes three arguments:\r\n");
                Console.WriteLine("1: the plain-text strokes data file\r\n");
                Console.WriteLine("2: the file to output the compiled data file to");
                return -1;
            }
            StreamReader strokesIn = null;
            FileStream compiledOutStream = null;
            BinaryWriter compiledOut = null;
            try
            {
                strokesIn = new StreamReader(args[0]);
                compiledOutStream = new FileStream(args[1], FileMode.Create);
                compiledOut = new BinaryWriter(compiledOutStream);

                StrokesParser strokesParser = new StrokesParser(strokesIn);
                strokesParser.WriteCompiledOutput(compiledOut);
                compiledOut.Flush();
                compiledOut.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return -1;
            }
            finally
            {
                if (compiledOut != null) compiledOut.Dispose();
                if (compiledOutStream != null) compiledOutStream.Dispose();
                if (strokesIn != null) strokesIn.Dispose();
            }
            return 0;
        }
    }
}
