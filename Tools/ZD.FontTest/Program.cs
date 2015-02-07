using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace ZD.FontTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //string currDir = Directory.GetCurrentDirectory();
            //string fontFile = Path.Combine(currDir, @"hdzb_75.TTF");
            //FontCoverage.CheckCoverage(fontFile, "fnt-simp-coverage.txt");
            //string fontFile = Path.Combine(currDir, @"ukaitw.TTF");
            //FontCoverage.CheckCoverage(fontFile, "fnt-trad-coverage.txt");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
