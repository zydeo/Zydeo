using System;
using System.Windows.Forms;

namespace ZD.Gui.Zen
{
    /// <summary>
    /// Represents one tab showed in a <see cref="ZenTabbedForm"/>.
    /// </summary>
    public class ZenTab
    {
        internal delegate void TabHeaderChangedDelegate();
        internal TabHeaderChangedDelegate TabHeaderChanged;

        private readonly ZenControl ctrl;
        private string header;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="ctrl">The control to be shown, docked in the content area.</param>
        /// <param name="header">The text in the tab selector (header text).</param>
        public ZenTab(ZenControl ctrl, string header)
        {
            if (ctrl == null) throw new ArgumentNullException("ctrl");
            if (header == null) throw new ArgumentNullException("header");
            this.ctrl = ctrl;
            this.header = header;
        }

        /// <summary>
        /// Gets the control to be shown docked in the content area.
        /// </summary>
        public ZenControl Ctrl
        {
            get { return ctrl; }
        }

        /// <summary>
        /// Gets or sets the header text shown in the tab selector.
        /// </summary>
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
