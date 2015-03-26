using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Text;

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
        }

        /// <summary>
        /// Creates a font - first trying from the private font collection, then from installed system fonts.
        /// </summary>
        public static Font CreateFont(string fface, float sz, FontStyle style)
        {
            Font res = null;
            foreach (FontFamily ff in fonts.Families)
            {
                if (ff.Name == fface)
                {
                    res = new Font(ff, sz, style);
                    break;
                }
            }
            if (res == null)
            {
                res = new Font(fface, sz, style);
                if (res.Name != fface) res = null;
            }
            if (res == null) throw new Exception("Requested font not available.");
            return res;
        }
    }
}
