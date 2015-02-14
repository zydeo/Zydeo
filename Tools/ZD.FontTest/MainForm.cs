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
            txtSz.TextChanged += onFontChanged;
            onFontChanged(null, null);
        }

        private void onFontChanged(object sender, EventArgs e)
        {
            string font = "ukaitw.TTF";
            if (rbSimp.Checked) font = "hdzb_75.TTF";
            else if (rbNoto.Checked) font = "NotoSansHans-Regular.otf";
            float sz;
            if (!float.TryParse(txtSz.Text, out sz)) sz = fontSz;
            fontSz = sz;
            canvas.SetFont(font, fontSz);

            pretty.SetFonts("Segoe UI", fontSz);
        }

        private void llSaveCoverage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            bool[] cvrSimp = new bool[65536];
            bool[] cvrTrad = new bool[65536];

            string currDir = Directory.GetCurrentDirectory();
            string fontFile = Path.Combine(currDir, @"hdzb_75.TTF");
            FontCoverage.CheckCoverage(fontFile, "fnt-simp-coverage.txt", cvrSimp);
            fontFile = Path.Combine(currDir, @"ukaitw.TTF");
            FontCoverage.CheckCoverage(fontFile, "fnt-trad-coverage.txt", cvrTrad);

            FontCoverage.SaveArphicCoverage(cvrSimp, cvrTrad, "arphic-coverage.bin");
        }
    }
}
