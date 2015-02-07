using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.IO;

namespace ZD.FontTest
{
    class FontCoverage
    {
        public static void CheckCoverage(string fontFileName, string outFileName)
        {
            var families = Fonts.GetFontFamilies(fontFileName);
            using (StreamWriter sw = new StreamWriter(outFileName))
            {
                foreach (FontFamily family in families)
                {
                    var typefaces = family.GetTypefaces();
                    foreach (Typeface typeface in typefaces)
                    {
                        GlyphTypeface glyph;
                        typeface.TryGetGlyphTypeface(out glyph);
                        IDictionary<int, ushort> characterMap = glyph.CharacterToGlyphMap;
                        int i = 1;
                        foreach (KeyValuePair<int, ushort> kvp in characterMap)
                        {
                            string line = "{0:D6}\t\\u{1:X4}\t{2}";
                            line = string.Format(line, i, kvp.Key, (char)kvp.Key);
                            sw.WriteLine(line);
                            ++i;
                        }
                        break;
                    }
                }
                sw.Flush();
            }
        }
    }
}
