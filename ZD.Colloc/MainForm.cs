using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

using ZD.CedictEngine;
using ZD.Common;

namespace ZD.Colloc
{
    public partial class MainForm : Form
    {
        private Colloc colloc = new Colloc();
        private HeadwordInfo hwi;
        private string htmlSkeleton;

        public MainForm()
        {
            InitializeComponent();
            Assembly a = Assembly.GetExecutingAssembly();
            string fileName = "ZD.Colloc.skeleton.html";
            using (Stream s = a.GetManifestResourceStream(fileName))
            using (StreamReader sr = new StreamReader(s))
            {
                htmlSkeleton = sr.ReadToEnd();
            }
            hwi = new HeadwordInfo("unihanzi.bin");
            rbChSqCorr.CheckedChanged += rbCheckedChanged;
            rbLogLike.CheckedChanged += rbCheckedChanged;
        }

        private void rbCheckedChanged(object sender, EventArgs e)
        {
            renderHtml();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            btnGo.Enabled = false;
            pbar.Visible = true;
            colloc.Query(txtQuery.Text, int.Parse(txtMinFreq.Text), int.Parse(txtMaxFreq.Text),
                onQueryDone, int.Parse(txtLeftWin.Text), int.Parse(txtRightWin.Text));
        }

        private void lnkLoadFreq_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkLoadFreq.Enabled = false;
            btnGo.Enabled = false;
            pbar.Visible = true;
            colloc.LoadFreqs(onFreqsLoaded);
        }

        private void onFreqsLoaded()
        {
            if (InvokeRequired) { Invoke(new Action(() => onFreqsLoaded())); return; }
            btnGo.Enabled = true;
            pbar.Visible = false;
        }

        private void onQueryDone()
        {
            if (InvokeRequired) { Invoke(new Action(() => onQueryDone())); return; }
            btnGo.Enabled = true;
            pbar.Visible = false;
            renderHtml();
        }

        private void renderHtml()
        {
            StringBuilder sb = new StringBuilder();
            if (rbLogLike.Checked)
                colloc.ResArr.Sort((x, y) => y.LL.CompareTo(x.LL));
            else
                colloc.ResArr.Sort((x, y) => y.ChSqCorr.CompareTo(x.ChSqCorr));

            for (int i = 0; i < colloc.ResArr.Count && i < 100; ++i)
            {
                Colloc.Result res = colloc.ResArr[i];
                sb.Append("<tr><td valign='top' class='score'>");
                sb.Append(getScore(res));
                sb.Append("</td><td valign='top' class='word'>");
                sb.Append(res.Word);
                sb.Append("<br/>");
                string py, trg;
                getDict(res.Word, out py, out trg);
                sb.Append(esc(py));
                sb.Append("</td><td valign='top' class='dict'>");
                sb.Append(trg);
                sb.Append("</td></tr>\r\n");
            }
            string html = htmlSkeleton;
            html = html.Replace("{results}", sb.ToString());
            wb.Navigate("about:blank");
            if (wb.Document != null) wb.Document.Write(string.Empty);
            wb.DocumentText = html;
        }

        private string getScore(Colloc.Result res)
        {
            int score;
            if (rbLogLike.Checked) score = (int)res.LL;
            else score = (int)res.ChSqCorr;
            string fmt = "{0:#,#}";
            return string.Format(fmt, score);
        }

        private void getDict(string word, out string py, out string trg)
        {
            py = trg = "";
            CedictEntry[] ced, hdd;
            hwi.GetEntries(word, out ced, out hdd);
            if (ced.Length == 0 && hdd.Length == 0)
            {
                trg = "&nbsp;";
                return;
            }
            List<PinyinSyllable> sylls = new List<PinyinSyllable>();
            if (ced.Length != 0) sylls.AddRange(ced[0].Pinyin);
            else sylls.AddRange(hdd[0].Pinyin);
            bool first = true;
            foreach (PinyinSyllable syll in sylls)
            {
                if (first) py += " ";
                else first = false;
                py += syll.GetDisplayString(true);
            }
            foreach (CedictEntry e in ced)
            {
                foreach (CedictSense s in e.Senses)
                    trg += "/" + esc(s.GetPlainText());
                trg += "/<br/>";
            }
            foreach (CedictEntry e in hdd)
            {
                foreach (CedictSense s in e.Senses)
                    trg += "/" + esc(s.GetPlainText());
                trg += "/<br/>";
            }
            if (trg == "") trg = "&nbsp;";
        }

        private static string esc(string str)
        {
            str = str.Replace("&", "&amp;");
            str = str.Replace("<", "&lt;");
            str = str.Replace(">", "&gt;");
            return str;
        }

    }
}
