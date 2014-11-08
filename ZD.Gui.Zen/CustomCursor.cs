using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;

namespace ZD.Gui.Zen
{
    /// <summary>
    /// Helper class to create custom cursors from bitmaps.
    /// </summary>
    public static class CustomCursor
    {
        private static Cursor hand;

        public static Cursor GetHand(float scale)
        {
            if (hand == null)
            {
                float f = 20F * scale;
                Size sz = new Size((int)f, (int)f);
                // Images for buttons under writing pad; will get owned by buttons, not that it matters.
                Assembly a = Assembly.GetExecutingAssembly();
                using (var imgHand = Image.FromStream(a.GetManifestResourceStream("ZD.Gui.Zen.Resources.hand-cursor.png")))
                using (Bitmap bmpHand = new Bitmap(imgHand, sz))
                {
                    hand = CreateCursor(bmpHand, sz.Width / 2, 0);
                }

            }
            return hand;
        }

        private struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        /// <summary>
        /// Creates a custom cursor from the provided bitmap.
        /// </summary>
        /// <param name="bmp">The cursor to draw.</param>
        /// <param name="xHotSpot">The cursor hot spot's X-coordinate within the bitmap.</param>
        /// <param name="yHotSpot">The cursor hot spot's X-coordinate within the bitmap.</param>
        /// <returns>The new cursor.</returns>
        public static Cursor CreateCursor(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            IntPtr ptr = bmp.GetHicon();
            IconInfo tmp = new IconInfo();
            GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            ptr = CreateIconIndirect(ref tmp);
            return new Cursor(ptr);
        }
    }
}
