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
        public static readonly string SvcLogFileName = "Zydeo.v{0}.{1}.update.log";

        public static readonly string GuiLogFileName = "Update.log";

        public static readonly string ServiceShortName = "ZydeoUpdateService";

        public static readonly string ServiceDisplayName = "Zydeo Update Helper";

        /// <summary>
        /// Zydeo folder within the user's appdata folder.
        /// </summary>
        /// <remarks>Keep in sync with <see cref="ZD.Gui.Magic.ZydeoUserFolder"/>.</remarks>
        public static readonly string ZydeoUserFolder = "Zydeo";

        public static readonly string ZydeoUpdateInfoFile = "Update.xml";

        public static readonly string UpdateCheckUrl = "http://zydeo.net/autoupdate";

        public static readonly string UpdatePostPattern = "product={0}&salt={1}&vmaj={2}&vmin={3}";

        public static readonly string UpdateProduct = "Zydeo";
    }
}
