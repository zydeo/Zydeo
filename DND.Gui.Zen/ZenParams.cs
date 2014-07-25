using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DND.Gui.Zen
{
    public static class ZenParams
    {
        public static readonly float HeaderHeight = 40;
        public static readonly float InnerPadding = 8;
        public static readonly Color BorderColor = Color.FromArgb(13, 32, 45);
        public static readonly Color PaddingBackColor = Color.Honeydew;
        public static readonly Color HeaderBackColor = Color.YellowGreen;
        public static readonly string HeaderTabFontFamily = "Segoe UI";
        public static readonly float HeaderTabFontSize = 12.0F;
        public static readonly float HeaderTabPadding = 12.0F;
        public static readonly string HeaderFontFamily = "Segoe UI";
        public static readonly float HeaderFontSize = 13.0F;
        public static readonly Color CloseColorBase = Color.FromArgb(199, 80, 80);
        public static readonly Color CloseColorHover = Color.FromArgb(224, 67, 67);
        public static readonly string GenericFontFamily = "Segoe UI";

        public static readonly string ZhoFontFamily = "DFKai-SB";
        public static readonly float ZhoFontSize = 22.0F;
        public static readonly string PinyinFontFamily = "Tahoma";
        public static readonly float PinyinFontSize = 11.0F;
        public static readonly string LemmaFontFamily = "Segoe UI";
        public static readonly float LemmaFontSize = 10.0F;
    }
}
