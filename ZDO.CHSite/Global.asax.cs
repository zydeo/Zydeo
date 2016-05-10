using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Configuration;
using System.IO;

using ZD.Common;
using ZD.CedictEngine;

namespace ZDO.CHSite
{
    public class Global : HttpApplication
    {
        private static string gaCode;

        public static string GACode
        {
            get { return gaCode; }
        }

        private static IHeadwordInfo hwIfno;
        public static IHeadwordInfo HWInfo
        {
            get { return hwIfno; }
        }

        private static string workFolder;
        public static string WorkFolder
        {
            get { return workFolder; }
        }

        private static TimeZoneInfo timeZoneInfo;
        public static TimeZoneInfo TimeZoneInfo
        {
            get { return timeZoneInfo; }
        }

        void Application_Start(object sender, EventArgs e)
        {
            // String resources
            TextProvider.Init();

            // Work folder
            string wfpath = HttpRuntime.AppDomainAppPath;
            wfpath = Path.Combine(wfpath, "_data");
            wfpath = Path.Combine(wfpath, "work");
            workFolder = wfpath;

            // Unihanzi repository
            string binFilePath = HttpRuntime.AppDomainAppPath;
            binFilePath = Path.Combine(binFilePath, "_data");
            binFilePath = Path.Combine(binFilePath, "unihanzi.bin");
            hwIfno = new HeadwordInfo(binFilePath);

            // Some static config parameters
            AppSettingsReader asr = new AppSettingsReader();
            gaCode = asr.GetValue("gaCode", typeof(string)).ToString();
            string tzname = asr.GetValue("timeZone", typeof(string)).ToString();
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(tzname);
        }

        void Application_End(object sender, EventArgs e)
        {
        }


        void Application_Error(object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;
            DiagLogger.LogError("Unhandled exception; details follow in next entry");
            Exception ex = context.Server.GetLastError().GetBaseException();
            DiagLogger.LogError(ex);
        }
    }
}
