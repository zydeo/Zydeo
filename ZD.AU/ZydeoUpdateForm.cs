using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Net;
using System.Threading;
using System.IO;

namespace ZD.AU
{
    /// <summary>
    /// UI, running in user's name, for process of downloading update and installing it.
    /// </summary>
    internal partial class ZydeoUpdateForm : Form
    {
        /// <summary>
        /// Delegate for scheduling a temp file for deletion.
        /// </summary>
        /// <param name="fname"></param>
        public delegate void ScheduleFileToDeleteDelegate(string fname);

        /// <summary>
        /// Delegete that I'll call to schedule a file for deletion when form closes or process crashes.
        /// </summary>
        private readonly ScheduleFileToDeleteDelegate scheduleFileToDelete;

        /// <summary>
        /// States of the download/update process.
        /// </summary>
        private enum State
        {
            InitFailed,
            DLoading,
            DLoadCanceled,
            DLoadFailed,
            Verifying,
            VerifyFailed,
            Installing,
            InstallFailed,
            InstallSuccess,
        }

        /// <summary>
        /// Form's original location (for home-cooked moving around on screen).
        /// </summary>
        private Point formOrigLoc = Point.Empty;

        /// <summary>
        /// Form's original position when mouse button is pressed (for home-cooked moving around on screen).
        /// </summary>
        private Point moveStart;

        /// <summary>
        /// If true, clicking button closes form; otherwise, click cancels download.
        /// </summary>
        private bool btnClosesWindow = false;

        /// <summary>
        /// If set, worker thread stops downloading the next time it looks around.
        /// </summary>
        private bool cancel = false;

        /// <summary>
        /// <para>The named pipe stream we use to communicate with service.</para>
        /// <para>Owned when created; we get rid of it in Dispose.</para>
        /// </summary>
        private NamedPipeStream pstream;

        /// <summary>
        /// If init failed, we don't even start worker thread; only one state upon load.
        /// </summary>
        private readonly bool initOK;

        /// <summary>
        /// Constructs updater form.
        /// </summary>
        /// <param name="fileToDelete">The file to delete when form is closed, or on crash.</param>
        public ZydeoUpdateForm(ScheduleFileToDeleteDelegate scheduleFileToDelete)
        {
            InitializeComponent();

            // If we're in designer, done here
            if (Process.GetCurrentProcess().ProcessName == "devenv") return;

            // Remember file delete scheduling delegate.
            this.scheduleFileToDelete = scheduleFileToDelete;

            // We want 1px to be 1px at all resolutions
            pnlOuter.Padding = new Padding(1);

            // Set image and icon
            Assembly a = Assembly.GetExecutingAssembly();
            var img = Image.FromStream(a.GetManifestResourceStream("ZD.AU.Resources.installer1.bmp"));
            pictureBox1.BackgroundImage = img;
            Icon = new Icon(a.GetManifestResourceStream("ZD.AU.Resources.ZydeoSetup.ico"));

            // Moveable by header; button event
            lblHeader.MouseDown += onHeaderMouseDown;
            lblHeader.MouseUp += onHeaderMouseUp;
            lblHeader.MouseMove += onHeaderMouseMove;
            btnClose.Click += onBtnClick;

            // Initial state
            initOK = doConnectToService();
            if (initOK) doSetStateSafe(State.DLoading);
            else doSetStateSafe(State.InitFailed);
        }

        /// <summary>
        /// Dispose: free owned resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Resources created by Designer.
                if (components != null) components.Dispose();
                // Named pipe
                if (pstream != null)
                {
                    try
                    {
                        pstream.Close();
                        pstream.Dispose();
                    }
                    catch { }
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Load event handler: starts download if initialization was successful.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (initOK) ThreadPool.QueueUserWorkItem(threadFun);
            base.OnLoad(e);
        }

        /// <summary>
        /// Force-closes and disposes the window.
        /// </summary>
        public void ForceClose()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    Close();
                });
                return;
            }
            Close();
        }

        /// <summary>
        /// Handles mouse move when button is pressed, so window can be moved around on screen.
        /// </summary>
        private void onHeaderMouseMove(object sender, MouseEventArgs e)
        {
            if (formOrigLoc == Point.Empty) return;
            Point mouseLoc = lblHeader.PointToScreen(e.Location);
            Point loc = formOrigLoc;
            loc.X += mouseLoc.X - moveStart.X;
            loc.Y += mouseLoc.Y - moveStart.Y;
            Location = loc;
        }

        /// <summary>
        /// Handles mouse button release, so window can be moved around on screen.
        /// </summary>
        private void onHeaderMouseUp(object sender, MouseEventArgs e)
        {
            formOrigLoc = Point.Empty;
        }

        /// <summary>
        /// Handles mouse button press, so window can be moved around on screen.
        /// </summary>
        private void onHeaderMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            moveStart = lblHeader.PointToScreen(e.Location);
            formOrigLoc = Location;
        }

        /// <summary>
        /// Create params for drop shadow.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        /// <summary>
        /// Handles button click: close or cancel.
        /// </summary>
        private void onBtnClick(object sender, EventArgs e)
        {
            if (btnClosesWindow) Close();
            else cancel = true;
        }

        /// <summary>
        /// Connects to update service (opens named pipe stream).
        /// </summary>
        /// <returns>True on success, false on failure.</returns>
        private bool doConnectToService()
        {
            DateTime startTime = DateTime.Now;
            Exception connectEx = null;
            while (DateTime.Now.Subtract(startTime).TotalMilliseconds < Magic.ServicePipeTimeoutMsec)
            {
                try
                {
                    pstream = new NamedPipeStream(@"\\.\pipe\" + Magic.ServiceShortName, FileAccess.ReadWrite);
                    break;
                }
                catch (Exception ex)
                {
                    connectEx = ex;
                    FileLogger.Instance.LogInfo("Failed to connect to named pipe; retrying in 0.5 sec.");
                }
                Thread.Sleep(500);
            }
            if (pstream == null)
            {
                FileLogger.Instance.LogError(connectEx, "Gave up trying to connect to named pipe; last exception below.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Background thread worker function.
        /// </summary>
        private void threadFun(object ctxt)
        {
            string fname, fileHash;
            // Try to download.
            if (!doDownload(out fname, out fileHash)) return;
            // Verify file hash.
            // Service will do the same, but this preliminary check allows us to report to user.
            if (!doVerifyFile(fname, fileHash)) return;
            // Install
            doInstall(fname, fileHash);
        }

        /// <summary>
        /// Installs update.
        /// </summary>
        private void doInstall(string fname, string fhash)
        {
            try
            {
                // UI state: installing
                doSetStateSafe(State.Installing);

                // Send info over named pipe
                byte[] buf = Encoding.Unicode.GetBytes(fname);
                pstream.Write(Helper.SerializeUInt32((uint)buf.Length), 0, 4);
                pstream.Write(buf, 0, buf.Length);
                buf = Encoding.Unicode.GetBytes(fhash);
                pstream.Write(Helper.SerializeUInt32((uint)buf.Length), 0, 4);
                pstream.Write(buf, 0, buf.Length);

                // Wait for result
                buf = new byte[1];
                Helper.ForceReadBytes(pstream, ref buf, buf.Length);
                // All we like here is "installation in progress"
                if (buf[0] != Magic.SrvCodeInstallStarted)
                {
                    doSetStateSafe(State.InstallFailed);
                    return;
                }
                // Read one more: this blocks until installation is finished
                Helper.ForceReadBytes(pstream, ref buf, buf.Length);
                // Set final UI state: success or failure
                if (buf[0] == Magic.SrvCodeSuccess) doSetStateSafe(State.InstallSuccess);
                else doSetStateSafe(State.InstallFailed);
            }
            catch
            {
                doSetStateSafe(State.InstallFailed);
            }
        }

        /// <summary>
        /// Verifies downloaded file against its hash; updates state as needed.
        /// </summary>
        private bool doVerifyFile(string fname, string fhash)
        {
            try
            {
                doSetStateSafe(State.Verifying);
                Thread.Sleep(1000); // Verify can be fast
                if (!SignatureCheck.VerifySignature(new FileInfo(fname), fhash))
                {
                    doSetStateSafe(State.VerifyFailed);
                    return false;
                }
            }
            catch
            {
                doSetStateSafe(State.VerifyFailed);
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Even to signal completion of download.
        /// </summary>
        ManualResetEvent dloadCompleteEvent = new ManualResetEvent(false);

        /// <summary>
        /// If download completed with failure, exception is placed here before setting event.
        /// </summary>
        Exception dloadException = null;

        /// <summary>
        /// Last time download progress was updated. Protected by locking on dloadCompleteEvent object.
        /// </summary>
        DateTime dloadLastProgress;

        /// <summary>
        /// Downloads update, reports failure and updates states as needed.
        /// </summary>
        private bool doDownload(out string fname, out string fileHash)
        {
            WebClient wc = null;
            string url, urlHash;
            fileHash = null;
            fname = null;
            // Start download
            try
            {
                // First, figure out what to download.
                UpdateInfo.GetDownloadInfo(out url, out urlHash, out fileHash);
                // Verify URL hash
                if (!SignatureCheck.VerifySignature(url, urlHash))
                {
                    // No good: download fails.
                    doSetStateSafe(State.DLoadFailed);
                    return false;
                }
                // Get to download. Schedule file we'll be downloading for deletion right now.
                fname = Helper.GetTempExePath();
                scheduleFileToDelete(fname);
                dloadLastProgress = DateTime.Now;
                wc = new WebClient();
                wc.UseDefaultCredentials = true;
                wc.Proxy = WebRequest.GetSystemWebProxy();
                wc.DownloadFileCompleted += onDownloadCompleted;
                wc.DownloadProgressChanged += onDownloadProgressChanged;
                wc.DownloadFileAsync(new Uri(url), fname);
                // Keep waiting
                while (true)
                {
                    // If done, break out of cycle.
                    if (dloadCompleteEvent.WaitOne(100)) break;
                    // Check for timeout - when did we last receive progress?
                    DateTime dtLastProgress;
                    lock (dloadCompleteEvent)
                    {
                        dtLastProgress = dloadLastProgress;
                    }
                    TimeSpan elapsed = DateTime.Now.Subtract(dtLastProgress);
                    // If timeout has elapsed, cancel download and - throw.
                    if (elapsed.TotalSeconds > Magic.DownloadTimeoutSec)
                    {
                        // This will trigger "completed", but we won't check the exception reported by it anymore.
                        wc.CancelAsync();
                        // Timeout = download fails.
                        throw new Exception("Download timed out.");
                    }
                    // Did user cancel download?
                    if (cancel)
                    {
                        // This will trigger "completed", but we won't check the exception reported by it anymore.
                        wc.CancelAsync();
                        // Set state to download canceled.
                        doSetStateSafe(State.DLoadCanceled);
                        return false;
                    }
                }
                // If completed with error, throw here
                if (dloadException != null) throw dloadException;
            }
            catch (Exception ex)
            {
                FileLogger.Instance.LogException(ex);
                doSetStateSafe(State.DLoadFailed);
                return false;
            }
            finally
            {
                if (wc != null)
                {
                    try { wc.Dispose(); }
                    catch { }
                }
            }
            return true;
        }

        /// <summary>
        /// Updates UI to reflect download progress.
        /// </summary>
        private void onDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Update last progress time. Used to detect timeout.
            lock (dloadCompleteEvent)
            {
                dloadLastProgress = DateTime.Now;
            }
            // Construct message
            double progress = ((double)e.BytesReceived) / ((double)e.TotalBytesToReceive) * 1000.0;
            double mbReceived = ((double)e.BytesReceived) / ((double)1048576);
            double mbTotal = ((double)e.TotalBytesToReceive) / ((double)1048576);
            // TO-DO: localize
            string strDetail = "{0:0.00} of {1:0.00} MB downloaded";
            strDetail = string.Format(strDetail, mbReceived, mbTotal);
            // TO-DO: time remaining
            // Update UI
            doSetDownloadProgressSafe(strDetail, (int)progress);
        }

        /// <summary>
        /// Releases event that signals to worker thread that download has finished.
        /// </summary>
        private void onDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            dloadException = e.Error;
            dloadCompleteEvent.Set();
        }

        /// <summary>
        /// Progress bar states (normal, warning error) - shown in color.
        /// </summary>
        private enum ProgressBarState
        {
            Normal = 1,
            Error = 2,
            Warning = 3,
        }

        /// <summary>
        /// Sends a message to a window. Used to change progress bar "state," a property not exposed in .NET.
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);
        
        /// <summary>
        /// Updates the UI. Not thread-safe.
        /// </summary>
        private void doUpdateUI(string strStatus, string strDetail, ProgressBarStyle pbarStyle,
            ProgressBarState pbarState, int pbarValue1000, string strButtonText, bool btnEnabled)
        {
            lblStatus.Text = strStatus;
            lblDetail.Text = strDetail;
            pbar.Style = pbarStyle;
            pbar.Value = pbarValue1000;
            SendMessage(pbar.Handle, 1040, (IntPtr)pbarState, IntPtr.Zero);
            btnClose.Text = strButtonText;
            btnClose.Enabled = btnEnabled;
        }

        /// <summary>
        /// Updates UI to show download progress. Always invokes; called from worker thread.
        /// </summary>
        private void doSetDownloadProgressSafe(string strDetail, int pbarValue1000)
        {
            Invoke((MethodInvoker)delegate
            {
                lblDetail.Text = strDetail;
                pbar.Value = pbarValue1000;
            });
        }

        /// <summary>
        /// Updates the UI to reflect the current state. Thread-safe.
        /// </summary>
        private void doSetStateSafe(State state)
        {
            string strStatus;
            string strDetail;
            ProgressBarStyle pbarStyle;
            ProgressBarState pbarState;
            int pbarValue;
            string strButtonText;
            bool btnEnabled;
            // TO-DO: Localize
            switch (state)
            {
                case State.DLoading:
                    strStatus = "Downloading update...";
                    strDetail = "";
                    pbarStyle = ProgressBarStyle.Continuous;
                    pbarState = ProgressBarState.Normal;
                    pbarValue = 0;
                    strButtonText = "&Cancel";
                    btnEnabled = true;
                    btnClosesWindow = false;
                    break;
                case State.DLoadCanceled:
                    strStatus = "Download canceled";
                    strDetail = "You have canceled the download before it completed. You can continue using the previous version of Zydeo on your computer, and update later.";
                    pbarStyle = ProgressBarStyle.Continuous;
                    pbarState = ProgressBarState.Warning;
                    pbarValue = 1000;
                    strButtonText = "&Close";
                    btnEnabled = true;
                    btnClosesWindow = true;
                    break;
                case State.DLoadFailed:
                    strStatus = "Download failed";
                    strDetail = "Failed to download update package. Zydeo has not been updated.";
                    pbarStyle = ProgressBarStyle.Continuous;
                    pbarState = ProgressBarState.Error;
                    pbarValue = 1000;
                    strButtonText = "&Close";
                    btnEnabled = true;
                    btnClosesWindow = true;
                    break;
                case State.Verifying:
                    strStatus = "Verifying update...";
                    strDetail = "Zydeo is verifying the update package.";
                    pbarStyle = ProgressBarStyle.Marquee;
                    pbarState = ProgressBarState.Normal;
                    pbarValue = 0;
                    strButtonText = "&Cancel";
                    btnEnabled = false;
                    btnClosesWindow = false;
                    break;
                case State.VerifyFailed:
                    strStatus = "Invalid update package";
                    strDetail = "Zydeo failed to verify the update package it has just downloaded. This may be because of an error on the Zydeo site. You can continue using the previous version of Zydeo on your computer, and update later.";
                    pbarStyle = ProgressBarStyle.Continuous;
                    pbarState = ProgressBarState.Error;
                    pbarValue = 1000;
                    strButtonText = "&Close";
                    btnEnabled = true;
                    btnClosesWindow = true;
                    break;
                case State.Installing:
                    strStatus = "Installing update...";
                    strDetail = "Please wait while the update is being installed.\r\nDo not start Zydeo until this step is finished.";
                    pbarStyle = ProgressBarStyle.Marquee;
                    pbarState = ProgressBarState.Normal;
                    pbarValue = 0;
                    strButtonText = "&Cancel";
                    btnEnabled = false;
                    btnClosesWindow = false;
                    break;
                case State.InstallFailed:
                    strStatus = "Failed to install update";
                    strDetail = "An error occurred while installing the update. You can continue using the previous version of Zydeo on your computer, and update later.";
                    pbarStyle = ProgressBarStyle.Continuous;
                    pbarState = ProgressBarState.Error;
                    pbarValue = 1000;
                    strButtonText = "&Close";
                    btnEnabled = true;
                    btnClosesWindow = true;
                    break;
                case State.InstallSuccess:
                    strStatus = "Finished";
                    strDetail = "Awesome! Zydeo's latest version is now installed for you.";
                    pbarStyle = ProgressBarStyle.Continuous;
                    pbarState = ProgressBarState.Normal;
                    pbarValue = 1000;
                    strButtonText = "&Close";
                    btnEnabled = true;
                    btnClosesWindow = true;
                    break;
                default:
                    throw new Exception("Unhandled state:" + state.ToString());
            }
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    doUpdateUI(strStatus, strDetail, pbarStyle, pbarState, pbarValue, strButtonText, btnEnabled);
                });
            }
            else doUpdateUI(strStatus, strDetail, pbarStyle, pbarState, pbarValue, strButtonText, btnEnabled);
        }
    }
}
