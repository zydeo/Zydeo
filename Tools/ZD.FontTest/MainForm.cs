using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace ZD.FontTest
{
    public partial class MainForm : Form
    {
        private float fontSz;

        public MainForm()
        {
            InitializeComponent();
            if (Process.GetCurrentProcess().ProcessName == "devenv") return;

            btnGo.Height = txtSz.Height;
            canvas.DpiScale = CurrentAutoScaleDimensions.Height / 13F;
            pretty.DpiScale = CurrentAutoScaleDimensions.Height / 13F;
            FontPool.Scale = CurrentAutoScaleDimensions.Height / 13F;

            rbArphic.CheckedChanged += onFontChanged;
            rbNoto.CheckedChanged += onFontChanged;
            rbSimp.CheckedChanged += onFontChanged;
            rbDFKai.CheckedChanged += onFontChanged;
            rbKaiTi.CheckedChanged += onFontChanged;
            txtSz.TextChanged += onFontChanged;
            onFontChanged(null, null);
        }

        private void onFontChanged(object sender, EventArgs e)
        {
            float sz;
            if (!float.TryParse(txtSz.Text, out sz)) sz = fontSz;
            fontSz = sz;
            pretty.SetFonts("Segoe UI", fontSz);

            if (rbDFKai.Checked || rbKaiTi.Checked)
            {
                string font = "DFKai-SB";
                if (rbKaiTi.Checked) font = "KaiTi";
                canvas.SetSysFont(font, fontSz);
            }
            else
            {
                string font = "ukaitw.TTF";
                if (rbSimp.Checked) font = "hdzb_75.TTF";
                else if (rbNoto.Checked) font = "NotoSansHans-Regular.otf";
                canvas.SetFont(font, fontSz);
            }
        }

        private void llSaveCoverage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            bool[] cvrSimp = new bool[65536];
            bool[] cvrTrad = new bool[65536];

            string currDir = @"D:\Development\Zydeo\_bin\Fonts";
            string fontFile = Path.Combine(currDir, @"ukaitw.ttf");
            FontCoverage.CheckCoverage(fontFile, "fnt-coverage-ukaitw.txt", cvrSimp);

            //string currDir = Directory.GetCurrentDirectory();
            //string fontFile = Path.Combine(currDir, @"hdzb_75.TTF");
            //FontCoverage.CheckCoverage(fontFile, "fnt-coverage-hdzb_75.txt", cvrSimp);
            //fontFile = Path.Combine(currDir, @"ukaitw.TTF");
            //FontCoverage.CheckCoverage(fontFile, "fnt-coverage-ukaitw.txt", cvrTrad);
            //FontCoverage.SaveArphicCoverage(cvrSimp, cvrTrad, "arphic-coverage.bin");

            //string currDir = @"C:\Windows\Fonts";
            //string fontFile = Path.Combine(currDir, @"kaiu.ttf");
            //FontCoverage.CheckCoverage(fontFile, "fnt-coverage-dfkai-sb.txt", cvrSimp);
            //fontFile = Path.Combine(currDir, @"simkai.ttf");
            //FontCoverage.CheckCoverage(fontFile, "fnt-coverage-kaiti.txt", cvrTrad);
            //FontCoverage.SaveArphicCoverage(cvrSimp, cvrTrad, "winfonts-coverage.bin");

            //string currDir = Directory.GetCurrentDirectory();
            //string fontFile = Path.Combine(currDir, @"NotoSansHans-Regular.otf");
            //FontCoverage.CheckCoverage(fontFile, "fnt-coverage-Noto.txt", cvrSimp);

            // System fonts
            //FontCoverage.CheckCoverage(@"C:\Windows\Fonts\simsun.ttc", "fnt-coverage-SimSun.txt", cvrSimp);
            //FontCoverage.CheckCoverage(@"C:\Windows\Fonts\msjh.ttc", "fnt-coverage-MSJhengHei.txt", cvrSimp);
            //FontCoverage.CheckCoverage(@"C:\Windows\Fonts\msyh.ttc", "fnt-coverage-MSYaHei.txt", cvrSimp);
            //FontCoverage.CheckCoverage(@"C:\Windows\Fonts\simhei.ttf", "fnt-coverage-SimHei.txt", cvrSimp);
            //FontCoverage.CheckCoverage(@"C:\Windows\Fonts\mingliu.ttc", "fnt-coverage-MingLiu.txt", cvrSimp);
            //FontCoverage.CheckCoverage(@"C:\Windows\Fonts\kaiu.ttf", "fnt-coverage-DFKai-SB.txt", cvrSimp);
            //FontCoverage.CheckCoverage(@"C:\Windows\Fonts\simkai.ttf", "fnt-coverage-KaiTi.txt", cvrSimp);
            //FontCoverage.CheckCoverage(@"C:\Windows\Fonts\simfang.ttf", "fnt-coverage-FangSong.txt", cvrSimp);
        }
    }
}
