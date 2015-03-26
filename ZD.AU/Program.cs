using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;
using System.ServiceProcess;
using System.Diagnostics;

namespace ZD.AU
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        /// <summary>
        /// The running "official" service, which will stop itself after re-launching from TEMP folder
        /// </summary>
        public static ServiceBase ServiceToRun = null;
        
        /// <summary>
        /// True if running in debugger - used to test UI directly.
        /// </summary>
        private static bool inDebugger;

        /// <summary>
        /// The form shown for downloading/installing and update.
        /// </summary>
        private static ZydeoUpdateForm uf = null;

        /// <summary>
        /// Installs the AU helper service on this system.
        /// </summary>
        private static void doInstallService()
        {
            try
            {
                // Simply exit if service has already been installed
                if (Helper.IsServiceRegistered()) return;

                string assLoc = Assembly.GetExecutingAssembly().Location;
                ServiceInstaller.InstallService(assLoc, Magic.ServiceShortName, Magic.ServiceDisplayName);
                ServiceMgr.ChangeStartMode(Magic.ServiceShortName, ServiceStartMode.Manual);
                ServiceMgr.SetServiceSDDL(Magic.ServiceShortName, System.Security.AccessControl.SecurityInfos.DiscretionaryAcl, ServiceMgr.AuSvcDaclSDDL);
                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                FileLogger.Instance.LogError(ex, "Failed to install service");
                Environment.ExitCode = -1;
            }
        }

        /// <summary>
        /// Uninstalls the AU helper service on this system.
        /// </summary>
        private static void doUninstallService()
        {
            try
            {
                ServiceInstaller.UnInstallService(Magic.ServiceShortName);
                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                FileLogger.Instance.LogError(ex, "Failed to uninstall service");
                Environment.ExitCode = -1;
            }
        }

        /// <summary>
        /// Re-launches the update UI from a TEMP folder.
        /// </summary>
        private static void doLaunchUpdate()
        {
            // Running from original location, launch ourselves from temp
            try
            {
                Helper.StartFromTemp();
            }
            catch (Exception ex)
            {
                FileLogger.Instance.LogError(ex, "Failed to launch update");
                Environment.ExitCode = -1;
            }
        }

        /// <summary>
        /// Starts the installed AU helper service. It will relaunch from TEMP folder and stop right away.
        /// Relaunched EXE from TEMP folder listens for our requests through named pipe.
        /// Still running as LOCAL SYSTEM, it has privileges to run our installer after verifying it.
        /// </summary>
        private static void doStartService()
        {
            FileLogger.Instance.LogInfo("Starting service");
            try
            {
                using (ServiceController sc = new ServiceController(Magic.ServiceShortName))
                {
                    sc.Start();
                }
            }
            catch (Exception ex)
            {
                FileLogger.Instance.LogError(ex, "Failed to start service");
            }
        }

        /// <summary>
        /// Starts the service pipe thread, through which relaunched service listens to update UI's wishes.
        /// </summary>
        private static void doStartServicePipeThread()
        {
            string pname = Process.GetCurrentProcess().ProcessName;
            FileLogger.Instance.LogInfo("Starting ServicePipeThread [" + pname + "]");
            try
            {
                ServicePipeThread spt = new ServicePipeThread();
                spt.Start();
            }
            catch (Exception ex)
            {
                FileLogger.Instance.LogError(ex, "Failed to start ServicePipeThread");
            }
        }

        /// <summary>
        /// Shows the update UI.
        /// </summary>
        private static void doUpdateForm()
        {
            FileLogger.Instance.LogInfo("Showing form");
            if (Environment.OSVersion.Version.Major >= 6) SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            uf = new ZydeoUpdateForm();
            Application.Run(uf);
        }

        /// <summary>
        /// Main entry point, within the exception cushion.
        /// </summary>
        /// <param name="args"></param>
        private static void mainCore(string[] args)
        {
            inDebugger = Debugger.IsAttached;
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "/install":
                        doInstallService();
                        return;
                    case "/uninstall":
                        doUninstallService();
                        return;
                    case "/update":
                        if (!inDebugger)
                        {
                            doLaunchUpdate();
                            return;
                        }
                        break;
               }
            }

            if (Helper.IsService() && !Helper.IsRunningFromTemp())
            {
                // Running from original location
                // Start the service, which will re-launch itself from temp location
                ServiceToRun = new ZydeoUpdateService();
                ServiceBase.Run(ServiceToRun);
                return;
            }

            if (!inDebugger && !Helper.IsRunningFromTemp())
            {
                // At this point we must be the user process
                // We should never get here
                Environment.ExitCode = -1;
                return;
            }

            // OK, we're definitely running from temp folder now. (Or debugging.)
            if (!Helper.IsService()) doStartService();

            // Running from temp as either SYSTEM or user
            // Wait until parent process exists. Parent's process ID is passed onto us as the first cmdline argument
            // BUT: don't do this in debugger; there we're running "as is", no PID argument passed.
            if (args.Length == 0) return;
            if (!inDebugger)
            {
                int parentProcessId;
                if (!int.TryParse(args[0], out parentProcessId))
                {
                    Environment.ExitCode = -1;
                    return;
                }
                Helper.WaitForProcessExit(parentProcessId);
            }

            if (Helper.IsService()) doStartServicePipeThread();
            else doUpdateForm();
        }

        /// <summary>
        /// Main entry point. Cushions actual main function with exception handling.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Touch error logger just to initialize it
            var x = FileLogger.Instance;
            AppDomain.CurrentDomain.UnhandledException += onUnhandledException;
            mainCore(args);
        }

        /// <summary>
        /// Handles unhandled app domain exceptions.
        /// </summary>
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

        /// <summary>
        /// Last-resourt exception handler. Executed on separate thread.
        /// </summary>
        /// <param name="o"></param>
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
                if (uf != null) uf.ForceClose();
            }
            catch { }
            // Log error to file. This we know is thread-safe.
            FileLogger.Instance.LogException(ex);
        }
    }
}
