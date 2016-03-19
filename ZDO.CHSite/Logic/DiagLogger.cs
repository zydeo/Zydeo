using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
using System.Threading;

namespace ZDO.CHSite
{
    internal class DiagLogger
    {
        private static string logFileName = null;
        private static object lockObj = new object();

        static DiagLogger()
        {
            logFileName = HttpRuntime.AppDomainAppPath;
            logFileName = Path.Combine(logFileName, @"_data\logs\diaglog.txt");
            // See whether log file is writable
            safeOpenLog().Close();
        }

        private static StreamWriter safeOpenLog()
        {
            int tryCount = 10;
            int waitIncrement = 50;

            int count = 0;
            while (true)
            {
                count++;
                try
                {
                    return new StreamWriter(logFileName, true);
                }
                catch
                {
                    if (count == tryCount) throw;
                }
                Thread.Sleep(count * waitIncrement);
            }
        }

        private static void logInStyle(string message)
        {
            string msgFmt = DateTime.Now.ToString();
            msgFmt += " ";
            msgFmt += message;

            StreamWriter stream = safeOpenLog();
            if (stream != null)
            {
                stream.WriteLine(msgFmt);
                stream.Close();
            }
        }

        public static void LogError(string message)
        {
            lock (lockObj)
            {
                logInStyle(message);
            }
        }

        public static void LogError(Exception e)
        {
            lock (lockObj)
            {
                string message = "MESSAGE: " + e.Message + "\r\n\tTYPE: ";
                message += e.GetType().FullName + "\r\n\tCALL STACK:\r\n";
                message += e.StackTrace + "\r\n";
                logInStyle(message);
            }
        }
    }
}
