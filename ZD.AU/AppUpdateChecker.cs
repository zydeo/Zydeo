using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Xml;
using System.IO;
using System.Reflection;

namespace ZD.AU
{
    /// <summary>
    /// Invoked from the installed client, checks for availability of updates and stores result in settings file.
    /// </summary>
    public class AppUpdateChecker
    {
        /// <summary>
        /// When invoked from Zydeo, app can override UI language here.
        /// This value will be stored in update info XML, so update UI can choose its own display language.
        /// </summary>
        public static string UILang = "en";

        /// <summary>
        /// Starts a deferred check for updates online, from a background thread. Asynchronous.
        /// </summary>
        /// <param name="msecDelay">Time to defer online query, in msec.</param>
        public static void StartDeferredCheck(int msecDelay)
        {
            if (msecDelay < 0) throw new ArgumentException("Delay must be a positive integer.");
            ThreadPool.QueueUserWorkItem(threadFun, msecDelay);
        }

        /// <summary>
        /// Thread function for deferred checking; just takes care of delay and exception handling.
        /// </summary>
        private static void threadFun(object param)
        {
            try
            {
                int msec = (int)param;
                Thread.Sleep(msec);
                checkForUpdates();
            }
            catch
            {
                // We just swallow exceptions. Otherwise, we'd mostly write
                // a log file full of HTTP errors when user is offline.
                // Worst case, updates don't show up.
            }
        }

        /// <summary>
        /// Checks for updates online.
        /// </summary>
        private static void checkForUpdates()
        {
            // Version info and salt
            uint salt = (uint)Salt.GetSalt();
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;

            // Create web request; make sure we go through user's configured proxy, if any.
            WebRequest req = HttpWebRequest.Create(Magic.UpdateCheckUrl);
            req.Proxy = WebRequest.GetSystemWebProxy();
            req.Proxy.Credentials = CredentialCache.DefaultCredentials;
            // POST data
            string pdata = Magic.UpdatePostPattern;
            pdata = string.Format(pdata, Magic.UpdateProduct, salt, ver.Major, ver.Minor);
            byte[] data = Encoding.ASCII.GetBytes(pdata);
            // Send POST data
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            using (var stream = req.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            // Read response, which is an XML.
            // Try-finally to make sure response is closed.
            XmlDocument xmldoc = null;
            WebResponse resp = req.GetResponse();
            try
            {
                Stream respStream = resp.GetResponseStream();
                using (MemoryStream memStream = new MemoryStream())
                {
                    byte[] buffer = new byte[4096];
                    int count = 0;
                    int n = 0;
                    do
                    {
                        n = respStream.Read(buffer, 0, buffer.Length);
                        count += n;
                        if (n == 0)
                            break;
                        memStream.Write(buffer, 0, n);
                    } while (n > 0);

                    respStream.Close();
                    memStream.Seek(0, SeekOrigin.Begin);
                    using (TextReader tr = new StreamReader(memStream))
                    {
                        xmldoc = new XmlDocument();
                        xmldoc.LoadXml(tr.ReadToEnd());
                    }
                }
            }
            finally
            {
                resp.Close();
            }
            // Try and store update info.
            // If this throws, then we got trash; store that there's no update.
            try
            {
                storeUpdateInfo(xmldoc);
            }
            catch
            {
                UpdateInfo.ClearUpdate();
            }
        }

        /// <summary>
        /// Interpret XML returned from update source; store information.
        /// </summary>
        private static void storeUpdateInfo(XmlDocument xd)
        {
            // Sample response: no update
            // <update>
            //   <available>no</available>
            // </update>
            // Sample response: update available
            // <update>
            //   <available>yes</available>
            //   <url>http://zydeo.net/getupdate/ZydeoSetup-v1.1.exe</url>
            //   <urlhash>QWTTY3gffdkj343rwe+vs789</urlhash>
            //   <filehash>jhkf8r75+vds/sdf56</filehash>
            //   <vmajor>1</vmajor>
            //   <vminor>1</vminor>
            //   <releasedate>2015-01-29</releasedate>
            //   <releasenotes>http://blog.zydeo.net/release-notes-1-1</releasenotes>
            // </update>

            XmlNode root = xd["update"];
            if (root["available"].InnerText != "yes")
            {
                UpdateInfo.ClearUpdate();
                return;
            }
            // Get date out of response
            int vmaj = int.Parse(root["vmajor"].InnerText);
            int vmin = int.Parse(root["vminor"].InnerText);
            string rdateStr = root["releasedate"].InnerText;
            int year = int.Parse(rdateStr.Substring(0, 4));
            int month = int.Parse(rdateStr.Substring(5, 2));
            int day = int.Parse(rdateStr.Substring(8, 2));
            DateTime rdate = new DateTime(year, month, day);
            // Store info about this update
            UpdateInfo.SetUpdate(root["url"].InnerText, root["urlhash"].InnerText, root["filehash"].InnerText,
                vmaj, vmin, rdate, root["releasenotes"].InnerText, UILang);
        }
    }
}
