using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace ZD.AU
{
    internal static class Helper
    {
        public enum MoveFileFlags
        {
            MOVEFILE_REPLACE_EXISTING = 1,
            MOVEFILE_COPY_ALLOWED = 2,
            MOVEFILE_DELAY_UNTIL_REBOOT = 4,
            MOVEFILE_WRITE_THROUGH = 8
        }

        [DllImportAttribute("kernel32.dll", EntryPoint = "MoveFileEx")]
        public static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName,
            MoveFileFlags dwFlags);

        /// <summary>
        /// Returns true if current process is running as a Windows service.
        /// </summary>
        public static bool IsService()
        {
            return WindowsIdentity.GetCurrent().IsSystem;
        }

        /// <summary>
        /// Returns true if current process is running from the TEMP folder.
        /// </summary>
        public static bool IsRunningFromTemp()
        {
            string assLoc = Assembly.GetExecutingAssembly().Location;
            return assLoc.StartsWith(Path.GetTempPath(), StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Gets a new random EXE file name in the TEMP folder.
        /// </summary>
        /// <returns></returns>
        private static string getTempExePath()
        {
            string tempPath = Path.GetTempPath();

            // Generate temp .exe file name
            string tempExeName = Path.GetRandomFileName().Replace(".", "") + ".exe";
            tempExeName = Magic.TempCopyPrefix + tempExeName;
            return Path.Combine(tempPath, tempExeName);
        }

        /// <summary>
        /// Re-launches current process from a TEMP folder; passes current PID as first argument.
        /// </summary>
        public static void StartFromTemp()
        {
            string tempExePath = getTempExePath();

            // Make a copy of ourselves to the temp path, and start it
            File.Copy(Assembly.GetExecutingAssembly().Location, tempExePath);
            ProcessStartInfo psi = new ProcessStartInfo(tempExePath, Process.GetCurrentProcess().Id.ToString());
            Process tempProc = Process.Start(psi);

            // Try to schedule deletion upon reboot
            MoveFileEx(tempExePath, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
        }

        /// <summary>
        /// Waits for a process to exit.
        /// </summary>
        public static void WaitForProcessExit(int pid)
        {
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.WaitForExit();
            }
            catch { }
        }

        /// <summary>
        /// Returns true if the AU helper service is currently registered.
        /// </summary>
        public static bool IsServiceRegistered()
        {
            foreach (var svc in ZydeoServiceController.GetServices())
                if (svc.ServiceName.Equals(Magic.ServiceShortName))
                    return true;
            return false;
        }

        /// <summary>
        /// Deletes a file without ever throwing. Marks it for deletion after restart if that don't work.
        /// </summary>
        internal static void SafeDeleteFile(string Path)
        {
            int tryCount = 5;
            bool success = false;

            while (tryCount-- > 0 && !success)
            {
                try
                {
                    File.Delete(Path);
                    success = true;
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }

            if (!success) MoveFileEx(Path, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
        }

        /// <summary>
        /// Reads exactly N number of bytes from stream.
        /// </summary>
        public static void ForceReadBytes(Stream str, ref byte[] buffer, int length)
        {
            int readSoFar = 0;
            while (readSoFar < length)
                readSoFar += str.Read(buffer, readSoFar, length - readSoFar);
        }

        /// <summary>
        /// Serializes UInt32 into a 4-byte array.
        /// </summary>
        public static byte[] SerializeUInt32(UInt32 value)
        {
            // Little endian encoding
            byte[] ret = new byte[4];

            ret[0] = (byte)(value & 255);
            ret[1] = (byte)((value >> 8) & 255);
            ret[2] = (byte)((value >> 16) & 255);
            ret[3] = (byte)((value >> 24) & 255);

            return ret;
        }

        /// <summary>
        /// Deserializes UInt32 from a 4-byte array.
        /// </summary>
        public static UInt32 DeserializeUInt32(byte[] input)
        {
            if (input.Length != 4) throw new ArgumentException("Invalid byte length");

            return (UInt32)input[0] | ((UInt32)input[1]) << 8 | ((UInt32)input[2]) << 16 | ((UInt32)input[3]) << 24;
        }

        /// <summary>
        /// Calculates SHA1 hash of string.
        /// </summary>
        public static string CalculateSHA1Hash(string str)
        {
            using (FileStream fs = new FileStream(str, FileMode.Open, FileAccess.Read))
            using (BufferedStream bs = new BufferedStream(fs))
            {
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    byte[] hash = sha1.ComputeHash(bs);
                    StringBuilder formatted = new StringBuilder(2 * hash.Length);
                    foreach (byte b in hash)
                    {
                        formatted.AppendFormat("{0:X2}", b);
                    }
                    return formatted.ToString();
                }
            }
        }
    }
}
