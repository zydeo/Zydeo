using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace ZD.FontTest
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main()
        {
            //string currDir = Directory.GetCurrentDirectory();
            //string fontFile = Path.Combine(currDir, @"hdzb_75.TTF");
            //FontCoverage.CheckCoverage(fontFile, "fnt-simp-coverage.txt");
            //string fontFile = Path.Combine(currDir, @"ukaitw.TTF");
            //FontCoverage.CheckCoverage(fontFile, "fnt-trad-coverage.txt");

            if (Environment.OSVersion.Version.Major >= 6) SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
