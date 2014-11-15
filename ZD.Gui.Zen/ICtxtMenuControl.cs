using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace ZD.Gui.Zen
{
    /// <summary>
    /// Keyboard navigation events that controls in a context menu must handle.
    /// </summary>
    public enum CtxtMenuNavKey
    {
        Down,
        Up,
        Enter,
        Space,
    }

    /// <summary>
    /// Functionality that a control in a context menu must implement.
    /// </summary>
    public interface ICtxtMenuControl
    {
        /// <summary>
        /// Handles a navigation key event.
        /// </summary>
        void DoNavKey(CtxtMenuNavKey key);

        /// <summary>
        /// Handles mouse leaving area of context menu control.
        /// </summary>
        void DoMouseLeave();

        /// <summary>
        /// Handles mouse moving above context menu control.
        /// </summary>
        void DoMouseMove(Point pt);

        /// <summary>
        /// Handles mouse clicked over context menu control.
        /// </summary>
        void DoMouseClick(Point pt);

        /// <summary>
        /// Gets reference to WinForms user control to be shown as context menu.
        /// </summary>
        UserControl AsUserControl { get; }

        /// <summary>
        /// Calculates tooltip's size and assumes it (after it's been added to context menu form and scale is known).
        /// </summary>
        void AssumeSize();
    }
}
