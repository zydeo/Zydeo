using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ZD.Gui;
using ZD.Common;

namespace ZD
{
    /// <summary>
    /// An error logger that appends errors to file in user's roaming profile.
    /// </summary>
    internal class FileErrorLogger : ZD.Common.ErrorLogger
    {
        /// <summary>
        /// Lock object around file for thread-safe access.
        /// </summary>
        private object fileLO = new object();

        /// <summary>
        /// <para>Appends exception to file; creates & opens file on demand.</para>
        /// <para>Never throws.</para>
        /// </summary>
        public override void LogException(Exception ex, bool fatal)
        {
            lock (fileLO)
            {
                try { doLogException(ex, fatal); }
                catch { }
            }
        }

        private void doLogException(Exception ex, bool fatal)
        {
            string fn = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            fn = Path.Combine(fn, ZD.Gui.Magic.ZydeoUserFolder);
            fn = Path.Combine(fn, ZD.Gui.Magic.ZydeoErrorFile);
            using (StreamWriter sw = new StreamWriter(fn, true))
            {
                string intro = "";
                DateTime dt = DateTime.Now;
                intro += dt.ToShortDateString() + " " + dt.ToShortTimeString();
                intro += fatal ? "  --  Fatal error " : "  --  Graceful error";
                sw.WriteLine(intro);
                sw.WriteLine(ex.ToString());
                sw.WriteLine();
            }
        }
    }
}
