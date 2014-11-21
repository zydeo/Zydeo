using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

using ZD.Gui;
using ZD.Texts;

namespace ZD
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private static CedictEngineFactory cef = new CedictEngineFactory();
        private static TextProvider tprov = new TextProvider("en");
        private static MainForm mf;

        static void mainCore()
        {

            if (Environment.OSVersion.Version.Major >= 6) SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mf = new MainForm(cef, tprov);
            Application.Run(mf.WinForm);
        }
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ZD.Gui.AppErrorLogger.Instance = new FileErrorLogger();
            AppDomain.CurrentDomain.UnhandledException += onUnhandledException;
            mainCore();
        }

        private static void onUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // We must do the processing in a different thread that is STA
            // Source of exception may be worker thread, which is MTA
            // MTA cannot show UI, and that's precisely what we want to do.
            Thread thread = new Thread(doHandleExceptionLastResort);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start(e.ExceptionObject);
            // Must wait for thread to complete.
            // Otherwise, running timers and other thread may throw again
            // And then we go straight against the wall with a Windows error report.
            thread.Join();
            Environment.Exit(-1);
        }

        private static void doHandleExceptionLastResort(object o)
        {
            Exception ex = null;
            // About all the swallowed exceptions.
            // We have already thrown. If the handler throws too, all bets are really of - nothing left to do here.
            try
            {
                // Get out exception object. I don't think this can fail, but still safer inside try block.
                ex = o as Exception;
                // Close main window
                mf.ForceClose();
            }
            catch { }
            // Log error to file. This we know is thread-safe.
            ZD.Gui.AppErrorLogger.Instance.LogException(ex, true);
            // On to error form
            try
            {
                // Show nice error form
                using (FatalErrorForm f = new FatalErrorForm(tprov))
                {
                    f.ShowDialog();
                }
            }
            catch { }
        }
    }
}
