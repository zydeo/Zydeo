using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace DND.Gui.Zen
{
    public class ZenImageButton : ZenControl
    {
        private static readonly Color defaultBgColor = Color.White;

        private bool visible = true;
        private int padding = 0;
        private Image image = null;

        /// <summary>
        /// <para>Gets or sets whether the button is visible.</para>
        /// <para>Invisible buttons also don't animate, interact, or fire the click event.</para>
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        /// <summary>
        /// Gets or sets button's padding (from edge, not from border). Affects image scaling.
        /// </summary>
        public int Padding
        {
            get { return padding; }
            set { padding = value; }
        }

        /// <summary>
        /// Gets or sets image. Button takes ownership: will dispose image.
        /// </summary>
        public Image Image
        {
            get { return image; }
            set
            {
                if (image != null) image.Dispose();
                image = value;
            }
        }

            /// <summary>
        /// Ctor: takes owner.
        /// </summary>
        public ZenImageButton(ZenControlBase parent)
            : base(parent)
        {
        }

        public override void Dispose()
        {
            if (image != null) image.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Represents shrink/grow animation states.
        /// </summary>
        private enum SizeAnimState
        {
            /// <summary>
            /// No animation in progress.
            /// </summary>
            None,
            /// <summary>
            /// Image is being shrunk, to be grown again. Used on hover.
            /// </summary>
            ShrinkBeforeGrow,
            /// <summary>
            /// Image is being grown after shrinking. Used on hover.
            /// </summary>
            GrowAfterShrink,
            /// <summary>
            /// Image is being shrunk, to stay shrunk. Used on mouse down.
            /// </summary>
            Shrink,
            /// <summary>
            /// Image is being grown back. Used on mouse up.
            /// </summary>
            Grow,
        }

        /// <summary>
        /// Lock object around animation's status parameters.
        /// </summary>
        private readonly object animLO = new object();
        /// <summary>
        /// Proportion to original to shrink to (relative minimum size of image).
        /// </summary>
        private const double smallProp = 0.8;
        /// <summary>
        /// Current animation state.
        /// </summary>
        private SizeAnimState sizeAnimState = SizeAnimState.None;
        /// <summary>
        /// Current value in animation; between -1 and 1.
        /// </summary>
        private double sizeAnimVal = double.MinValue;
        /// <summary>
        /// If true, image must remain shrunk after finished shrinking. Used while mouse is down.
        /// </summary>
        private bool stableShrinkedState = false;

        /// <summary>
        /// Gets the current transitional values for painting.
        /// </summary>
        private void getAnimValues(out float prop)
        {
            lock (animLO)
            {
                // Image size
                if (sizeAnimVal == double.MinValue)
                    prop = stableShrinkedState ? (float)smallProp : 1.0F;
                else
                {
                    double val;
                    val = 1.0 - Math.Pow(sizeAnimVal, 2.0);
                    val = (Math.Cos(sizeAnimVal * Math.PI) + 1.0) / 2.0;
                    val *= (1.0 - smallProp);
                    prop = (float)(1.0 - val);
                }
            }
        }

        /// <summary>
        /// Starts a shrink-grow animation (on hover). Can transition from other animation in progress.
        /// </summary>
        private void doStartAnimShrinkGrow()
        {
            // Only bother if there is an image
            if (image == null) return;
            lock (animLO)
            {
                if (sizeAnimState == SizeAnimState.None)
                    sizeAnimVal = -1.0;
                else if (sizeAnimState == SizeAnimState.GrowAfterShrink || sizeAnimState == SizeAnimState.Grow)
                    sizeAnimVal = -sizeAnimVal;
                sizeAnimState = SizeAnimState.ShrinkBeforeGrow;
                if (stableShrinkedState)
                {
                    sizeAnimVal = 0;
                    sizeAnimState = SizeAnimState.GrowAfterShrink;
                }
                stableShrinkedState = false;
            }
            SubscribeToTimer();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Starts a shrink animation (on mouse down). Can transition from other animation in progress.
        /// </summary>
        private void doStartAnimShrink()
        {
            // Only bother if there is an image
            if (image == null) return;
            lock (animLO)
            {
                if (sizeAnimState == SizeAnimState.None)
                    sizeAnimVal = -1.0;
                else if (sizeAnimState == SizeAnimState.GrowAfterShrink || sizeAnimState == SizeAnimState.Grow)
                    sizeAnimVal = -sizeAnimVal;
                sizeAnimState = SizeAnimState.Shrink;
                stableShrinkedState = false;
            }
            SubscribeToTimer();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Starts a grow animation (on mouse up). Can transition from other animation in progress.
        /// </summary>
        private void doStartAnimGrow()
        {
            // Only bother if there is an image
            if (image == null) return;
            lock (animLO)
            {
                if (sizeAnimState == SizeAnimState.None)
                {
                    // Only grow back if image is permanently shrunk. Would be ugly to start growing
                    // from small when we're actually large.
                    if (!stableShrinkedState) return;
                    sizeAnimVal = 0;
                }
                else if (sizeAnimState == SizeAnimState.ShrinkBeforeGrow || sizeAnimState == SizeAnimState.Shrink)
                    sizeAnimVal = -sizeAnimVal;
                sizeAnimState = SizeAnimState.Grow;
                stableShrinkedState = false;
            }
            SubscribeToTimer();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// <para>Nudges size animation to next state.</para>
        /// <para>Must be called from within critical section through lock object.</para>
        /// </summary>
        /// <returns>True if timer is still needed; false otherwise.</returns>
        private bool doTimerSize()
        {
            // Done: say we don't need timer no more.
            if (sizeAnimState == SizeAnimState.None) return false;
            // Shrinking
            if (sizeAnimState == SizeAnimState.Shrink || sizeAnimState == SizeAnimState.ShrinkBeforeGrow)
            {
                sizeAnimVal += 0.2;
                if (sizeAnimVal >= 0)
                {
                    if (sizeAnimState == SizeAnimState.Shrink)
                    {
                        sizeAnimState = SizeAnimState.None;
                        sizeAnimVal = double.MinValue;
                        stableShrinkedState = true;
                    }
                    else sizeAnimState = SizeAnimState.GrowAfterShrink;
                }
            }
            // Growing
            else if (sizeAnimState == SizeAnimState.Grow || sizeAnimState == SizeAnimState.GrowAfterShrink)
            {
                sizeAnimVal += 0.2;
                if (sizeAnimVal >= 1)
                {
                    sizeAnimState = SizeAnimState.None;
                    sizeAnimVal = double.MinValue;
                }
            }
            // Keep up the timer
            return true;
        }

        /// <summary>
        /// Handles timer tick: nudges animation on to next state.
        /// </summary>
        public override void DoTimer()
        {
            lock (animLO)
            {
                bool timerNeeded = false;
                timerNeeded |= doTimerSize();
                if (!timerNeeded) UnsubscribeFromTimer();
            }
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Handles (or rather: consciously ignores) mouse click. We emit our click on mouse up.
        /// </summary>
        public override bool DoMouseClick(Point p, MouseButtons button)
        {
            // Make sure control's base does not send click event
            // We already did in MouseUp
            return visible;
        }

        /// <summary>
        /// Handles mouse down (triggers required animations).
        /// </summary>
        public override bool DoMouseDown(Point p, MouseButtons button)
        {
            if (!Visible) return false;
            doStartAnimShrink();
            return true;
        }

        /// <summary>
        /// Handles mouse up (triggers required animations). Emits click.
        /// </summary>
        public override bool DoMouseUp(Point p, MouseButtons button)
        {
            if (!Visible) return false;
            doStartAnimShrinkGrow();
            FireClick();
            return true;
        }

        /// <summary>
        /// Handles mouse enter (triggers required animations).
        /// </summary>
        public override void DoMouseEnter()
        {
            if (!Visible) return;
            doStartAnimShrinkGrow();
        }

        /// <summary>
        /// Handles mouse leave (triggers animation to return to normal).
        /// </summary>
        public override void DoMouseLeave()
        {
            if (!Visible) return;
            // This is only really needed if mouse leaves button while shrinking or permanently shrunk.
            doStartAnimGrow();
        }

        /// <summary>
        /// Paints control.
        /// </summary>
        public override void DoPaint(Graphics g)
        {
            // If not visible: no painting.
            if (!visible) return;

            // Proportion (shrink effect)
            // Current heat
            float prop;
            getAnimValues(out prop);

            // Background: solid white, for now
            using (Brush b = new SolidBrush(Color.White))
            {
                g.FillRectangle(b, 0, 0, Width, Height);
            }
            // Image
            if (image != null)
            {
                RectangleF imgRect;
                // Yes, there is shrinkging: calculate
                if (prop != 1.0)
                {
                    float sz = (float)(Height - 2 * (padding));
                    sz *= prop;
                    float szHalf = sz / 2.0F;
                    float imgMid = ((float)(Height + padding)) / 2.0F;
                    imgRect = new RectangleF(imgMid - szHalf, imgMid - szHalf, sz, sz);
                }
                // Full size: easier
                else
                {
                    imgRect = new RectangleF(padding, padding,
                       Height - 2 * (padding), Height - 2 * (padding));
                }
                // Draw image, shrunk to rectangle
                g.DrawImage(image, imgRect);
            }
        }
    }
}
