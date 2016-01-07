using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

using ZD.Common;

namespace ZD.HanziAnim
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<char, HanziInfo> chars = new Dictionary<char, HanziInfo>();
            
            using (StreamReader sr = new StreamReader("makemeahanzi.txt", Encoding.UTF8))
            using (BinWriter bw = new BinWriter("makemeahanzi.bin"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!line.StartsWith("{")) continue;
                    HanziParser hp = new HanziParser(line);
                    hp.Parse();
                    HanziInfo hi = hp.GetHanziInfo();
                    hi.Serialize(bw);
                }
            }
            if (Debugger.IsAttached) { Console.WriteLine("Press Enter..."); Console.ReadLine(); }
        }
    }
}
