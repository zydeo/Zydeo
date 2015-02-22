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

        public static bool IsService()
        {
            return WindowsIdentity.GetCurrent().IsSystem;
        }

        public static bool IsRunningFromTemp()
        {
            string assLoc = Assembly.GetExecutingAssembly().Location;
            return assLoc.StartsWith(Path.GetTempPath(), StringComparison.InvariantCultureIgnoreCase);
        }

        public static string GetTempExePath()
        {
            string tempPath = Path.GetTempPath();

            // Generate temp .exe file name
            string tempExeName = Path.GetRandomFileName().Replace(".", "") + ".exe";
            return Path.Combine(tempPath, tempExeName);
        }

        public static void StartFromTemp()
        {
            string tempExePath = GetTempExePath();

            // Make a copy of ourselves to the temp path, and start it
            File.Copy(Assembly.GetExecutingAssembly().Location, tempExePath);
            ProcessStartInfo psi = new ProcessStartInfo(tempExePath, Process.GetCurrentProcess().Id.ToString());
            Process tempProc = Process.Start(psi);

            // Try to schedule deletion upon reboot
            MoveFileEx(tempExePath, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
        }

        public static void WaitForProcessExit(int pid)
        {
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.WaitForExit();
            }
            catch { }
        }

        public static bool IsServiceRegistered()
        {
            foreach (var svc in ZydeoServiceController.GetServices())
                if (svc.ServiceName.Equals(Magic.ServiceShortName))
                    return true;
            return false;
        }

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

        public static void ForceReadBytes(Stream str, ref byte[] buffer, int length)
        {
            int readSoFar = 0;
            while (readSoFar < length)
                readSoFar += str.Read(buffer, readSoFar, length - readSoFar);
        }

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

        public static UInt32 DeserializeUInt32(byte[] input)
        {
            if (input.Length != 4) throw new ArgumentException("Invalid byte length");

            return (UInt32)input[0] | ((UInt32)input[1]) << 8 | ((UInt32)input[2]) << 16 | ((UInt32)input[3]) << 24;
        }

        public static string CalculateSHA1Hash(string Path)
        {
            using (FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
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
