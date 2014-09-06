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
        /// Lock object around animation's status parameters.
        /// </summary>
        private readonly object animLO = new object();
        /// <summary>
        /// Proportion to original to shrink to (relative minimum size of image).
        /// </summary>
        private const float smallProp = 0.8F;
        /// <summary>
        /// Current animation state:
        /// 0: not animating (stable)
        /// -1: shrinking
        /// -2: shrinking, should re-grow when done; FAST
        /// 1: growing
        /// 2: growing, FAST
        /// </summary>
        private int sizeAnimState = 0;
        /// <summary>
        /// Current value in animation; between 0 and 1.
        /// 0: fully shrunken state
        /// 1: fully grown state
        /// </summary>
        private float sizeAnimVal = 0;

        /// <summary>
        /// Gets the current transitional values for painting.
        /// </summary>
        /// <param name="prop">Image proportion for sizing</param>
        private void getAnimValues(out float prop)
        {
            lock (animLO)
            {
                if (sizeAnimVal == 1) prop = 1;
                else if (sizeAnimVal == 0) prop = smallProp;
                else
                {
                    // We don't want linear ease-in and ease-out
                    // Cubic's chique.
                    float ts = sizeAnimVal * sizeAnimVal;
	                float tc = ts * sizeAnimVal;
	                float cubeVal = -2.0F * tc + 3.0F * ts;
                    prop = smallProp + 0.2F * cubeVal;
                }
            }
        }

        /// <summary>
        /// <para>Nudges size animation to next state.</para>
        /// <para>Must be called from within critical section through lock object.</para>
        /// </summary>
        /// <returns>True if timer is still needed; false otherwise.</returns>
        private bool doTimerSize()
        {
            lock (animLO)
            {
                // Shrinking
                if (sizeAnimState < 0)
                {
                    sizeAnimVal -= 0.1F;
                    if (sizeAnimState == -2) sizeAnimVal -= 0.1F;
                    // Reached minimum size?
                    if (sizeAnimVal < 0)
                    {
                        sizeAnimVal = 0;
                        // Need to re-grow?
                        if (sizeAnimState == -2) sizeAnimState = 2;
                        else sizeAnimState = 0;
                    }
                }
                // Growing
                else if (sizeAnimState > 0)
                {
                    sizeAnimVal += 0.1F;
                    if (sizeAnimState == 2) sizeAnimVal += 0.1F;
                    // Reached maximum size?
                    if (sizeAnimVal > 1)
                    {
                        sizeAnimVal = 1;
                        sizeAnimState = 0;
                    }
                }
            }
            // Keep up the timer
            return sizeAnimState != 0;
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
        /// Start grow animation (on mouse enter).
        /// </summary>
        private void doAnimGrow()
        {
            lock (animLO)
            {
                // Currently growing: we're good.
                if (sizeAnimState == 1) return;
                // Currently shrinking: to to grow if it's slow
                if (sizeAnimState == -1) sizeAnimState = 1;
                // Currently shrinking with regrow: good
                else if (sizeAnimState == -2) { /* NOP */ }
                // Not animating: grow
                else sizeAnimState = 1;
            }
            SubscribeToTimer();
        }

        /// <summary>
        /// Start shrink animation (on mouse leave, or in click).
        /// </summary>
        /// <param name="regrow">If true, icon must re-grow (click completed with mouse up).</param>
        private void doAnimShrink(bool regrow)
        {
            lock (animLO)
            {
                // Currently shrinking
                if (sizeAnimState < 0)
                {
                    // Make sure we re-grow if needed
                    // And we don't if it's not
                    if (regrow) sizeAnimState = -2;
                    else sizeAnimState = -1;
                }
                // Currently growing: reverse course
                else if (sizeAnimState > 0) sizeAnimState = regrow ? -2 : -1;
                // Not animating: shrink
                else sizeAnimState = regrow ? -2 : -1;
            }
            SubscribeToTimer();
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
            doAnimShrink(false);
            return true;
        }

        /// <summary>
        /// Handles mouse up (triggers required animations). Emits click.
        /// </summary>
        public override bool DoMouseUp(Point p, MouseButtons button)
        {
            if (!Visible) return false;
            doAnimShrink(true);
            FireClick();
            return true;
        }

        /// <summary>
        /// Handles mouse enter (triggers required animations).
        /// </summary>
        public override void DoMouseEnter()
        {
            if (!Visible) return;
            doAnimGrow();
        }

        /// <summary>
        /// Handles mouse leave (triggers animation to return to normal).
        /// </summary>
        public override void DoMouseLeave()
        {
            if (!Visible) return;
            doAnimShrink(false);
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
