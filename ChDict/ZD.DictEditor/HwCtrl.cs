using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace ZD.DictEditor
{
    public partial class HwCtrl : UserControl
    {
        private static Bitmap bmpStatDone;
        private static Bitmap bmpStatDropped;
        private static Bitmap bmpStatEdited;
        private static Bitmap bmpStatMarked;
        private static Color clrSelected = Color.FromArgb(0xc8, 0xeb, 0xff);
        private static Color clrNotStarted = SystemColors.Window;
        private static Color clrEdited = Color.FromArgb(0xf9, 0xe9, 0xe6);
        private static Color clrDone = Color.FromArgb(0xe7, 0xf8, 0xdc);
        private static Color clrDropped = Color.FromArgb(0xf0, 0xf0, 0xf0);
        private static Color clrMarked = Color.FromArgb(0xd7, 0xc8, 0xe1);

        private DictData.HwData data;
        private bool selected = false;

        static HwCtrl()
        {
            Assembly a = Assembly.GetExecutingAssembly();
            bmpStatDone = new Bitmap(a.GetManifestResourceStream("ZD.DictEditor.Resources.stat-done.png"));
            bmpStatDropped = new Bitmap(a.GetManifestResourceStream("ZD.DictEditor.Resources.stat-dropped.png"));
            bmpStatEdited = new Bitmap(a.GetManifestResourceStream("ZD.DictEditor.Resources.stat-edited.png"));
            bmpStatMarked = new Bitmap(a.GetManifestResourceStream("ZD.DictEditor.Resources.stat-marked.png"));
        }

        public HwCtrl()
        {
            InitializeComponent();
        }

        private void setColorFromStatus()
        {
            if (selected) BackColor = clrSelected;
            else if (data.Status == DictData.HwStatus.Edited) BackColor = clrEdited;
            else if (data.Status == DictData.HwStatus.Done) BackColor = clrDone;
            else if (data.Status == DictData.HwStatus.Dropped) BackColor = clrDropped;
            else if (data.Status == DictData.HwStatus.Marked) BackColor = clrMarked;
            else BackColor = clrNotStarted;
        }

        public DictData.HwData Data
        {
            set
            {
                data = value;
                lblHeadword.Text = value.Simp;
                lblPinyin.Text = value.Pinyin;
                lblExtract.Text = value.Extract;
                if (value.Status == DictData.HwStatus.Done) pbStatus.BackgroundImage = bmpStatDone;
                else if (value.Status == DictData.HwStatus.Edited) pbStatus.BackgroundImage = bmpStatEdited;
                else if (value.Status == DictData.HwStatus.Marked) pbStatus.BackgroundImage = bmpStatMarked;
                else if (value.Status == DictData.HwStatus.Dropped) pbStatus.BackgroundImage = bmpStatDropped;
                else pbStatus.BackgroundImage = null;
                setColorFromStatus();
            }
        }

        public bool Selected
        {
            set
            {
                selected = value;
                setColorFromStatus();
            }
        }

        private void arrange()
        {
            lblHeadword.Width = Width - lblHeadword.Left - pbStatus.Left;
            lblPinyin.Width = Width - lblPinyin.Left - pbStatus.Left;
            lblExtract.Width = Width - lblExtract.Left - pbStatus.Left;
            pnlBottomSep.Location = new Point(pbStatus.Left, Height - 1);
            pnlBottomSep.Size = new Size(Width - 2 * pnlBottomSep.Left, 1);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            arrange();
        }
    }
}
