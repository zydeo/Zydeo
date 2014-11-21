using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// Base class for true error loggers. Acts as an interface with no real functionality.
    /// </summary>
    public class ErrorLogger
    {
        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="ex"></param>
        public virtual void LogException(Exception ex, bool fatal)
        {
            // Nop.
        }
    }
}
