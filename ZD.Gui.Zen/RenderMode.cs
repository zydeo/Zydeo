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
        /// Does not re-render canvas after paint.
        /// </summary>
        None = 1,
        /// <summary>
        /// Invalidate main window's canvas; queues up a regular Windows PAINT event.
        /// </summary>
        Invalidate = 2,
        /// <summary>
        /// Render canvas immediately.
        /// </summary>
        Update = 3,
    }
}
