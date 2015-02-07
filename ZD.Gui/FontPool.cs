using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Text;

namespace ZD.Gui
{
    internal class FontPool
    {
        /// <summary>
        /// My private font collection - loaded from file.
        /// </summary>
        private static PrivateFontCollection fonts = new PrivateFontCollection();

        /// <summary>
        /// Static ctor: loads fonts deployed with Zydeo.
        /// </summary>
        static FontPool()
        {
            fonts.AddFontFile("ukaitw.ttf");
        }

        /// <summary>
        /// Get a specific font.
        /// </summary>
        public static Font GetFont(string family, float size, FontStyle style)
        {
            Font fnt = null;
            foreach (FontFamily ff in fonts.Families)
            {
                if (ff.Name == family)
                {
                    fnt = new Font(ff, size, style);
                    break;
                }
            }
            if (fnt == null) return new Font(family, size, style);
            else return fnt;
        }
    }
}
