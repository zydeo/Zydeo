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
        public static void CheckCoverage(string fontFileName, string outFileName, bool[] cvr)
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
                            if (kvp.Key < 65536)
                                cvr[kvp.Key] = true;
                            ++i;
                        }
                        break;
                    }
                    break;
                }
                sw.Flush();
            }
        }

        private static void setArr(byte[] arr, int ix, byte val)
        {
            int arrIx = ix / 4;
            int ofsInByte = ix - arrIx * 4;

            val &= 3;
            val <<= (ofsInByte * 2);
            byte b = arr[arrIx];
            byte mask = 3;
            mask <<= (ofsInByte * 2);
            mask ^= 0xff;
            b &= mask;
            b |= val;
            arr[arrIx] = b;
        }

        private static byte getVal(byte[] arr, int ix)
        {
            int arrIx = ix / 4;
            int ofsInByte = ix - arrIx * 4;
            byte b = arr[arrIx];
            b >>= (ofsInByte * 2);
            b &= 3;
            return b;
        }

        public static void SaveArphicCoverage(bool[] cvrSimp, bool[] cvrTrad, string outFileName)
        {
            byte[] logical = new byte[65536];
            byte[] arr = new byte[65536 / 4];

            for (int i = 0; i != 65536; ++i)
            {
                byte val = 0;
                if (cvrSimp[i]) val |= 1;
                if (cvrTrad[i]) val |= 2;
                logical[i] = val;
                setArr(arr, i, val);
            }
            // Verify
            for (int i = 0; i != 65536; ++i)
            {
                if (logical[i] != getVal(arr, i))
                    throw new Exception("I messed up.");
            }
            // Write as binary data
            using (FileStream fs = new FileStream(outFileName, FileMode.Create, FileAccess.ReadWrite))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(arr);
            }
        }
    }
}
