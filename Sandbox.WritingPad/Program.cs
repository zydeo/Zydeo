using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

using DND.HanziLookup;

namespace Sandbox
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (FileStream fs = new FileStream("strokes-extended.dat", FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                StrokesDataSource sds = new StrokesDataSource(br);
                Application.Run(new MainForm(sds));
            }
        }
    }
}
