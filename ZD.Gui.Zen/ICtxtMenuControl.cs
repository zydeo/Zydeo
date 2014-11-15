using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
