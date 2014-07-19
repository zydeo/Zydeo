using System;
using System.Windows.Forms;

namespace DND.Gui.Zen
{
    public class ZenTab
    {
        internal delegate void TabHeaderChangedDelegate();
        internal TabHeaderChangedDelegate TabHeaderChanged;

        private readonly ZenControl ctrl;
        private string header;

        public ZenTab(ZenControl ctrl, string header)
        {
            if (ctrl == null) throw new ArgumentNullException("ctrl");
            if (header == null) throw new ArgumentNullException("header");
            this.ctrl = ctrl;
            this.header = header;
        }

        public ZenControl Ctrl
        {
            get { return ctrl; }
        }

        public string Header
        {
            get { return header; }
            set
            {
                if (value == null) throw new ArgumentNullException("Header");
                header = value;
                if (TabHeaderChanged != null) TabHeaderChanged();
            }
        }
    }
}
