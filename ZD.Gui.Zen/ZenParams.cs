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
        public static readonly float SysBtnPadding = 13;
        public static readonly float TooltipPadding = 8;

        public static readonly Size CloseBtnLogicalSize = new Size(40, 20);
        //public static readonly Color CloseBtnBgColorBase = Color.FromArgb(0x48, 0x2e, 0x74);
        public static readonly Color CloseBtnBgColorBase = Color.FromArgb(0x74, 0x35, 0x00);
        public static readonly Color CloseBtnBgColorHover = Color.FromArgb(0x95, 0x93, 0x99);
        //public static readonly Color CloseBtnBgColorBase = Color.FromArgb(199, 80, 80);
        //public static readonly Color CloseBtnBgColorHover = Color.FromArgb(224, 67, 67);
        public static readonly Color CloseBtnForeColor = Color.White;
        public static readonly Size OtherSysBtnLogicalSize = new Size(25, 20);
        //public static readonly Color OtherSysBtnBgColorBase = PaddingBackColor;
        //public static readonly Color OtherSysBtnBgColorHover = Color.FromArgb(54, 101, 179);
        public static readonly Color OtherSysBtnBgColorBase = Color.FromArgb(0x41, 0x6b, 0x00);
        public static readonly Color OtherSysBtnBgColorHover = Color.FromArgb(0x95, 0x93, 0x99);
        public static readonly Color OtherSysBtnForeColorBase = Color.Black;
        public static readonly Color OtherSysBtnForeColorHover = Color.White;

        public static readonly Color TabSysBgBase = Color.FromArgb(0x74, 0x35, 0x00);
        public static readonly Color TabSysBgActive = Color.White;
        public static readonly Color TabSysTxtBase = Color.FromArgb(0xf7, 0xf2, 0xed);
        public static readonly Color TabSysTxtActive = Color.Black;
        public static readonly Color TabOtherBgBase = Color.FromArgb(0x95, 0x93, 0x99);
        public static readonly Color TabOtherBgActive = Color.FromArgb(0xf7, 0xf2, 0xed);
        public static readonly Color TabOtherTxtBase = Color.White;
        public static readonly Color TabOtherTxtActive = Color.Black;
        public static readonly string DefaultSysFontFamily = "Segoe UI";
        public static readonly float HeaderTabFontSize = 12.0F;
        public static readonly float HeaderTabPadding = 12.0F;
        
        public static readonly Color BorderColor = Color.FromArgb(128, 128, 128);
        public static readonly Color PaddingBackColor = Color.FromArgb(0xf7, 0xf2, 0xed);
        public static readonly Color HeaderBackColorL = Color.FromArgb(0x51, 0x84, 0x03);
        public static readonly Color HeaderBackColorR = Color.FromArgb(0x41, 0x6b, 0x00);
        public static readonly float HeaderFontSize = 13.0F;
        public static readonly Color HeaderFontColor = Color.FromArgb(0xf7, 0xf2, 0xed);

        public static readonly Color WindowColor = Color.White;
        public static readonly float StandardFontSize = 12.0F;
        public static readonly Color StandardTextColor = Color.Black;
        public static readonly float TooltipFontSize = 10.0F;
        public static readonly Color TooltipTextColor = Color.White;
        public static readonly Color TooltipBackColor = Color.FromArgb(76, 76, 76);
        public static readonly byte TooltipMaxAlfa = 224;
        public static readonly Color DisabledTextColor = Color.FromArgb(128, 128, 128);
        public static readonly Color CtxtMenuHoverColor = Color.FromArgb(205, 205, 205);

        public static readonly Color BtnGradLightColor = Color.White;
        public static readonly Color BtnGradDarkColorBase = Color.FromArgb(0xcb, 0xce, 0xc6);
        public static readonly Color BtnGradDarkColorHover = Color.FromArgb(0xe2, 0xce, 0xff);
        public static readonly Color BtnGradDarkColorDisabled = Color.FromArgb(240, 240, 240);
        public static readonly Color BtnPressColor = Color.FromArgb(0xc5, 0x9f, 0x7f);

        public static readonly int ScrollBarWidth = 20;
        public static readonly Color ScrollColBg = Color.FromArgb(240, 240, 240);
        public static readonly Color ScrollColArrowBase = Color.FromArgb(96, 96, 96);
        public static readonly Color ScrollColBtnHover = Color.FromArgb(218, 218, 218);
        public static readonly Color ScrollColArrowHover = Color.FromArgb(0, 0, 0);
        public static readonly Color ScrollColThumbBase = Color.FromArgb(205, 205, 205);
        public static readonly Color ScrollColThumbSemiHover = Color.FromArgb(166, 166, 166);
        public static readonly Color ScrollColThumbHover = Color.FromArgb(136, 136, 136);
        public static readonly Color ScrollColThumbActive = Color.FromArgb(106, 106, 106);
        public static readonly Color ScrollColArrowActive = Color.FromArgb(96, 96, 96);
        public static readonly Color ScrollColBtnPress = Color.FromArgb(96, 96, 96);
        public static readonly Color ScrollColArrowPress = Color.FromArgb(255, 255, 255);
    }
}
