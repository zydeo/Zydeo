using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ZD.DictEditor
{
    public class Settings
    {
        private static string stgsFileName = "ch-dictedit-settings.xml";

        public class SerializedData
        {
            public int ActiveEntryId = 0;
            public bool WindowStateMaximized = false;
            public int WindowW = -1;
            public int WindowH = -1;
            public int WindowX = -1;
            public int WindowY = -1;
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
                string fn = Environment.CurrentDirectory;
                fn = Path.Combine(fn, stgsFileName);
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

        public static int ActiveEntryId
        {
            get { return Data.ActiveEntryId; }
            set { Data.ActiveEntryId = value; saveData(); }
        }

        public static bool WindowStateMaximized
        {
            get { return Data.WindowStateMaximized; }
            set { Data.WindowStateMaximized = value; saveData(); }
        }

        public static int WindowX
        {
            get { return Data.WindowX; }
            set { Data.WindowX = value; saveData(); }
        }

        public static int WindowY
        {
            get { return Data.WindowY; }
            set { Data.WindowY = value; saveData(); }
        }

        public static int WindowW
        {
            get { return Data.WindowW; }
            set { Data.WindowW = value; saveData(); }
        }

        public static int WindowH
        {
            get { return Data.WindowH; }
            set { Data.WindowH = value; saveData(); }
        }
    }
}
