using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ZD.Gui.Zen
{
    public static class ZenParams
    {
        public static readonly float HeaderHeight = 40;
        public static readonly float InnerPadding = 8;
        public static readonly float TooltipPadding = 8;
        public static readonly Color BorderColor = Color.FromArgb(128, 128, 128);
        public static readonly Color PaddingBackColor = Color.Honeydew;
        public static readonly Color HeaderBackColor = Color.YellowGreen;
        public static readonly string HeaderTabFontFamily = "Segoe UI";
        public static readonly float HeaderTabFontSize = 12.0F;
        public static readonly float HeaderTabPadding = 12.0F;
        public static readonly string HeaderFontFamily = "Segoe UI";
        public static readonly float HeaderFontSize = 13.0F;
        public static readonly Color CloseColorBase = Color.FromArgb(199, 80, 80);
        public static readonly Color CloseColorHover = Color.FromArgb(224, 67, 67);
        public static readonly Color WindowColor = Color.White;
        public static readonly string GenericFontFamily = "Segoe UI";
        public static readonly float StandardFontSize = 12.0F;
        public static readonly Color StandardTextColor = Color.Black;
        public static readonly float TooltipFontSize = 10.0F;
        public static readonly Color TooltipTextColor = Color.White;
        public static readonly Color TooltipBackColor = Color.FromArgb(76, 76, 76);
        public static readonly byte TooltipMaxAlfa = 224;
        public static readonly Color DisabledTextColor = Color.FromArgb(128, 128, 128);

        public static readonly Color BtnGradLightColor = Color.White;
        public static readonly Color BtnGradDarkColorBase = Color.FromArgb(175, 238, 238);
        public static readonly Color BtnGradDarkColorHover = Color.YellowGreen;
        public static readonly Color BtnGradDarkColorDisabled = Color.FromArgb(240, 240, 240);
        public static readonly Color BtnPressColor = Color.FromArgb(240, 128, 128);
    }
}
