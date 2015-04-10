using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using ZD.Gui.Zen;

namespace ZD.Gui
{
    /// <summary>
    /// Overrides <see cref="ZD.Gui.Zen.SystemFontProvider"/> to control generic UI font, and extends with Zydeo-specific fonts.
    /// </summary>
    internal class ZydeoSystemFontProvider : SystemFontProvider
    {
        /// <summary>
        /// True if our fonts are based on Segoe UI because it exists; false otherwise.
        /// </summary>
        private readonly bool segoeExists;

        /// <summary>
        /// Font face of system font.
        /// </summary>
        private readonly string systemFontFace;

        /// <summary>
        /// Font face of font in buttons with Hanzi, and in search input control.
        /// </summary>
        private readonly string zhoButtonFontFace;

        /// <summary>
        /// Font face of font used for displaying lemmas.
        /// </summary>
        private readonly string lemmaFontFace;

        /// <summary>
        /// Ctor; checks for availability of Segoe UI font.
        /// </summary>
        public ZydeoSystemFontProvider()
        {
            bool segoeExists = false;
            Font fntSegoe = null;
            try
            {
                fntSegoe = new Font("Segoe UI", 12F, FontStyle.Regular);
                segoeExists = fntSegoe != null && fntSegoe.Name == "Segoe UI";
            }
            finally { if (fntSegoe != null) fntSegoe.Dispose(); }
            this.segoeExists = segoeExists;
            if (segoeExists)
            {
                systemFontFace = "Segoe UI";
                zhoButtonFontFace = "Segoe UI";
                lemmaFontFace = "Segoe UI";
            }
            else
            {
                systemFontFace = "Noto Sans";
                zhoButtonFontFace = "Noto Sans S Chinese Regular";
                lemmaFontFace = "Noto Sans";
            }
        }

        /// <summary>
        /// Gets whether system fonts are based on Segoe. If false, they're based on Noto.
        /// </summary>
        public bool SegoeExists
        {
            get { return segoeExists; }
}

        public override Font GetSystemFont(FontStyle style, float size)
        {
            return FontCollection.CreateFont(systemFontFace, size, style);
        }

        public Font GetZhoButtonFont(FontStyle style, float size)
        {
            return FontCollection.CreateFont(zhoButtonFontFace, size, style);
        }

        public Font GetLemmaFont(FontStyle style, float size)
        {
            return FontCollection.CreateFont(lemmaFontFace, size, style);
        }
    }
}
