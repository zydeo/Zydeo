using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.IO;

namespace ZD.FontTest
{
    partial class FontTestCtrl
    {
        public void SetFont(string fontFileName, float ptSize)
        {
            string fnFull = Path.GetFullPath(fontFileName);
            var families = Fonts.GetFontFamilies(fnFull);
            string familyName = "stupid compiler";
            foreach (System.Windows.Media.FontFamily family in families)
            {
                foreach (string fn in family.FamilyNames.Values) familyName = fn;
                var typefaces = family.GetTypefaces();
                foreach (Typeface typeface in typefaces)
                {
                    GlyphTypeface glyph;
                    typeface.TryGetGlyphTypeface(out glyph);
                    fm = new FontMetrics(typeface, glyph);
                    break;
                }
            }
            fonts.AddFontFile(fnFull);
            foreach (System.Drawing.FontFamily ff in fonts.Families)
            {
                if (ff.Name == familyName || ff.Name == familyName + " Regular")
                {
                    float ptSizeAdj = ptSize / ((float)fm.Height);
                    ptSizeAdj = ptSize;
                    fnt = new System.Drawing.Font(ff, ptSizeAdj, System.Drawing.FontStyle.Regular);
                    break;
                }
            }
            Invalidate();
        }
    }
}
