using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

using ZD.Common;

namespace ZD.Gui
{
    public partial class SettingsControlWin : UserControl
    {
        public SettingsControlWin(ITextProvider tprov, ICedictEngineFactory dictFact)
        {
            InitializeComponent();

            setTexts(tprov, dictFact);
            arrangeHeader();

            chkUpdates.Checked = AppSettings.NotifyOfUpdates;
            chkUpdates.CheckedChanged += onUpdatesCheckedChanged;
            lblWebVal.Click += onLinkLabelClick;
            lblSourceCodeVal.Click += onLinkLabelClick;
        }

        private void setTexts(ITextProvider tprov, ICedictEngineFactory dictFact)
        {
            // Localized labels
            lblAbout.Text =tprov.GetString("ZydeoAbout");
            lblHeader1.Text = tprov.GetString("ZydeoHeader1");
            lblHeader2.Text = tprov.GetString("ZydeoHeader2");
            lblSourceCode.Text = tprov.GetString("ZydeoSourceCode");
            lblLicense.Text = tprov.GetString("ZydeoLicense");
            lblCopyrightVal.Text = tprov.GetString("ZydeoCopyrightVal");
            lblCopyright.Text = tprov.GetString("ZydeoCopyright");
            lblCharRecogVal.Text = tprov.GetString("ZydeoCharRecogVal");
            lblCharRecog.Text = tprov.GetString("ZydeoCharRecog");
            lblDictionaryVal.Text = tprov.GetString("ZydeoDictionaryVal");
            lblDictionary.Text = tprov.GetString("ZydeoDictionary");
            lblVersion.Text = tprov.GetString("ZydeoVersion");
            lblWeb.Text = tprov.GetString("ZydeoWeb");
            lblUpdates.Text = tprov.GetString("ZydeoUpdates");
            chkUpdates.Text = tprov.GetString("ZydeoNotifyUpdates");

            // Magic labels: string hard-wired
            lblWebVal.Text = Magic.WebUrl;
            lblSourceCodeVal.Text = Magic.GithubUrl;

            // Runtime data
            ICedictInfo info = dictFact.GetInfo(Magic.DictFileName);
            string infoStr = tprov.GetString("ZydeoDictionaryVal");
            // Formatting entry count with current locale's thousand separator
            string entryCountFmt = info.EntryCount.ToString("N0");
            // Formatting date in current locale's "short" form
            string dateFmt = info.Date.ToShortDateString();
            infoStr = string.Format(infoStr, dateFmt, entryCountFmt);
            lblDictionaryVal.Text = infoStr;
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            lblVersionVal.Text = ver.Major.ToString() + "." + ver.Minor.ToString();
        }

        private void arrangeHeader()
        {
            // First header line: at top, center aligned over separator column
            lblHeader1.Width = lblHeader1.PreferredWidth;
            lblHeader1.Top = 0;
            lblHeader1.Left = pnlTitle.Width - (int)tblInner.ColumnStyles[2].Width - (int)tblInner.ColumnStyles[1].Width / 2 - lblHeader1.Width / 2;
            lblHeader1.Anchor = AnchorStyles.Right;

            // Second header line: right below first, center aligned over separator column
            lblHeader2.Width = lblHeader2.PreferredWidth;
            lblHeader2.Top = lblHeader1.Bottom;
            lblHeader2.Left = pnlTitle.Width - (int)tblInner.ColumnStyles[2].Width - (int)tblInner.ColumnStyles[1].Width / 2 - lblHeader2.Width / 2;
            lblHeader2.Anchor = AnchorStyles.Right;
        }

        void onUpdatesCheckedChanged(object sender, EventArgs e)
        {
            AppSettings.NotifyOfUpdates = chkUpdates.Checked;
        }

        void onLinkLabelClick(object sender, EventArgs e)
        {
            string openUrl = null;
            if (sender == lblSourceCodeVal) openUrl = "http://" + Magic.GithubUrl;
            else if (sender == lblWebVal) openUrl = "http://" + Magic.WebUrl;
            if (openUrl == null) return;
            // Open URL in system's default browser - if we can.
            try
            {
                System.Diagnostics.Process.Start(openUrl);
            }
            catch
            {
                // Swallow it all. Worst case, we don't open link - so what.
                // Not worth a crash or messages.
            }
        }
    }
}
