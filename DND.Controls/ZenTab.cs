using System;
using System.Windows.Forms;

namespace DND.Controls
{
    public class ZenTab
    {
        internal delegate void TabHeaderChangedDelegate();
        internal TabHeaderChangedDelegate TabHeaderChanged;

        private readonly Control ctrl;
        private string header;

        public ZenTab(Control ctrl, string header)
        {
            if (ctrl == null) throw new ArgumentNullException("ctrl");
            if (header == null) throw new ArgumentNullException("header");
            this.ctrl = ctrl;
            this.header = header;
        }

        public Control Ctrl
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
