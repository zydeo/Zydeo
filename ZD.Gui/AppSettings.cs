using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Drawing;

using ZD.Common;

namespace ZD.Gui
{
    /// <summary>
    /// <para>Represents persistent user settings, both explicit and implicit (like window size).</para>
    /// <para>Not thread-safe, only access from UI thread.</para>
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Actual XML serialized data; public read-write members.
        /// </summary>
        public class SerializedData
        {
            public bool NotifyOfUpdates = true;
            public int WindowLogicalSizeW = Magic.WinDefaultLogicalSize.Width;
            public int WindowLogicalSizeH = Magic.WinDefaultLogicalSize.Height;
            public int WindowPosX = int.MinValue;
            public int WindowPosY = int.MinValue;
            public static readonly string SearchLangZho = "chinese";
            public static readonly string SearchLangTrg = "target";
            public string SearchLang = SearchLangZho;
            public static readonly string ScriptSimp = "simplifed";
            public static readonly string ScriptTrad = "traditional";
            public static readonly string ScriptBoth = "both";
            public string SearchScript = ScriptSimp;
        }

        /// <summary>
        /// <para>Static serialized data object. Constructed (loaded or default) on-demand.</para>
        /// <para>Do not access directly; use <see cref="Data"/> property.</para>
        /// </summary>
        private static SerializedData __data = null;

        /// <summary>
        /// The full file path. Use through <see cref="FilePath"/> property.
        /// </summary>
        private static string __filePath = null;

        /// <summary>
        /// Wraps <see cref="__data"/> in on-demand loading/construction.
        /// </summary>
        /// <remarks>
        /// Initialization is not thread-safe but doesn't need to be.
        /// Only ever accessed from UI thread. OK?
        /// </remarks>
        private static SerializedData Data
        {
            get
            {
                // If not there yet, load it
                if (__data == null)
                {
                    __data = loadData();
                    // Load data swallows exceptions. If still null, default construct it.
                    if (__data == null) __data = new SerializedData();
                }
                // Return reference to singleton.
                return __data;
            }
        }

        /// <summary>
        /// Calculates the full path and name of the settings file if needed, then returns it.
        /// </summary>
        private static string FilePath
        {
            get
            {
                if (__filePath != null) return __filePath;
                string fn = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                fn = Path.Combine(fn, Magic.ZydeoUserFolder);
                fn = Path.Combine(fn, Magic.ZydeoSettingsFile);
                __filePath = fn;
                return __filePath;
            }
        }

        /// <summary>
        /// Loads persistent data from disk. Returns null if *any* error occurs; does not throw.
        /// </summary>
        private static SerializedData loadData()
        {
            SerializedData data = null;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(SerializedData));
                using (StreamReader sr = new StreamReader(FilePath))
                {
                    data = ser.Deserialize(sr) as SerializedData;
                }
            }
            catch { }
            return data;
        }

        /// <summary>
        /// Saves <see cref="__data"/> member to disk. Never throws. Does not save if member is null;
        /// </summary>
        private static void saveData()
        {
            if (__data == null) return;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(SerializedData));
                // Serializing first to memory stream, not to file.
                // If error occurs during serialization directly to file stream, result can be an all-zeroes file.
                // Been there, seen it.
                using (MemoryStream ms = new MemoryStream())
                {
                    ser.Serialize(ms, __data);
                    ms.Flush();
                    string dir = Path.GetDirectoryName(FilePath);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    using (FileStream fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
                    {
                        ms.WriteTo(fs);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Gets or sets whether user is to be notified of updates.
        /// </summary>
        public static bool NotifyOfUpdates
        {
            get { return Data.NotifyOfUpdates; }
            set { Data.NotifyOfUpdates = value; saveData(); }
        }

        /// <summary>
        /// Gets or sets the main window location in real screen coordinates.
        /// </summary>
        public static Point WindowLoc
        {
            get { return new Point(Data.WindowPosX, Data.WindowPosY); }
            set { Data.WindowPosX = value.X; Data.WindowPosY = value.Y; saveData(); }
        }

        /// <summary>
        /// Gets the logical, unscaled window size. (When resizing, set size and location in single call to
        /// <see cref="SetWindowSizeAndLocation"/>.
        /// </summary>
        public static Size WindowLogicalSize
        {
            get { return new Size(Data.WindowLogicalSizeW, Data.WindowLogicalSizeH); }
        }

        /// <summary>
        /// Sets the window size and location in one go, and saves settings as all setters do.
        /// </summary>
        public static void SetWindowSizeAndLocation(Point loc, Size size)
        {
            Data.WindowPosX = loc.X;
            Data.WindowPosY = loc.Y;
            Data.WindowLogicalSizeW = size.Width;
            Data.WindowLogicalSizeH = size.Height;
            saveData();
        }

        /// <summary>
        /// Gets or sets the search language.
        /// </summary>
        public static SearchLang SearchLang
        {
            get
            {
                if (Data.SearchLang.ToLowerInvariant() == SerializedData.SearchLangTrg)
                    return ZD.Common.SearchLang.Target;
                else if (Data.SearchLang.ToLowerInvariant() == SerializedData.SearchLangZho)
                    return ZD.Common.SearchLang.Chinese;
                // If data is nonsense, default to Chinese
                return ZD.Common.SearchLang.Chinese;
            }
            set
            {
                if (value == Common.SearchLang.Chinese)
                    Data.SearchLang = SerializedData.SearchLangZho.ToLowerInvariant();
                else if (value == Common.SearchLang.Target)
                    Data.SearchLang = SerializedData.SearchLangTrg.ToLowerInvariant();
                else throw new Exception("Serialization of search language value not implemented: " + value.ToString());
                saveData();
            }
        }

        /// <summary>
        /// Gets or sets the search script.
        /// </summary>
        public static SearchScript SearchScript
        {
            get
            {
                if (Data.SearchScript.ToLowerInvariant() == SerializedData.ScriptSimp)
                    return ZD.Common.SearchScript.Simplified;
                else if (Data.SearchScript.ToLowerInvariant() == SerializedData.ScriptTrad)
                    return ZD.Common.SearchScript.Traditional;
                else if (Data.SearchScript.ToLowerInvariant() == SerializedData.ScriptBoth)
                    return ZD.Common.SearchScript.Both;
                // If data is nonsense, default to simplified
                return ZD.Common.SearchScript.Simplified;
            }
            set
            {
                if (value == Common.SearchScript.Simplified)
                    Data.SearchScript = SerializedData.ScriptSimp.ToLowerInvariant();
                else if (value == Common.SearchScript.Traditional)
                    Data.SearchScript = SerializedData.ScriptTrad.ToLowerInvariant();
                else if (value == Common.SearchScript.Both)
                    Data.SearchScript = SerializedData.ScriptBoth.ToLowerInvariant();
                else throw new Exception("Serialization of search script value not implemented: " + value.ToString());
                saveData();
            }
        }
    }
}
