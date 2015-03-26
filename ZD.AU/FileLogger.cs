using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace ZD.AU
{
    /// <summary>
    /// An error logger that appends errors to file in user's roaming profile, or to log file in TEMP folder.
    /// </summary>
    internal class FileLogger
    {
        public static readonly FileLogger Instance = new FileLogger();

        /// <summary>
        /// Lock object around file for thread-safe access.
        /// </summary>
        private object fileLO = new object();

        /// <summary>
        /// Appends an info message to file.
        /// Never throws.
        /// </summary>
        public void LogInfo(string msg)
        {
            lock (fileLO)
            {
                try { doLogInfo(msg); }
                catch { }
            }
        }

        private void doLogInfo(string msg)
        {
            string logNameWithVersion = Helper.IsService() ?
                Magic.SvcLogFileName :
                Magic.GuiLogFileName;
            logNameWithVersion = string.Format(logNameWithVersion,
                Assembly.GetExecutingAssembly().GetName().Version.Major,
                Assembly.GetExecutingAssembly().GetName().Version.Minor);
            string fn = Helper.IsService() ?
                Path.GetTempPath() :
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Magic.ZydeoUserFolder);
            fn = Path.Combine(fn, logNameWithVersion);
            if (!Helper.IsService()) doEnsureFolder(fn);
            using (StreamWriter sw = new StreamWriter(fn, true))
            {
                string intro = "";
                DateTime dt = DateTime.Now;
                intro += dt.ToShortDateString() + " " + dt.ToShortTimeString();
                sw.WriteLine(intro + ": INFO: " + msg);
            }
        }

        /// <summary>
        /// <para>Appends exception to file; creates & opens file on demand.</para>
        /// <para>Never throws.</para>
        /// </summary>
        public void LogException(Exception ex)
        {
            lock (fileLO)
            {
                try { doLogException(ex, null); }
                catch { }
            }
        }

        /// <summary>
        /// Appens exception with custom message to file.
        /// </summary>
        public void LogError(Exception ex, string msg)
        {
            lock (fileLO)
            {
                try { doLogException(ex, msg); }
                catch { }
            }
        }

        /// <summary>
        /// Does the actual logging of an exception plus message to file.
        /// </summary>
        private void doLogException(Exception ex, string msg)
        {
            string logNameWithVersion = Helper.IsService() ?
                Magic.SvcLogFileName :
                Magic.GuiLogFileName;
            logNameWithVersion = string.Format(logNameWithVersion,
                Assembly.GetExecutingAssembly().GetName().Version.Major,
                Assembly.GetExecutingAssembly().GetName().Version.Minor);
            string fn = Helper.IsService() ?
                Path.GetTempPath() :
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Magic.ZydeoUserFolder);
            fn = Path.Combine(fn, logNameWithVersion);
            if (!Helper.IsService()) doEnsureFolder(fn);
            using (StreamWriter sw = new StreamWriter(fn, true))
            {
                string intro = "";
                DateTime dt = DateTime.Now;
                intro += dt.ToShortDateString() + " " + dt.ToShortTimeString();
                intro += ": ERROR:";
                if (msg != null) intro += " " + msg;
                sw.WriteLine(intro);
                sw.WriteLine(ex.ToString());
                sw.WriteLine();
            }
        }

        /// <summary>
        /// Creates log file's folder if it's missing.
        /// </summary>
        private void doEnsureFolder(string fullPath)
        {
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }
    }
}
