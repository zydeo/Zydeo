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
    /// Specifies the ideographic font family.
    /// </summary>
    public enum IdeoFamily
    {
        /// <summary>
        /// AR PL UKai, or its simplified derivative.
        /// </summary>
        ArphicKai,
        /// <summary>
        /// Noto Sans Han, Regular.
        /// </summary>
        Noto,
    }

    /// <summary>
    /// Specifies simplified or traditional style.
    /// </summary>
    public enum IdeoScript
    {
        /// <summary>
        /// Simplified.
        /// </summary>
        Simp,
        /// <summary>
        /// Traditional.
        /// </summary>
        Trad,
    }

    /// <summary>
    /// Manages private and system fonts for rendering Hanzi; measures and renders Hanzi ranges.
    /// </summary>
    internal class HanziRenderer
    {
        /// <summary>
        /// One specific font (including size) and associated typographic information.
        /// </summary>
        private class FontTray
        {
            /// <summary>
            /// The font instance used for drawing.
            /// </summary>
            public readonly Font Font;
            /// <summary>
            /// Vertical offset from desired (visually correct) Y location when drawing.
            /// </summary>
            public readonly float VertOfs;
            /// <summary>
            /// Horizontal offset from desired (visually correct) X location when drawing a single character.
            /// </summary>
            public readonly float HorizOfs;
            /// <summary>
            /// Genuine (visually correct) width of a single Hanzi on screen.
            /// </summary>
            public readonly float DisplayWidth;
            /// <summary>
            /// Genuine (visually correct) height of a single Hanzi on screen.
            /// </summary>
            public readonly float DisplayHeight;

            /// <summary>
            /// Ctor: init immutable instance.
            /// </summary>
            internal FontTray(Font font, float vertOfs, float horizOfs, float displayWidth, float displayHeight)
            {
                Font = font;
                VertOfs = vertOfs;
                HorizOfs = horizOfs;
                DisplayWidth = displayWidth;
                DisplayHeight = displayHeight;
            }
        }

        private struct FontCacheKey
        {
            private readonly IdeoFamily family;
            private readonly IdeoScript script;
            private readonly int size;
            private readonly FontStyle style;

            public FontCacheKey(IdeoFamily family, IdeoScript script, float size, FontStyle style)
            {
                this.family = family;
                this.script = script;
                this.size = (int)(100F * size);
                this.style = style;
            }

            public override int GetHashCode()
            {
                int hash = 17;
                hash = hash * 31 + (int)family;
                hash = hash * 31 + (int)script;
                hash = hash * 31 + size;
                hash = hash * 31 + (int)style;
                return hash;
            }

            public override bool Equals(object other)
            {
                return other is FontCacheKey ? Equals((FontCacheKey)other) : false;
            }

            public bool Equals(FontCacheKey other)
            {
                return family == other.family &&
                    script == other.script &&
                    size == other.size &&
                    style == other.style;
            }

        }

        /// <summary>
        /// My private font collection - loaded from file.
        /// </summary>
        private static readonly PrivateFontCollection fonts = new PrivateFontCollection();

        /// <summary>
        /// Cached 
        /// </summary>
        private static readonly Dictionary<FontCacheKey, FontTray> fontCache = new Dictionary<FontCacheKey, FontTray>();

        /// <summary>
        /// See <see cref="Scale"/>.
        /// </summary>
        private static float scale = 0;

        /// <summary>
        /// Static ctor: loads fonts deployed with Zydeo.
        /// </summary>
        static HanziRenderer()
        {
            fonts.AddFontFile("ukaitw.ttf");
            fonts.AddFontFile("hdzb_75.ttf");
            if (File.Exists("NotoSansHans-Light.otf")) fonts.AddFontFile("NotoSansHans-Light.otf");
            if (File.Exists("NotoSansHant-Light.otf")) fonts.AddFontFile("NotoSansHant-Light.otf");
        }

        /// <summary>
        /// Measures the display rectangle of a string.
        /// </summary>
        public static SizeF MeasureString(Graphics g, string text, float size)
        {
            // TO-DO: actually invoke g.MeasureString for non-Hanzi characters
            // I.e., regular and half-width alphabetical and digits

            FontTray ftray = getFont(IdeoFamily.ArphicKai, IdeoScript.Simp, size, FontStyle.Regular);
            float width = ((float)text.Length) * ftray.DisplayWidth;
            return new SizeF(width, ftray.DisplayHeight);
        }

        public static SizeF MeasureChar(Graphics g, char c, float size)
        {
            // TO-DO
            return SizeF.Empty;
        }

        public static void DrawString(Graphics g, string text, PointF loc, Brush b,
            IdeoFamily fam, IdeoScript script, float size, FontStyle style)
        {
            FontTray ftray = getFont(fam, script, size, style);
            float x = loc.X;
            float y = loc.Y + ftray.VertOfs;

            // TO-DO: actually invoke g.MeasureString for non-Hanzi characters
            // I.e., regular and half-width alphabetical and digits

            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            for (int i = 0; i != text.Length; ++i)
            {
                string chr = text.Substring(i, 1);
                g.DrawString(chr, ftray.Font, b, new PointF(x + ftray.HorizOfs, y), sf);
                x += ftray.DisplayWidth;
            }
        }

        /// <summary>
        /// Sets the application's DPI scaling factor.
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
        /// <para>Gets the size of a Hanzi's display rectangle, if using the font with the provided parameters.</para>
        /// <para>Expensive (instantiates and destroys font).</para>
        /// </summary>
        public static SizeF GetCharSize(float size)
        {
            FontTray ftray = null;
            try
            {
                // Font family and script do not matter
                // Whole point of this class is to make sure the display rectangles
                //   for any family and script are standardized.
                ftray = createFont(IdeoFamily.ArphicKai, IdeoScript.Simp, size, FontStyle.Regular);
                return new SizeF(ftray.DisplayWidth, ftray.DisplayHeight);
            }
            finally
            {
                ftray.Font.Dispose();
            }
        }

        private static FontTray createFont(IdeoFamily fam, IdeoScript script, float size, FontStyle style)
        {
            FontTray ftray = null;
            float height = size * 4F / 3F;
            height *= scale;
            foreach (FontFamily ff in fonts.Families)
            {
                if (fam == IdeoFamily.ArphicKai && script == IdeoScript.Trad && ff.Name == "AR PL UKai TW")
                    ftray = new FontTray(
                        new Font(ff, size, style), -0.08798828125F * height, -0.05F,
                        height * 0.9F, height);
                else if (fam == IdeoFamily.ArphicKai && script == IdeoScript.Simp && ff.Name == "䡡湄楮札䍓ⵆ潮瑳")
                    ftray = new FontTray(
                        new Font(ff, size, style), 0, -0.05F,
                        height * 0.9F, height);
                else if (fam == IdeoFamily.Noto && script == IdeoScript.Trad && ff.Name == "Noto Sans T Chinese Light")
                    ftray = new FontTray(
                        new Font(ff, size * 0.85F, style), height * 0.075F, height * 0.025F,
                        height * 0.9F, height);
                else if (fam == IdeoFamily.Noto && script == IdeoScript.Simp && ff.Name == "Noto Sans S Chinese Light")
                    ftray = new FontTray(
                        new Font(ff, size * 0.85F, style), height * 0.075F, height * 0.025F,
                        height * 0.9F, height);
                if (ftray != null) break;
            }
            if (ftray == null) throw new Exception("Requested font not available.");
            return ftray;
        }

        /// <summary>
        /// Gets the font with the desired parameters for measuring or drawing text.
        /// </summary>
        private static FontTray getFont(IdeoFamily fam, IdeoScript script, float size, FontStyle style)
        {
            // If font's already in cache, great: use that.
            FontCacheKey fck = new FontCacheKey(fam, script, size, style);
            if (fontCache.ContainsKey(fck)) return fontCache[fck];
   
            // First time someone request a font like this. Instantiate and cache it.
            FontTray ftray = createFont(fam, script, size, style);
            fontCache[fck] = ftray;
            return ftray;
        }
    }
}
