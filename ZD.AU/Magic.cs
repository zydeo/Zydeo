using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.AU
{
    /// <summary>
    /// All kinds of magic constants.
    /// </summary>
    internal class Magic
    {
        /// <summary>
        /// Name of update service's log file in the TEMP folder.
        /// </summary>
        public static readonly string SvcLogFileName = "Zydeo.v{0}.{1}.update.log";

        /// <summary>
        /// Name of the update UI's log file, in the user's Zydeo appdata folder.
        /// </summary>
        public static readonly string GuiLogFileName = "Update.log";

        /// <summary>
        /// Short name of AU helper service.
        /// </summary>
        public static readonly string ServiceShortName = "ZydeoUpdateService";

        /// <summary>
        /// Display name of AU helper service.
        /// </summary>
        public static readonly string ServiceDisplayName = "Zydeo Update Helper";

        /// <summary>
        /// Prefix prepended to random-named EXE copies in TEMP folder.
        /// </summary>
        public static readonly string TempCopyPrefix = "ZydAU-";

        /// <summary>
        /// Zydeo folder within the user's appdata folder.
        /// </summary>
        /// <remarks>Keep in sync with <see cref="ZD.Gui.Magic.ZydeoUserFolder"/>.</remarks>
        public static readonly string ZydeoUserFolder = "Zydeo";

        /// <summary>
        /// Name of file storing info about latest update in user's appdata folder.
        /// </summary>
        public static readonly string ZydeoUpdateInfoFile = "Update.xml";

        /// <summary>
        /// URL that returns info about available updates.
        /// </summary>
        public static readonly string UpdateCheckUrl = "http://zydeo.net/autoupdate";

        /// <summary>
        /// POSTDATA to send to update URL.
        /// </summary>
        public static readonly string UpdatePostPattern = "product={0}&salt={1}&vmaj={2}&vmin={3}";

        /// <summary>
        /// Product for which we request update from URL. Same URL may be serving other products later.
        /// </summary>
        public static readonly string UpdateProduct = "Zydeo";

        /// <summary>
        /// Timeout, in seconds, after which download fails.
        /// </summary>
        public static readonly double DownloadTimeoutSec = 30;

        /// <summary>
        /// How long the service waits for a connection from update client, in msec.
        /// </summary>
        public static readonly int ServicePipeTimeoutMsec = 60000;
    }
}
