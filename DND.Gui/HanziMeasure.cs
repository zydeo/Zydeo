using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace DND.Gui
{
    /// <summary>
    /// Measure alleged and real display rectangle of hanzi characters with various fonts.
    /// </summary>
    internal class HanziMeasure
    {
        /// <summary>
        /// Identifies a font by face and size (we never do bold or italic on hanzi, so it's enough)
        /// </summary>
        private class FontKey
        {
            /// <summary>
            /// The font face.
            /// </summary>
            public readonly string Face;
            /// <summary>
            /// The font size.
            /// </summary>
            public readonly float Size;
            /// <summary>
            /// Ctor: init immutable instance.
            /// </summary>
            public FontKey(string face, float size)
            {
                Face = face;
                Size = size;
            }
            /// <summary>
            /// Gets object's hash code.
            /// </summary>
            public override int GetHashCode()
            {
                float sx = Size * 100.0F;
                return Face.GetHashCode() + (int)sx;
            }
            /// <summary>
            /// Checks if object equals another one.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj)) return true;
                if (GetHashCode() != obj.GetHashCode()) return false;
                if (!(obj is FontKey)) return false;
                FontKey other = obj as FontKey;
                return Face == other.Face && Size == other.Size;
            }
        }

        /// <summary>
        /// One character's alleged size, and actual rectangle inside.
        /// </summary>
        public class SizeInfo
        {
            /// <summary>
            /// One character's alleged size, as returned by MeasureString.
            /// </summary>
            public readonly SizeF AllegedSize;
            /// <summary>
            /// The actual rectangle occupied by characters within their full alleged rectangle.
            /// </summary>
            public readonly RectangleF RealRect;
            /// <summary>
            /// Ctor: initialize values.
            /// </summary>
            public SizeInfo(SizeF allegedSize, RectangleF realRect)
            {
                AllegedSize = allegedSize;
                RealRect = realRect;
            }
        }

        /// <summary>
        /// Cache of measured fonts.
        /// </summary>
        private readonly Dictionary<FontKey, SizeInfo> cache = new Dictionary<FontKey, SizeInfo>();

        /// <summary>
        /// Private ctor: singleton pattern.
        /// </summary>
        private HanziMeasure() { }

        /// <summary>
        /// Singleton instance.
        /// </summary>
        private static HanziMeasure instance;

        /// <summary>
        /// Gets the singleton instance (created on demand).
        /// </summary>
        public static HanziMeasure Instance
        {
            get
            {
                if (instance == null) instance = new HanziMeasure();
                return instance;
            }
        }

        /// <summary>
        /// Get a font's real hanzi drawig properties.
        /// </summary>
        public SizeInfo GetMeasures(string fontFace, float size)
        {
            FontKey fk = new FontKey(fontFace, size);
            // Try from cache
            // Need locking: multiple drawing threads may be calling us.
            lock (cache)
            {
                if (cache.ContainsKey(fk)) return cache[fk];
            }
            // Not cached: measure now
            SizeInfo si = measure(fk);
            lock (cache)
            {
                cache[fk] = si;
            }
            return si;
        }

        /// <summary>
        /// Measure a hanzi character's display size and its actual rectangle.
        /// </summary>
        private SizeInfo measure(FontKey fk)
        {
            // Our results
            SizeF size;
            int left = 0;
            int top = 0;
            int right = 0;
            int bottom = 0;
            // This character is very tall and pretty wide. Oh beautiful life.
            string testStr = "蠹";
            StringFormat sf = StringFormat.GenericTypographic;
            using (Font fnt = FontPool.GetFont(fk.Face, fk.Size, FontStyle.Regular))
            {
                // To measure alleged size, just use a 1px bitmap
                using (Bitmap bmp = new Bitmap(1, 1))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    size = g.MeasureString(testStr, fnt, 65535, sf);
                }
                // To measure real rectangle, create a big enough bitmap and actually draw
                using (Bitmap bmp = new Bitmap(((int)size.Width), ((int)size.Height)))
                using (Graphics g = Graphics.FromImage(bmp))
                using (Brush bgBrush = new SolidBrush(Color.White))
                using (Brush txtBrush = new SolidBrush(Color.Black))
                {
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.FillRectangle(bgBrush, new Rectangle(0, 0, bmp.Width, bmp.Height));
                    g.DrawString(testStr, fnt, txtBrush, new PointF(0, 0));
                    // Now the brutality. Find topmost, leftmost, rightmost, bottommost non-white pixel.
                    // Yes. That's exactly what we do.
                    // Top
                    for (int y = 0; y != bmp.Height; ++y)
                    {
                        bool found = false;
                        for (int x = 0; x != bmp.Width; ++x)
                        {
                            if (bmp.GetPixel(x, y) != Color.FromArgb(255, 255, 255, 255))
                            { found = true; top = y; break; }
                        }
                        if (found) break;
                    }
                    // Left
                    for (int x = 0; x != bmp.Width; ++x)
                    {
                        bool found = false;
                        for (int y = 0; y != bmp.Height; ++y)
                        {
                            if (bmp.GetPixel(x, y) != Color.FromArgb(255, 255, 255, 255))
                            { found = true; left = x; break; }
                        }
                        if (found) break;
                    }
                    // Bottom
                    for (int y = bmp.Height - 1; y >= 0; --y)
                    {
                        bool found = false;
                        for (int x = 0; x != bmp.Width; ++x)
                        {
                            if (bmp.GetPixel(x, y) != Color.FromArgb(255, 255, 255, 255))
                            { found = true; bottom = y; break; }
                        }
                        if (found) break;
                    }
                    // Right
                    for (int x = bmp.Width - 1; x >= 0; --x)
                    {
                        bool found = false;
                        for (int y = 0; y != bmp.Height; ++y)
                        {
                            if (bmp.GetPixel(x, y) != Color.FromArgb(255, 255, 255, 255))
                            { found = true; right = x; break; }
                        }
                        if (found) break;
                    }
                }
            }
            // Got alleged size and real boundaries
            return new SizeInfo(size, new RectangleF(left, top, right - left + 1, bottom - top + 1));
        }
    }
}
