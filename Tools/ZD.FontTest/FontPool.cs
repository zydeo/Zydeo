using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Text;

namespace ZD.FontTest
{
    internal enum IdeoFont
    {
        ArphicKai,
        Noto,
        WinKai,
    }

    internal enum SimpTradFont
    {
        Simp,
        Trad,
    }

    internal class FontTray
    {
        public readonly Font Font;
        public readonly float VertOfs;
        public readonly float HorizOfs;
        public readonly float DisplayWidth;
        public readonly float DisplayHeight;

        internal FontTray(Font font, float vertOfs, float horizOfs, float displayWidth, float displayHeight)
        {
            Font = font;
            VertOfs = vertOfs;
            HorizOfs = horizOfs;
            DisplayWidth = displayWidth;
            DisplayHeight = displayHeight;
        }
    }

    internal class FontPool
    {

        /// <summary>
        /// My private font collection - loaded from file.
        /// </summary>
        private static PrivateFontCollection fonts = new PrivateFontCollection();

        /// <summary>
        /// The scale that belongs to the current DPI.
        /// </summary>
        private static float scale = 0;

        /// <summary>
        /// Static ctor: loads fonts deployed with Zydeo.
        /// </summary>
        static FontPool()
        {
            fonts.AddFontFile("ukaitw.ttf");
            fonts.AddFontFile("hdzb_75.ttf");
            fonts.AddFontFile("NotoSansHans-Regular.otf");
            fonts.AddFontFile("NotoSansHant-Regular.otf");
            fonts.AddFontFile("NotoSansHans-Light.otf");
            fonts.AddFontFile("NotoSansHant-Light.otf");
        }

        /// <summary>
        /// Sets the scale that belongs to the current DPI.
        /// </summary>
        public static float Scale
        {
            set
            {
                if (value <= 0) throw new ArgumentException();
                scale = value;
            }
        }

        /// <summary>
        /// Get a specific font.
        /// </summary>
        public static FontTray GetFont(IdeoFont ifont, SimpTradFont stfont, float size, FontStyle style)
        {
            if (scale == 0) throw new InvalidOperationException("Cannot manufacture fonts until scale has been set.");
            FontTray res = null;
            float height = size * 4F / 3F;
            height *= scale;

            if (ifont == IdeoFont.WinKai)
            {
                if (stfont == SimpTradFont.Simp)
                    res = new FontTray(
                        new Font("KaiTi", size, style), 0, -0.05F,
                        height * 0.9F, height);
                else res = new FontTray(
                     new Font("DFKai-SB", size, style), 0, -0.05F,
                     height * 0.9F, height);
                return res;
            }

            foreach (FontFamily ff in fonts.Families)
            {
                if (ifont == IdeoFont.ArphicKai && stfont == SimpTradFont.Trad && ff.Name == "AR PL UKai TW")
                    res = new FontTray(
                        new Font(ff, size, style), -0.08798828125F * height, -0.05F,
                        height * 0.9F, height);
                else if (ifont == IdeoFont.ArphicKai && stfont == SimpTradFont.Simp && ff.Name == "䡡湄楮札䍓ⵆ潮瑳")
                    res = new FontTray(
                        new Font(ff, size, style), 0, -0.05F,
                        height * 0.9F, height);
                else if (ifont == IdeoFont.Noto && stfont == SimpTradFont.Trad && ff.Name == "Noto Sans T Chinese Light")
                    res = new FontTray(
                        new Font(ff, size * 0.85F, style), height * 0.075F, height * 0.025F,
                        height * 0.9F, height);
                else if (ifont == IdeoFont.Noto && stfont == SimpTradFont.Simp && ff.Name == "Noto Sans S Chinese Regular")
                    res = new FontTray(
                        new Font(ff, size * 0.85F, style), height * 0.075F, height * 0.025F,
                        height * 0.9F, height);
                if (res != null) break;
            }
            return res;
        }
    }
}
