using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ZD.Gui.Zen
{
    /// <summary>
    /// Values to define where around a control's rectangle the tooltip should appear.
    /// </summary>
    public enum TooltipLocation
    {
        /// <summary>
        /// To the left of the control.
        /// </summary>
        West,
        /// <summary>
        /// To the top of the control.
        /// </summary>
        North,
        /// <summary>
        /// To the right of the control.
        /// </summary>
        East,
    }

    /// <summary>
    /// To be implemented by user-defined Zen controls to show tooltips.
    /// </summary>
    public interface IZenTooltip
    {
        /// <summary>
        /// <para>Gets the location of the tooltip needle's tip.</para>
        /// <para>If tooltip is north of control: X from control's left.</para>
        /// <para>If tooltip is east/west of control: Y from control's top.</para>
        /// </summary>
        int NeedlePos { get; }

        /// <summary>
        /// Gets the needle's "height", i.e., how far the bubble's edge will be from the needle's tip.
        /// </summary>
        int NeedleHeight { get; }

        /// <summary>
        /// Gets the bubble's location relative to the control's rectangle.
        /// </summary>
        TooltipLocation TooltipLocation { get; }

        /// <summary>
        /// <para>Gets the edge of the tooltip bubble that the control wants fixed.</para>
        /// <para>Bubble's width is always dynamic to fit text and to fit inside window.</para>
        /// <para>If equals int.MinValue, bubble is aligned so needle is in the middle.</para>
        /// <para>If tooltip is to East or West, this is top of the bubble's edge, with 0 = control's top.</para>
        /// <para>If tooltip is to North, positive value is left of bubble's edge, with 0 = control's left.</para>
        /// <para>If tooltip is to North, absolute of negative value is right of bubble's edge, with 0 = control's left.</para>
        /// </summary>
        int TopOrSide { get; }

        /// <summary>
        /// If true, visible tooltip is hidden if control is clicked.
        /// </summary>
        bool HideOnClick { get; }

        /// <summary>
        /// Gets the tooltip text to be shown.
        /// </summary>
        string Text { get; }
    }
}
