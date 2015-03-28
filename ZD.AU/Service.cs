using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Security.AccessControl;
using System.Security.Principal;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ZD.AU
{
    /// <summary>
    /// <para>Update helper service. As a service, all it does is re-launch from a TEMP folder.</para>
    /// <para>Also contains actual function that listens to updater UI's wishes through named pipe, and runs installer.</para>
    /// </summary>
    internal partial class Service : ServiceBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public Service()
        {
            InitializeComponent();
        }

        /// <summary>
        /// When started, re-launches from TEMP folder and stops immediately.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            if (!Helper.IsRunningFromTemp())
            {
                // Running from original location, launch ourselves from temp
                Helper.StartFromTemp();

                // Stop service
                Program.ServiceToRun.Stop();

                return;
            }
            else throw new Exception("This should not happen.");
        }

        /// <summary>
        /// Nothing particular to do when stopping.
        /// </summary>
        protected override void OnStop()
        {
        }

        /// <summary>
        /// Runs as LOCAL SYSTEM from temp path; receives info from updater UI; checks package; executes installer.
        /// </summary>
        public static void Work()
        {
            // The service pipe we're listening through.
            NamedPipeStream pstream = null; 
            try
            {
                // Create service pipe
                FileLogger.Instance.LogInfo("Creating named pipe.");
                FileSecurity pipeSecurity = new FileSecurity();
                pipeSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-5-11"), FileSystemRights.FullControl, AccessControlType.Allow));
                pstream = NamedPipeStream.Create(Magic.ServiceShortName, NamedPipeStream.ServerMode.Bidirectional, pipeSecurity);

                // Wait for client (updater UI) to connect; but not too long.
                FileLogger.Instance.LogInfo("Waiting for client to connect.");
                if (!doListen(ref pstream)) throw new Exception("Client didn't show up; tired of waiting.");
                FileLogger.Instance.LogInfo("Client connected, reading paths and hash.");

                // Read location of downloaded installer and its file hash.
                string fname, fhash;
                doReadRequest(pstream, out fname, out fhash);

                // Verify signature
                FileLogger.Instance.LogInfo("Info received; verifying signature.");
                if (!SignatureCheck.VerifySignature(new FileInfo(fname), fhash))
                    throw new Exception("Signature incorrect.");
                FileLogger.Instance.LogInfo("Installer signature OK; launching installer");

                // Let caller know we've started installer
                pstream.WriteByte(Magic.SrvCodeInstallStarted);

                // Launch installer
                int exitCode = doRunInstaller(fname);
                FileLogger.Instance.LogInfo("Installer returned exit code " + exitCode.ToString());

                // Exit code 1 is failure
                if (exitCode == 1) throw new Exception("Installer failed.");

                // We've succeeded; let caller know.
                pstream.WriteByte(Magic.SrvCodeSuccess);
                FileLogger.Instance.LogInfo("Finished with success; quitting.");
            }
            catch
            {
                // Upon error, return failure code to caller.
                if (pstream != null)
                {
                    try { pstream.WriteByte(Magic.SrvCodeFail); }
                    catch { }
                }
                throw;
            }
            finally
            {
                // Close & dispose of service pipe before we exit left.
                if (pstream != null)
                {
                    try
                    {
                        pstream.Close();
                        pstream.Dispose();
                        pstream = null;
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Executes the installer.
        /// </summary>
        /// <param name="fname">The EXE to run.</param>
        /// <returns>The installer's exit code.</returns>
        private static int doRunInstaller(string fname)
        {
            ProcessStartInfo psi = new ProcessStartInfo(fname, "/SP- /SILENT /VERYSILENT /SUPPRESSMSGBOXES");
            Process proc = Process.Start(psi);
            proc.WaitForExit();
            return proc.ExitCode;
        }

        /// <summary>
        /// Reads local path of installer, and its EXE signature, from named pipe.
        /// </summary>
        private static void doReadRequest(NamedPipeStream pstream, out string fname, out string fhash)
        {
            // Buffer to hold a long integer.
            byte[] longBuf = new byte[4];

            // Reading EXE Path length
            Helper.ForceReadBytes(pstream, ref longBuf, longBuf.Length);
            uint strlen = Helper.DeserializeUInt32(longBuf);
            if (strlen > 4096) throw new Exception("Way too much info: " + strlen + " bytes.");

            // Deserializing EXE Path
            byte[] strbuf = new byte[strlen];
            Helper.ForceReadBytes(pstream, ref strbuf, strbuf.Length);
            fname = Encoding.Unicode.GetString(strbuf);

            // Reading binary hash length
            Helper.ForceReadBytes(pstream, ref longBuf, longBuf.Length);
            strlen = Helper.DeserializeUInt32(longBuf);
            if (strlen > 4096) throw new Exception("Way too much info: " + strlen + " bytes.");

            // Deserializing binary hash
            strbuf = new byte[strlen];
            Helper.ForceReadBytes(pstream, ref strbuf, strbuf.Length);
            fhash = Encoding.Unicode.GetString(strbuf);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteFile(string lpFileName);

        private static int isConnected = 0;
        private static object isConnectedLO = new object();

        /// <summary>
        /// Waits a bit, then deletes name pipe's file. Ugly, but the only way to achieve a quasi-timeout.
        /// </summary>
        private static void doClosePipeAfterWaiting(object ctxt)
        {
            NamedPipeStream pstream = (NamedPipeStream)ctxt;
            Thread.Sleep(Magic.ServicePipeTimeoutMsec);
            lock (isConnectedLO)
            {
                // If still no connection has been made, kill named pipe now.
                if (isConnected == 0)
                {
                    // It doesn't get uglier than this. But it works.
                    // http://stackoverflow.com/questions/1353263/how-to-unblock-connectnamedpipe-and-readfile-c
                    DeleteFile(@"\\.\pipe\" + Magic.ServiceShortName);
                    isConnected = -1;
                }
            }
        }

        /// <summary>
        /// Waits for incoming connection over named pipe.
        /// </summary>
        /// <returns>True if connected, false if got tired of waiting.</returns>
        private static bool doListen(ref NamedPipeStream pstream)
        {
            ThreadPool.QueueUserWorkItem(doClosePipeAfterWaiting, pstream);
            // This call blocks. It will return in one of two ways:
            // 1: Client connected
            // 2: File was deleted
            // Both return true :(
            pstream.Listen();
            lock (isConnectedLO)
            {
                // -1 value here means thread killed the named pipe.
                if (isConnected == -1)
                {
                    pstream.Dispose();
                    pstream = null;
                    return false;
                }
                // Tell thread not to kill named pipe now
                isConnected = 1;
                return true;
            }
        }
    }
}
