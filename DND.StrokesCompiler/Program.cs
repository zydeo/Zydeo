using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using DND.HanziLookup;

namespace DND.StrokesCompiler
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Takes three arguments:\r\n");
                Console.WriteLine("1: the plain-text strokes data file\r\n");
                Console.WriteLine("2: the plain-text types data file\r\n");
                Console.WriteLine("3: the file to output the compiled data file to");
                return -1;
            }
            StreamReader strokesIn = null;
            StreamReader typesIn = null;
            FileStream compiledOutStream = null;
            BinaryWriter compiledOut = null;
            try
            {
                strokesIn = new StreamReader(args[0]);
                typesIn = new StreamReader(args[1]);
                compiledOutStream = new FileStream(args[2], FileMode.Create);
                compiledOut = new BinaryWriter(compiledOutStream);

                CharacterTypeParser typeParser = new CharacterTypeParser(typesIn);
                CharacterTypeRepository typeRepository = typeParser.BuildCharacterTypeRepository();

                StrokesParser strokesParser = new StrokesParser(strokesIn, typeRepository);
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
                if (typesIn != null) typesIn.Dispose();
                if (strokesIn != null) strokesIn.Dispose();
            }
            return 0;
        }
    }
}
