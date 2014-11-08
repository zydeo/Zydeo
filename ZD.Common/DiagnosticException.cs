using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// Diagnostic exception thrown to test error handling scenarios.
    /// </summary>
    public class DiagnosticException : Exception
    {
        /// <summary>
        /// If true, exception is to be handled locally if possible (e.g., character recognition or dictionary lookup).
        /// </summary>
        public readonly bool HandleLocally;

        /// <summary>
        /// Ctor: initialize.
        /// </summary>
        /// <param name="handleLocally">True if exception is to be handled locally. See also <seealso cref="HandleLocally"/>.</param>
        public DiagnosticException(bool handleLocally)
        {
            HandleLocally = handleLocally;
        }
    }
}
