using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ZD.Gui.Zen
{
    /// <summary>
    /// Provides fonts for displaying standrd UI texts. Consumer can override to specify a different font.
    /// </summary>
    public class SystemFontProvider
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        private static SystemFontProvider instance = new SystemFontProvider();

        /// <summary>
        /// Gets or sets the font provider singleton.
        /// </summary>
        public static SystemFontProvider Instance
        {
            get { return instance; }
            set { instance = value; }
        }

        /// <summary>
        /// Creates a font with the provided style and size. Ownership is transferred to caller (who must dispose object).
        /// </summary>
        public virtual Font GetSystemFont(FontStyle style, float size)
        {
            return new Font(ZenParams.DefaultSysFontFamily, size, style);
        }
    }
}
