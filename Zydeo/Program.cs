using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using ZD.Gui;
using ZD.Texts;

namespace ZD
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            CedictEngineFactory cef = new CedictEngineFactory();
            TextProvider tprov = new TextProvider("en");

            if (Environment.OSVersion.Version.Major >= 6) SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var mf = new MainForm(cef, tprov);
            Application.Run(mf.WinForm);
        }
    }
}
