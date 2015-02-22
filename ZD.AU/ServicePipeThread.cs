using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Xml;
using System.Reflection;

namespace ZD.AU
{
    internal class ServicePipeThread
    {
        private NamedPipeStream servicePipe;

        public ServicePipeThread()
        {
            try
            {
                FileSecurity pipeSecurity = new FileSecurity();
                pipeSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-5-11"), FileSystemRights.FullControl, AccessControlType.Allow));
                servicePipe = NamedPipeStream.Create(Magic.ServiceShortName, NamedPipeStream.ServerMode.Bidirectional, pipeSecurity);
            }
            catch (Exception ex)
            {
                File.WriteAllText(@"C:\TEMP\updatesvc.log", "ServicePipeThread.ctor:\r\n" + ex.ToString());
            }
        }

        private bool listen(int Timeout)
        {
            int passedSoFar = 0;
            while (passedSoFar < Timeout && !servicePipe.IsConnected)
            {
                Thread.Sleep(100);
                passedSoFar += 100;
            }
            return servicePipe.IsConnected;
        }

        public void Start()
        {
            string
                exePath = null,
                binaryHash = null,
                uiExePath = null;

            try
            {
                FileLogger.Instance.LogInfo("Waiting for client to connect...");
                if (!listen(60000))
                {
                    FileLogger.Instance.LogInfo("Client failed to connect, exiting...");
                    return;
                }
                FileLogger.Instance.LogInfo("Client connected, reading paths and hash");

                try
                {
                    byte[] longBuf = new byte[4];

                    // Reading EXE Path length
                    Helper.ForceReadBytes(servicePipe, ref longBuf, longBuf.Length);
                    uint strlen = Helper.DeserializeUInt32(longBuf);

                    // Deserializing EXE Path
                    byte[] strbuf = new byte[strlen];
                    Helper.ForceReadBytes(servicePipe, ref strbuf, strbuf.Length);
                    exePath = Encoding.Unicode.GetString(strbuf);

                    // Reading binary hash length
                    Helper.ForceReadBytes(servicePipe, ref longBuf, longBuf.Length);
                    strlen = Helper.DeserializeUInt32(longBuf);

                    // Deserializing binary hash
                    strbuf = new byte[strlen];
                    Helper.ForceReadBytes(servicePipe, ref strbuf, strbuf.Length);
                    binaryHash = Encoding.Unicode.GetString(strbuf);

                    // Reading UI exe path length
                    Helper.ForceReadBytes(servicePipe, ref longBuf, longBuf.Length);
                    strlen = Helper.DeserializeUInt32(longBuf);

                    // Deserializing UI exe path
                    strbuf = new byte[strlen];
                    Helper.ForceReadBytes(servicePipe, ref strbuf, strbuf.Length);
                    uiExePath = Encoding.Unicode.GetString(strbuf);

                    // Fail if binary signature is incorrect
                    if (!SignatureCheck.VerifySignature(new FileInfo(exePath), binaryHash))
                    {
                        FileLogger.Instance.LogInfo("Binary signature verification failed");
                        ReportResult(OperationResult.Error);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    FileLogger.Instance.LogError(ex, "Failed to read data from named pipe stream");
                }

                FileLogger.Instance.LogInfo("Signature verification succeeded");

                // Schedule deletion of UI exe, if its SHA1 hash equals ours
                try
                {
                    string assLoc = Assembly.GetExecutingAssembly().Location;
                    if (Helper.CalculateSHA1Hash(assLoc) == Helper.CalculateSHA1Hash(uiExePath))
                        Helper.MoveFileEx(uiExePath, null, Helper.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
                }
                catch { }

                // Starting update, errorlevel 1 means failure
                try
                {
                    servicePipe.WriteByte(128);
                    ProcessStartInfo psi = new ProcessStartInfo(exePath, "/SP- /SILENT /VERYSILENT /SUPPRESSMSGBOXES");
                    Process proc = Process.Start(psi);
                    proc.WaitForExit();
                    FileLogger.Instance.LogInfo("Installer returned with exit code " + proc.ExitCode.ToString());
                    ReportResult(proc.ExitCode != 1 ? OperationResult.Success : OperationResult.Error);
                }
                catch (Exception ex)
                {
                    FileLogger.Instance.LogError(ex, "Failed to start installer process");
                }
            }
            catch (Exception ex)
            {
                FileLogger.Instance.LogError(ex, "Unexpected error");
            }
            finally
            {
                try
                {
                    if (!String.IsNullOrEmpty(exePath)) Helper.SafeDeleteFile(exePath);
                }
                catch { }
            }
        }

        private void ReportResult(OperationResult Result)
        {
            servicePipe.WriteByte((byte)Result);
            servicePipe.Close();
        }

        enum OperationResult
        {
            Success = 130,
            Error = 131
        }
    }
}
