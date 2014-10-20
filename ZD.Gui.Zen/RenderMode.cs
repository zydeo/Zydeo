using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Gui.Zen
{
    /// <summary>
    /// Determines canvas rendering behavior after callback to a control's pain handler.
    /// </summary>
    public enum RenderMode
    {
        /// <summary>
        /// Render canvas immediately.
        /// </summary>
        Update,
        /// <summary>
        /// Invalidate main window's canvas; queues up a regular Windows PAINT event.
        /// </summary>
        Invalidate,
        /// <summary>
        /// Does not re-render canvas after paint.
        /// </summary>
        None
    }
}
