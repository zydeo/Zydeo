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

        void Application_Start(object sender, EventArgs e)
        {
            // Unihanzi repository
            string binFilePath = HttpRuntime.AppDomainAppPath;
            binFilePath = Path.Combine(binFilePath, "_data");
            binFilePath = Path.Combine(binFilePath, "unihanzi.bin");
            hwIfno = new HeadwordInfo(binFilePath);

            // Some static config parameters
            AppSettingsReader asr = new AppSettingsReader();
            gaCode = asr.GetValue("gaCode", typeof(string)).ToString();
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
