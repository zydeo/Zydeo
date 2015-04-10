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

        private readonly static string fnUbuntu = @"Fonts\Ubuntu-Bold.ttf";
        private readonly static string fnNeuton = @"Fonts\Neuton-Regular.ttf";
        private readonly static string fnNotoSansHanS = @"Fonts\NotoSansHans-Regular.otf";
        private readonly static string fnNotoSansRegular = @"Fonts\NotoSans-Regular.ttf";
        private readonly static string fnNotoSansBold = @"Fonts\NotoSans-Bold.ttf";
        private readonly static string fnNotoSansItalic = @"Fonts\NotoSans-Italic.ttf";
        private readonly static string fnNotoSansBoldItalic = @"Fonts\NotoSans-BoldItalic.ttf";

        /// <summary>
        /// Static ctor: add custom fonts to private collection.
        /// </summary>
        static FontCollection()
        {
            fonts.AddFontFile(fnUbuntu);
            fonts.AddFontFile(fnUbuntu);
            if (File.Exists(fnNotoSansHanS)) fonts.AddFontFile(fnNotoSansHanS);
            if (File.Exists(fnNotoSansRegular)) fonts.AddFontFile(fnNotoSansRegular);
            if (File.Exists(fnNotoSansBold)) fonts.AddFontFile(fnNotoSansBold);
            if (File.Exists(fnNotoSansItalic)) fonts.AddFontFile(fnNotoSansItalic);
            if (File.Exists(fnNotoSansBoldItalic)) fonts.AddFontFile(fnNotoSansBoldItalic);
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
