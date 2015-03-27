using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace ZD.AU
{
    /// <summary>
    /// Represents info about an available update. Serialized in the user's Zydeo data folder.
    /// </summary>
    public class UpdateInfo
    {
        /// <summary>
        /// Actual XML serialized data; public read-write members.
        /// </summary>
        public class SerializedData
        {
            public bool UpdateAvailable = false;
            public string UILang = "en";
            public string UpdateUrl = string.Empty;
            public string UpdateUrlHash = string.Empty;
            public string UpdateFileHash = string.Empty;
            public ushort UpdateVersionInOne = 0;
            public string UpdateReleaseDate = string.Empty; // "2015-01-29"
            public string ReleaseNotesUrl = string.Empty;
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
                fn = Path.Combine(fn, Magic.ZydeoUpdateInfoFile);
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
        /// Returns true if an update is available.
        /// </summary>
        public static bool UpdateAvailable
        {
            get
            {
                if (!Data.UpdateAvailable) return false;
                // Verify format and signature of update Url. If incorrect say no.
                try
                {
                    Uri updateUri = new Uri(Data.UpdateUrl);
                    if (updateUri.IsFile || (updateUri.Scheme != "http" && updateUri.Scheme != "https"))
                        return false;
                    if (!SignatureCheck.VerifySignature(Data.UpdateUrl, Data.UpdateUrlHash))
                        return false;
                }
                catch { return false; }
                // All good.
                return true;
            }
        }

        /// <summary>
        /// Gets UI language of main application, as it was last seen.
        /// </summary>
        public static string GetUILang()
        {
            return Data.UILang;
        }

        /// <summary>
        /// Retrieves update's download URL, and URL and file hashes. Throws if no update is actually available.
        /// </summary>
        public static void GetDownloadInfo(out string url, out string urlHash, out string fileHash)
        {
            if (!UpdateAvailable) throw new InvalidOperationException("No update is available.");
            url = Data.UpdateUrl;
            urlHash = Data.UpdateUrlHash;
            fileHash = Data.UpdateFileHash;
        }

        /// <summary>
        /// Retrieves information about an available update. Throws if data is incorrect or if there is no update.
        /// </summary>
        public static void GetUpdateInfo(out int verMajor, out int verMinor, out DateTime releaseDate,
            out string releaseNotesUrl)
        {
            if (!UpdateAvailable) throw new InvalidOperationException("No update is available.");
            verMajor = (int)Data.UpdateVersionInOne;
            verMajor >>= 8;
            verMinor = (int)Data.UpdateVersionInOne;
            verMinor &= 0xff;

            string strYear = Data.UpdateReleaseDate.Substring(0, 4);
            string strMonth = Data.UpdateReleaseDate.Substring(5, 2);
            string strDay = Data.UpdateReleaseDate.Substring(8, 2);
            releaseDate = new DateTime(int.Parse(strYear), int.Parse(strMonth), int.Parse(strDay));
            releaseNotesUrl = Data.ReleaseNotesUrl;
        }

        /// <summary>
        /// Clears update data: no update available.
        /// </summary>
        internal static void ClearUpdate()
        {
            Data.UpdateAvailable = false;
            Data.UpdateUrl = string.Empty;
            Data.UpdateUrlHash = string.Empty;
            Data.UpdateVersionInOne = 0;
            Data.UpdateReleaseDate = string.Empty;
            Data.ReleaseNotesUrl = string.Empty;
            saveData();
        }

        /// <summary>
        /// Sets information about an available update. If provided data is incorrect, stores "no udate" and throws.
        /// </summary>
        internal static void SetUpdate(string url, string urlHash, string fileHash,
            int verMajor, int verMinor, DateTime releaseDate, string releaseNotesUrl,
            string uiLang)
        {
            try
            {
                Uri updateUri = new Uri(url);
                if (updateUri.IsFile || (updateUri.Scheme != "http" && updateUri.Scheme != "https"))
                    throw new ArgumentException("Invalid update URL.");
                if (!SignatureCheck.VerifySignature(url, urlHash))
                    throw new ArgumentException("Update URL signature incorrect.");
                if (verMajor < 1 || verMajor > 255) throw new ArgumentException("Invalid major version.");
                if (verMinor < 0 || verMinor > 255) throw new ArgumentException("Invalid minor version.");
                Uri notesUri = new Uri(releaseNotesUrl);
                if (notesUri.IsFile || (notesUri.Scheme != "http" && notesUri.Scheme != "https"))
                    throw new ArgumentException("Invalid release notes URL.");

                // OK: store info
                int verInOne = verMajor;
                verInOne <<= 8;
                verInOne += verMinor;

                Data.UpdateAvailable = true;
                Data.UpdateUrl = url;
                Data.UpdateUrlHash = urlHash;
                Data.UpdateFileHash = fileHash;
                Data.UpdateVersionInOne = (ushort)verInOne;
                Data.UpdateReleaseDate = releaseDate.Year.ToString() + "-" + releaseDate.Month.ToString("00") + "-" + releaseDate.Day.ToString("00");
                Data.ReleaseNotesUrl = releaseNotesUrl;
                Data.UILang = uiLang;
                saveData();
            }
            catch
            {
                try { ClearUpdate(); }
                catch { }
                throw;
            }
        }
    }
}
