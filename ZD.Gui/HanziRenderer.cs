using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Reflection;

using ZD.Common;

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
        /// DFKai-SB and KaiTi, the two Kai fonts installed with Windows (Vista and above).
        /// </summary>
        WinKai,
        /// <summary>
        /// Noto Sans Han T/S, Regular.
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

        /// <summary>
        /// Composite value-type key identifying cached instantiate fonts.
        /// </summary>
        private struct FontCacheKey
        {
            /// <summary>
            /// The font family.
            /// </summary>
            private readonly IdeoFamily family;
            /// <summary>
            /// The script.
            /// </summary>
            private readonly IdeoScript script;
            /// <summary>
            /// Size (100 times float size in points).
            /// </summary>
            private readonly int size;
            /// <summary>
            /// Font style.
            /// </summary>
            private readonly FontStyle style;

            /// <summary>
            /// Ctor: initialize immutable instance.
            /// </summary>
            public FontCacheKey(IdeoFamily family, IdeoScript script, float size, FontStyle style)
            {
                this.family = family;
                this.script = script;
                this.size = (int)(100F * size);
                this.style = style;
            }

            /// <summary>
            /// Hash code (for use in associative collection).
            /// </summary>
            public override int GetHashCode()
            {
                int hash = 17;
                hash = hash * 31 + (int)family;
                hash = hash * 31 + (int)script;
                hash = hash * 31 + size;
                hash = hash * 31 + (int)style;
                return hash;
            }

            /// <summary>
            /// Equality (untyped).
            /// </summary>
            public override bool Equals(object other)
            {
                return other is FontCacheKey ? Equals((FontCacheKey)other) : false;
            }

            /// <summary>
            /// Equality (typed).
            /// </summary>
            public bool Equals(FontCacheKey other)
            {
                return family == other.family &&
                    script == other.script &&
                    size == other.size &&
                    style == other.style;
            }
        }

        /// <summary>
        /// Returns coverage info from a binary compact array.
        /// </summary>
        private class CvrBinary : IFontCoverage
        {
            /// <summary>
            /// The compact binary array with info about the font's code point coverage.
            /// </summary>
            private readonly byte[] coverage;

            /// <summary>
            /// Ctor: take reference to compact binary array.
            /// </summary>
            /// <param name="coverage"></param>
            public CvrBinary(byte[] coverage)
            {
                if (coverage == null) throw new ArgumentNullException("coverage");
                if (coverage.Length != 65536 / 4) throw new ArgumentException("Coverage array must be 16k long.");
                this.coverage = coverage;
            }

            /// <summary>
            /// Returns coverage value for character from compact array.
            /// </summary>
            private byte getCvrVal(char c)
            {
                int ix = (int)c;
                int arrIx = ix / 4;
                int ofsInByte = ix - arrIx * 4;
                byte b = arphicCoverage[arrIx];
                b >>= (ofsInByte * 2);
                b &= 3;
                return b;
            }

            /// <summary>
            /// See <see cref="IFontCoverage.GetCoverage"/>.
            /// </summary>
            public FontCoverageFlags GetCoverage(char c)
            {
                FontCoverageFlags res = FontCoverageFlags.None;
                byte val = getCvrVal(c);
                if ((val & 1) == 1) res |= FontCoverageFlags.Simp;
                if ((val & 2) == 2) res |= FontCoverageFlags.Trad;
                return res;
            }
        }

        /// <summary>
        /// Specifies Arphic font coverage for a simplified character.
        /// </summary>
        private enum ArphicSimpCoverage
        {
            /// <summary>
            /// Simplified Arphic font has character.
            /// </summary>
            SimpCovers,
            /// <summary>
            /// Simplified font does not have character, but traditional does: can substitute.
            /// </summary>
            CanSubstitute,
            /// <summary>
            /// Not even traditional Arphic font covers character.
            /// </summary>
            None,
        }

        #region Font face and file names etc.
        private readonly static string myFileTradKai = "ukaitw.ttf";
        private readonly static string myFileSimpKai = "hdzb_75.ttf";
        private readonly static string myFileTradHei = "NotoSansHant-Light.otf";
        private readonly static string myFileSimpHei = "NotoSansHans-Light.otf";
        private readonly static string winFontNameTrad = "DFKai-SB";
        private readonly static string winFontNameSimp = "KaiTi";
        #endregion

        /// <summary>
        /// Compact array holding coverage info about simplified and traditional Arphic font.
        /// </summary>
        private static readonly byte[] arphicCoverage;

        /// <summary>
        /// Compact array holding coverage info about the Windows system Kai fonts.
        /// </summary>
        private static readonly byte[] winCoverage;

        /// <summary>
        /// Coverage info provider about the Arphic fonts.
        /// </summary>
        private static readonly CvrBinary cvrArphic;

        /// <summary>
        /// My private font collection - loaded from file.
        /// </summary>
        private static readonly PrivateFontCollection fonts = new PrivateFontCollection();

        /// <summary>
        /// Cached instantiated fonts.
        /// </summary>
        private static readonly Dictionary<FontCacheKey, FontTray> fontCache = new Dictionary<FontCacheKey, FontTray>();

        /// <summary>
        /// See <see cref="Scale"/>.
        /// </summary>
        private static float scale = 0;

        /// <summary>
        /// Returns true if the built-in Windows Kai fonts are available on the system.
        /// </summary>
        public static bool IsWinKaiAvailable()
        {
            bool simpThere = false;
            bool tradThere = false;
            using (Font fnt = new Font(winFontNameSimp, 10F))
            {
                simpThere = fnt.Name == winFontNameSimp;
            }
            using (Font fnt = new Font(winFontNameTrad, 10F))
            {
                tradThere = fnt.Name == winFontNameTrad;
            }
            return simpThere && tradThere;
        }

        /// <summary>
        /// Returns a coverage info provider for the specified font family.
        /// </summary>
        public static IFontCoverage GetFontCoverage(IdeoFamily fam)
        {
            if (fam == IdeoFamily.Noto) return new FontCoverageFull();
            else if (fam == IdeoFamily.ArphicKai) return cvrArphic;
            else if (fam == IdeoFamily.WinKai) return new CvrBinary(winCoverage);
            else throw new Exception("Forgotten family: " + fam.ToString());
        }

        /// <summary>
        /// Static ctor: loads fonts deployed with Zydeo.
        /// </summary>
        static HanziRenderer()
        {
            // Deserialize compact arrays about font coverage
            Assembly a = Assembly.GetExecutingAssembly();
            using (Stream s = a.GetManifestResourceStream("ZD.Gui.Resources.arphic-coverage.bin"))
            using (BinaryReader br = new BinaryReader(s))
            {
                arphicCoverage = br.ReadBytes(65536 / 4);
                cvrArphic = new CvrBinary(arphicCoverage);
            }
            using (Stream s = a.GetManifestResourceStream("ZD.Gui.Resources.winfonts-coverage.bin"))
            using (BinaryReader br = new BinaryReader(s))
            {
                winCoverage = br.ReadBytes(65536 / 4);
            }

            // Load deployed fonts into private collection
            fonts.AddFontFile(myFileTradKai);
            fonts.AddFontFile(myFileSimpKai);
            if (File.Exists(myFileSimpHei)) fonts.AddFontFile(myFileSimpHei);
            if (File.Exists(myFileTradHei)) fonts.AddFontFile(myFileTradHei);
        }

        /// <summary>
        /// Tells if simplified Arphic font covers a character, or can substitute, or no coverage at all.
        /// </summary>
        private static ArphicSimpCoverage getArphicCoverageSimp(char c)
        {
            FontCoverageFlags flags = cvrArphic.GetCoverage(c);
            if (flags == FontCoverageFlags.None) return ArphicSimpCoverage.None;
            if (flags == FontCoverageFlags.Simp) return ArphicSimpCoverage.SimpCovers;
            return ArphicSimpCoverage.CanSubstitute;
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

        /// <summary>
        /// Measures the display rectangle of a single character.
        /// </summary>
        public static SizeF MeasureChar(Graphics g, char c, float size)
        {
            // TO-DO: actually invoke g.MeasureString for non-Hanzi characters
            // I.e., regular and half-width alphabetical and digits

            FontTray ftray = getFont(IdeoFamily.ArphicKai, IdeoScript.Simp, size, FontStyle.Regular);
            return new SizeF(ftray.DisplayWidth, ftray.DisplayHeight);
        }

        /// <summary>
        /// Draws a Hanzi string in the desired font.
        /// </summary>
        public static void DrawString(Graphics g, string text, PointF loc, Brush b,
            IdeoFamily fam, IdeoScript script, float size, FontStyle style)
        {
            // Font tray for requested font
            FontTray ftray = getFont(fam, script, size, style);
            // Substitute font - only for Arphic simplified
            FontTray ftraySubst = null;
            if (fam == IdeoFamily.ArphicKai && script == IdeoScript.Simp)
                ftraySubst = getFont(fam, IdeoScript.Trad, size, style);
            // Where to draw - font-specific adjustment
            float x = loc.X;
            float y = loc.Y;

            // TO-DO: actually invoke g.MeasureString for non-Hanzi characters
            // I.e., regular and half-width alphabetical and digits

            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            for (int i = 0; i != text.Length; ++i)
            {
                string chr = text.Substring(i, 1);
                // Get coverage info about character
                ArphicSimpCoverage asp = ArphicSimpCoverage.SimpCovers;
                char c = chr[0];
                bool tofu = false;
                if (fam == IdeoFamily.ArphicKai) tofu =
                    (script == IdeoScript.Trad && !cvrArphic.GetCoverage(c).HasFlag(FontCoverageFlags.Trad)) ||
                    (script == IdeoScript.Simp && (asp = getArphicCoverageSimp(c)) == ArphicSimpCoverage.None);
                // Draw tofu is must be
                if (tofu)
                {
                    using (Pen p = new Pen(Color.Gray))
                    {
                        p.DashStyle = DashStyle.Dot;
                        g.SmoothingMode = SmoothingMode.None;
                        g.DrawRectangle(p, x, y, ftray.DisplayWidth, ftray.DisplayHeight);
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.DrawLine(p, x, y, x + ftray.DisplayWidth, y + ftray.DisplayHeight);
                        g.DrawLine(p, x + ftray.DisplayWidth, y, x, y + ftray.DisplayHeight);
                    }
                }
                // Draw with substitute string
                else if (asp == ArphicSimpCoverage.CanSubstitute)
                    g.DrawString(chr, ftraySubst.Font, b, new PointF(x + ftraySubst.HorizOfs, y + ftraySubst.VertOfs), sf);
                // Draw with requested string
                else g.DrawString(chr, ftray.Font, b, new PointF(x + ftray.HorizOfs, y + ftray.VertOfs), sf);
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

        /// <summary>
        /// Actually instantiates a specific font (based on family, script, size and style).
        /// </summary>
        private static FontTray createFont(IdeoFamily fam, IdeoScript script, float size, FontStyle style)
        {
            FontTray ftray = null;
            float height = size * 4F / 3F;
            height *= scale;

            // If a system font is requested, instantiate
            if (fam == IdeoFamily.WinKai)
            {
                if (script == IdeoScript.Simp)
                {
                    ftray = new FontTray(
                        new Font(winFontNameSimp, size, style), 0, -0.05F,
                        height * 0.9F, height);
                    if (ftray.Font.Name != winFontNameSimp) throw new Exception("Requested font not available.");
                }
                else
                {
                    ftray = new FontTray(
                         new Font(winFontNameTrad, size, style), 0, -0.05F,
                         height * 0.9F, height);
                    if (ftray.Font.Name != winFontNameTrad) throw new Exception("Requested font not available.");
                }
                return ftray;
            }

            // If a private font is requested, find font face in private collection and instantiate
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
        /// Gets the font with the desired parameters for measuring or drawing text. Works from cache.
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
