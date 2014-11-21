using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZD.Common;

namespace ZD.Gui
{
    /// <summary>
    /// The application's error logger; reset to genuine logger at startup by top-level project.
    /// </summary>
    public static class AppErrorLogger
    {
        /// <summary>
        /// My single error logger instance.
        /// </summary>
        public static ErrorLogger Instance = new ErrorLogger();
    }
}
