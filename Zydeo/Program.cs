using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;
using System.Drawing;

using ZD.Gui;
using ZD.Texts;
using ZD.AU;

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

            Assembly a = Assembly.GetExecutingAssembly();
            Icon icon = new Icon(a.GetManifestResourceStream("ZD.Zydeo.ico"));

            mf = new MainForm(cef, tprov);
            mf.WinForm.Icon = icon;
            mf.WinForm.FormClosed += onFormClosed;
            doCheckUpdate();
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

        /// <summary>
        /// Checks if an update is available and tells main form. Launches deferred update check otherwise.
        /// </summary>
        private static void doCheckUpdate()
        {
            // Start deferred check - update info changes all the time
            AppUpdateChecker.UILang = "en"; // When we have UI lang settings, get this from ZD.Gui.AppSettings
            AppUpdateChecker.StartDeferredCheck(5000);
            // Any number of things can go wrong here.
            // If there's an exception, we just assume no update.
            try
            {
                // No update - done here.
                if (!UpdateInfo.UpdateAvailable) return;
                // Get details
                int vmaj, vmin;
                DateTime rdate;
                string rnotes;
                // This call, in particular, is allowed to throw if Update.xml's data does not verify
                UpdateInfo.GetUpdateInfo(out vmaj, out vmin, out rdate, out rnotes);
                // If update's version is equal to mine or smaller, then it's just data lingering around
                // from before last successful update
                Version myVer = Assembly.GetExecutingAssembly().GetName().Version;
                if (vmaj < myVer.Major || (vmaj == myVer.Major && vmin <= myVer.Minor))
                    return;
                // Tell form
                mf.SetWelcomeUpdate(vmaj, vmin, rdate, rnotes);
            }
            catch { }
        }

        /// <summary>
        /// Called when main form is closed.
        /// </summary>
        private static void onFormClosed(object sender, FormClosedEventArgs e)
        {
            if (mf.UpdateAfterClose)
            {
                System.Diagnostics.Process.Start("ZD.AU.exe", "/update");
            }
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
