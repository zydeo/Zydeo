using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.IO;

namespace ZD.Gui
{
    /// <summary>
    /// Stores a collection of private fonts.
    /// </summary>
    internal class FontCollection
    {
        /// <summary>
        /// My private font collection - loaded from file.
        /// </summary>
        private static PrivateFontCollection fonts = new PrivateFontCollection();

        /// <summary>
        /// Static ctor: add custom fonts to private collection.
        /// </summary>
        static FontCollection()
        {
            fonts.AddFontFile("Ubuntu-Bold.ttf");
            fonts.AddFontFile("Neuton-Regular.ttf");
            if (File.Exists("NotoSansHans-Regular.otf")) fonts.AddFontFile("NotoSansHans-Regular.otf");
            if (File.Exists("NotoSans-Regular.ttf")) fonts.AddFontFile("NotoSans-Regular.ttf");
            if (File.Exists("NotoSans-Bold.ttf")) fonts.AddFontFile("NotoSans-Bold.ttf");
            if (File.Exists("NotoSans-Italic.ttf")) fonts.AddFontFile("NotoSans-Italic.ttf");
            if (File.Exists("NotoSans-BoldItalic.ttf")) fonts.AddFontFile("NotoSans-BoldItalic.ttf");
        }

        /// <summary>
        /// Retrieves a font family from the private collection, or return null of family's not there.
        /// </summary>
        private static FontFamily getCachedFontFamily(string fface)
        {
            foreach (FontFamily ff in fonts.Families)
            {
                if (ff.Name == fface) return ff;
            }
            return null;
        }

        /// <summary>
        /// Creates a font - first trying from the private font collection, then from installed system fonts.
        /// </summary>
        public static Font CreateFont(string fface, float sz, FontStyle style)
        {
            FontFamily cachedFF = getCachedFontFamily(fface);
            Font res = null;
            if (cachedFF != null) res = new Font(cachedFF, sz, style);
            if (res == null)
            {
                res = new Font(fface, sz, style);
                if (res.Name != fface) res = null;
            }
            if (res == null) throw new Exception("Requested font not available.");
            return res;
        }

        /// <summary>
        /// Creates a font - first trying from the private font collection, then from installed system fonts.
        /// </summary>
        public static Font CreateFont(string fface, float sz, FontStyle style, GraphicsUnit unit, byte gdiCharSet)
        {
            FontFamily cachedFF = getCachedFontFamily(fface);
            Font res = null;
            if (cachedFF != null) res = new Font(cachedFF, sz, style, unit, gdiCharSet);
            if (res == null)
            {
                res = new Font(fface, sz, style, unit, gdiCharSet);
                if (res.Name != fface) res = null;
            }
            if (res == null) throw new Exception("Requested font not available.");
            return res;
        }
    }
}
