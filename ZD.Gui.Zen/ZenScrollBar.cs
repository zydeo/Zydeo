using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ZD.Gui.Zen
{
    /// <summary>
    /// Vertical scrollbar with built-in inertia and bounceback capability.
    /// </summary>
    public class ZenScrollBar : ZenControl, IMessageFilter
    {
        public delegate void SubscribeScrollTimerDelegate();
        private readonly SubscribeScrollTimerDelegate subscribeScrollTimer;

        /// <summary>
        /// Padding inside, around scroll thumb. Also used for painting up/down triangles in buttons.
        /// </summary>
        private readonly int pad;

        /// <summary>
        /// See <see cref="Maximum"/>.
        /// </summary>
        private int maximum = 100;

        /// <summary>
        /// See <see cref="PageSize"/>.
        /// </summary>
        private int pageSize = 10;

        /// <summary>
        /// See <see cref="SmallChange"/>.
        /// </summary>
        private int smallSchange = 1;

        /// <summary>
        /// See <see cref="Position"/>.
        /// </summary>
        private int position = 0;

        /// <summary>
        /// See <see cref="SpringFactor"/>.
        /// </summary>
        private float springFactor = 1F;

        /// <summary>
        /// The actual size of the spring zone (page size times spring factor).
        /// </summary>
        private int springSize = 0;

        private float acceleration = 4F;
        private float deceleration = 2F;
        private float maxSpeed = 100F;

        /// <summary>
        /// Ctor: take parent.
        /// </summary>
        public ZenScrollBar(ZenControlBase parent, SubscribeScrollTimerDelegate subscribeScrollTimer)
            : base(parent)
        {
            this.subscribeScrollTimer = subscribeScrollTimer;
            Width = (int)(Scale * ((float)ZenParams.ScrollBarWidth));
            pad = Width / 4;
            acceleration = acceleration * Scale;
            deceleration = deceleration * Scale;
            maxSpeed = maxSpeed * Scale;
            doInitAnimVals();
            Application.AddMessageFilter(this);
        }

        /// <summary>
        /// <para>Gets or sets the scroll bar's maximum value. Cannot be smaller than 1.</para>
        /// </summary>
        public int Maximum
        {
            get { return maximum; }
            set
            {
                if (maximum < 1) throw new ArgumentException("Maximum must be 1 or greater.");
                if (pageSize > value) pageSize = value;
                doStopAnyScroll();
                maximum = value;
                if (position + pageSize > maximum) position = maximum - pageSize;
                MakeMePaint(false, RenderMode.Invalidate);
            }
        }

        /// <summary>
        /// <para>Gets or sets the scroll bar's page size.</para>
        /// </summary>
        public int PageSize
        {
            get { return pageSize; }
            set
            {
                if (value < 1) throw new ArgumentException("Page size must 1 or greater.");
                if (value > maximum) throw new ArgumentException("Page size cannot be greater than maximum value.");
                doStopAnyScroll();
                pageSize = value;
                if (position + pageSize > maximum) position = maximum - pageSize;
                springSize = (int)(springFactor * ((float)pageSize));
                MakeMePaint(false, RenderMode.Invalidate);
            }
        }

        /// <summary>
        /// Gets or sets the small change value, i.e., the amount the scroll moves on a single click at the up/down buttons.
        /// </summary>
        public int SmallChange
        {
            get { return smallSchange; }
            set
            {
                if (value < 1) throw new ArgumentException("Small change must 1 or greater.");
                if (value > pageSize) throw new ArgumentException("Small change cannot be greater than page size.");
                doStopAnyScroll();
                smallSchange = value;
                MakeMePaint(false, RenderMode.Invalidate);
            }
        }

        /// <summary>
        /// <para>Gets or sets the scroll bar's current position.</para>
        /// <para>Unless in spring zone, <see cref="Position"/> + <see cref="PageSize"/> &lt;= <see cref="Maximum"/> always.</para>
        /// <para>Cannot be set programatically in the spring zone - e.g., must always be positive.</para>
        /// </summary>
        public int Position
        {
            get { return position; }
            set
            {
                if (value < 0) throw new ArgumentException("Position must be positive.");
                if (value + pageSize > maximum) throw new ArgumentException("Position plus page size must not exceed maximum value.");
                if (position == value) return;
                doStopAnyScroll();
                position = value;
                MakeMePaint(false, RenderMode.Invalidate);
            }
        }

        /// <summary>
        /// <para>Gets or sets the spring factor. Must be between 0 and 0.8.</para>
        /// <para>A spring factor of 0 means scrolling stops at top and bottom abruptly.</para>
        /// <para>0.5 means inertia or tap-and-move can go half a page into negative or beyond maximum.</para>
        /// </summary>
        public float SpringFactor
        {
            get { return springFactor; }
            set
            {
                doStopAnyScroll();
                springFactor = value;
                springSize = (int)(springFactor * ((float)pageSize));
            }
        }

        #region Interaction / event handling

        /// <summary>
        /// <para>The mouse Y position, in my coordinates, at the time the thumb was pressed (mouse down).</para>
        /// <para>Value irrelevant if <see cref="pressedPart"/> is not the thumb.</para>
        /// </summary>
        private int thumbHoldStartMouseY = -1;

        /// <summary>
        /// Top of thumb when dragging started.
        /// </summary>
        private int thumbHoldStartThumbTop = -1;

        /// <summary>
        /// The <see cref="Position"/> when dragging started.
        /// </summary>
        private int thumbHoldStartPos = -1;

        /// <summary>
        /// Gets the thumb's rectangle (depend on position, page size, full control size).
        /// </summary>
        /// <param name="pos">Rectangle corresponding to position, and current size and page size.</param>
        private Rectangle getThumbRect(int pos)
        {
            // Adjust position so it's never out of bounds when we're in spring range
            int adjPos = position;
            if (adjPos < 0) adjPos = 0;
            else if (adjPos + pageSize > maximum) adjPos = maximum - pageSize;

            // Size is flexible based on proportion of page size to maximum
            // But never smaller than 2 times width
            int vSpace = Height - 2 * Width; // Playig field of the thumb
            int hThumb = (int)(((float)vSpace) * ((float)pageSize) / ((float)maximum));
            if (hThumb < 2 * Width) hThumb = 2 * Width;
            // Y in (Free playing field outside thumb area) ~~ Position in (Maximum - PageSize)
            int yThumb = (int)(((float)(vSpace - hThumb)) * ((float)adjPos) / ((float)(maximum - pageSize)));
            // Got it
            return new Rectangle(0, yThumb + Width, Width, hThumb);
        }

        /// <summary>
        /// Parts that can be "working", i.e., pressed and held.
        /// </summary>
        private enum WorkingPart
        {
            None,
            TopBtn,
            TopJump,
            Thumb,
            BottomJump,
            BottomBtn,
        }

        /// <summary>
        /// Component that is currently being pressed and held.
        /// </summary>
        private WorkingPart pressedPart = WorkingPart.None;

        public override bool DoMouseDown(Point p, MouseButtons button)
        {
            // No righties
            if (button != MouseButtons.Left) return true;

            // Figure out what's been pressed
            Rectangle thumbRect = getThumbRect(position);
            bool overTopBtn = new Rectangle(0, 0, Width, Width).Contains(p);
            bool overThumb = thumbRect.Contains(p);
            bool overBottomBtn = new Rectangle(0, Height - Width, Width, Width).Contains(p);
            if (overTopBtn) pressedPart = WorkingPart.TopBtn;
            else if (overThumb) pressedPart = WorkingPart.Thumb;
            else if (overBottomBtn) pressedPart = WorkingPart.BottomBtn;
            else if (p.Y < thumbRect.Top) pressedPart = WorkingPart.TopJump;
            else if (p.Y > thumbRect.Bottom) pressedPart = WorkingPart.BottomJump;
            else pressedPart = WorkingPart.None;

            // UI goodies, ease colors in and out
            doAnimateTo(true, overTopBtn, overThumb, overBottomBtn, pressedPart);

            // Thumb now gets dragged around
            if (pressedPart == WorkingPart.Thumb)
            {
                // Dragging the thumb means any scrolling in progress stops right now
                // But: record mandatory scroll position
                doStopAnyScroll(position);
                // Remember where we started dragging
                thumbHoldStartMouseY = p.Y;
                thumbHoldStartThumbTop = thumbRect.Top;
                thumbHoldStartPos = position;
                // Start capturing mouse. We want to keep tracking even if pointer leaves to left or right while
                // button is pressed.
                CaptureMouse();
            }
            // Jumpy-holdy parts (buttons and full page scroll)
            else
            {
                int fuelToBuy;
                int repeatBuy;
                if (pressedPart == WorkingPart.TopBtn) { fuelToBuy = -smallSchange; repeatBuy = fuelToBuy / 2; }
                else if (pressedPart == WorkingPart.BottomBtn) { fuelToBuy = smallSchange; repeatBuy = fuelToBuy / 2; }
                else if (pressedPart == WorkingPart.TopJump) { fuelToBuy = -pageSize / 4; repeatBuy = fuelToBuy / 6; }
                else { fuelToBuy = pageSize / 4; repeatBuy = fuelToBuy / 6; }
                // Buy the fuel
                doBuyScrollFuel(fuelToBuy);
                // Commission repeated purchases: "repeat fire" effect if button is kept pressed
                lock (animLOScroll)
                {
                    animRepeatCounter = 0;
                    animRepeatPurchase = ((float)repeatBuy);
                    if (pressedPart == WorkingPart.TopJump || pressedPart == WorkingPart.BottomJump)
                        animRepeatBarrierY = p.Y;
                }
            }
            
            // We're done here.
            return true;
        }

        public override bool DoMouseUp(Point p, MouseButtons button)
        {
            // No righties
            if (button != MouseButtons.Left) return true;

            // If we've been tracking thumb, stoppit.
            if (pressedPart == WorkingPart.Thumb) StopCapturingMouse();

            pressedPart = WorkingPart.None;
            lock (animLOScroll)
            {
                // No forced positioning
                animPosInDrag = int.MinValue;
                // Cancel any repeat purchase order
                animRepeatCounter = int.MinValue;
                animRepeatPurchase = float.MinValue;
            }
            doAnimateTo(true, false, false, false, pressedPart);
            return true;
        }

        public override bool DoMouseMove(Point p, MouseButtons button)
        {
            // If thumb is being dragged, handled that first
            if (pressedPart == WorkingPart.Thumb)
            {
                // How far we've moved vertically since user pressed and held thumb
                int diff = p.Y - thumbHoldStartMouseY;
                // How high is thumb
                int thumbH = getThumbRect(0).Height;
                // Where thumb's top goes - but only within bounds!
                // Note: "width" stands for height of up/down buttons
                int thumbY = thumbHoldStartThumbTop + diff;
                if (thumbY < Width) thumbY = Width;
                else if (thumbY + thumbH > Height - Width) thumbY = Height - Width - thumbH;
                // Calculate new position - thumb's top divides area whose height is my height - thumb height - two buttons
                float uiRange = (float)(Height - 2 * Width - thumbH);
                float uiProportion = ((float)(thumbY - Width)) / uiRange;
                float posRange = (float)(maximum - pageSize);
                float newPos = posRange * uiProportion;
                // Let scroll animation thread know about this
                lock (animLOScroll) { animPosInDrag = (int)newPos; }
            }

            // Do display niceties
            bool overTopBtn = false;
            bool overThumb = false;
            bool overBottomBtn = false;
            if (pressedPart == WorkingPart.None)
            {
                overTopBtn = new Rectangle(0, 0, Width, Width).Contains(p);
                overThumb = getThumbRect(position).Contains(p);
                overBottomBtn = new Rectangle(0, Height - Width, Width, Width).Contains(p);
            }
            doAnimateTo(true, overTopBtn, overThumb, overBottomBtn, pressedPart);
            return true;
        }

        public override void DoMouseLeave()
        {
            // When we leave, that unpresses whatever's currently pressed, except held thumb
            if (pressedPart != WorkingPart.Thumb)
            {
                // If we're holding on to thumb, pretend we don't leave
                pressedPart = WorkingPart.None;
                // Cancel any repeat purchase order (pressed up/down buttons)
                lock (animLOScroll)
                {
                    animRepeatCounter = int.MinValue;
                    animRepeatPurchase = float.MinValue;
                }
            }
            // Update our make-up.
            doAnimateTo(false, false, false, false, pressedPart);
        }

        #endregion

        #region Animation and paint

        /// <summary>
        /// Animated values.
        /// </summary>
        private struct AnimVals
        {
            public Color TopBtnBg;
            public Color TopBtnArrow;
            public Color BottomBtnBg;
            public Color BottomBtnArrow;
            public Color Thumb;
        }

        /// <summary>
        /// Set of all animation states.
        /// </summary>
        private class AnimState
        {
            /// <summary>
            /// Each component's starting color, in case component is being animated.
            /// </summary>
            public AnimVals From;
            /// <summary>
            /// Each component's target color, in case component's is being animated.
            /// </summary>
            public AnimVals To;
            /// <summary>
            /// -1: no animation in progress. 0 to 1: slow; 2..3: fast.
            /// </summary>
            public float StateTopBtn = -1;
            /// <summary>
            /// -1: no animation in progress. 0 to 1: slow; 2..3: fast.
            /// </summary>
            public float StateBottomBtn = -1;
            /// <summary>
            /// -1: no animation in progress. 0 to 1: slow; 2..3: fast.
            /// </summary>
            public float StateThumb = -1;
        }

        /// <summary>
        /// Lock object around my own anim states and values
        /// Ie stuff touched in <see cref="DoTimer"/>.
        /// </summary>
        private object animLO = new object();

        /// <summary>
        /// Current animated values.
        /// </summary>
        private AnimVals animVals;

        /// <summary>
        /// Current animation state.
        /// </summary>
        private readonly AnimState animState = new AnimState();

        /// <summary>
        /// Handles timer to nudge around scrollbar's own animations (everything except inertia scrolling).
        /// </summary>
        public override void DoTimer(out bool? needBackground, out RenderMode? renderMode)
        {
            needBackground = null;
            renderMode = null;
            bool needTimer = false;
            bool needPaint = false;
            lock (animLO)
            {
                // Top button
                if (animState.StateTopBtn != -1)
                {
                    needPaint = true;
                    if (animState.StateTopBtn > 1.99) animState.StateTopBtn += 0.2F;
                    else animState.StateTopBtn += 0.07F;
                    bool pastEnd = animState.StateTopBtn > 3 || (animState.StateTopBtn > 1 && animState.StateTopBtn < 1.99F);
                    if (pastEnd)
                    {
                        animState.StateTopBtn = -1;
                        animVals.TopBtnBg = animState.To.TopBtnBg;
                        animVals.TopBtnArrow = animState.To.TopBtnArrow;
                    }
                    else
                    {
                        float val = animState.StateTopBtn;
                        if (val > 2) val -= 2;
                        animVals.TopBtnBg = MixColors(animState.From.TopBtnBg, animState.To.TopBtnBg, val);
                        animVals.TopBtnArrow = MixColors(animState.From.TopBtnArrow, animState.To.TopBtnArrow, val);
                        needTimer = true;
                    }
                }
                // Bottom button
                if (animState.StateBottomBtn != -1)
                {
                    needPaint = true;
                    if (animState.StateBottomBtn > 1.99) animState.StateBottomBtn += 0.2F;
                    else animState.StateBottomBtn += 0.07F;
                    bool pastEnd = animState.StateBottomBtn > 3 || (animState.StateBottomBtn > 1 && animState.StateBottomBtn < 1.99F);
                    if (pastEnd)
                    {
                        animState.StateBottomBtn = -1;
                        animVals.BottomBtnBg = animState.To.BottomBtnBg;
                        animVals.BottomBtnArrow = animState.To.BottomBtnArrow;
                    }
                    else
                    {
                        float val = animState.StateBottomBtn;
                        if (val > 2) val -= 2;
                        animVals.BottomBtnBg = MixColors(animState.From.BottomBtnBg, animState.To.BottomBtnBg, val);
                        animVals.BottomBtnArrow = MixColors(animState.From.BottomBtnArrow, animState.To.BottomBtnArrow, val);
                        needTimer = true;
                    }
                }
                // Thumb
                if (animState.StateThumb != -1)
                {
                    needPaint = true;
                    if (animState.StateThumb > 1.99) animState.StateThumb += 0.2F;
                    else animState.StateThumb += 0.07F;
                    bool pastEnd = animState.StateThumb > 3 || (animState.StateThumb > 1 && animState.StateThumb < 1.99F);
                    if (pastEnd)
                    {
                        animState.StateThumb = -1;
                        animVals.Thumb = animState.To.Thumb;
                    }
                    else
                    {
                        float val = animState.StateThumb;
                        if (val > 2) val -= 2;
                        animVals.Thumb = MixColors(animState.From.Thumb, animState.To.Thumb, val);
                        needTimer = true;
                    }
                }
            }
            if (!needTimer) UnsubscribeFromTimer();
            if (needPaint)
            {
                needBackground = false;
                renderMode = RenderMode.Invalidate;
            }
        }

        /// <summary>
        /// Inits animated values at creation time.
        /// </summary>
        private void doInitAnimVals()
        {
            animVals.TopBtnBg = ZenParams.ScrollColBg;
            animVals.TopBtnArrow = ZenParams.ScrollColArrowBase;
            animVals.BottomBtnBg = ZenParams.ScrollColBg;
            animVals.BottomBtnArrow = ZenParams.ScrollColArrowBase;
            animVals.Thumb = ZenParams.ScrollColThumbBase;
        }

        /// <summary>
        /// Updates animations to target new states for our components.
        /// </summary>
        private void doAnimateTo(bool mouseIn, bool overTop, bool overThumb, bool overBottom,
            WorkingPart wp)
        {
            // Figure out each component's desired color based on arguments
            // First, just hover stuff
            Color topBtnBg = overTop ? ZenParams.ScrollColBtnHover : ZenParams.ScrollColBg;
            Color topBtnArrow = overTop ? ZenParams.ScrollColArrowHover : ZenParams.ScrollColArrowBase;
            Color bottomBtnBg = overBottom ? ZenParams.ScrollColBtnHover : ZenParams.ScrollColBg;
            Color bottomBtnArrow = overBottom ? ZenParams.ScrollColArrowHover : ZenParams.ScrollColArrowBase;
            Color thumb = mouseIn ? ZenParams.ScrollColThumbSemiHover : ZenParams.ScrollColThumbBase;
            if (overThumb) thumb = ZenParams.ScrollColThumbHover;
            // Then, pressed stuff
            if (wp == WorkingPart.TopBtn)
            {
                topBtnBg = ZenParams.ScrollColBtnPress;
                topBtnArrow = ZenParams.ScrollColArrowPress;
            }
            else if (wp == WorkingPart.BottomBtn)
            {
                bottomBtnBg = ZenParams.ScrollColBtnPress;
                bottomBtnArrow = ZenParams.ScrollColArrowPress;
            }
            else if (wp == WorkingPart.Thumb) thumb = ZenParams.ScrollColThumbActive;
            // Go from here to there, if we have to
            lock (animLO)
            {
                // If something is "working", i.e., pressed, we need timer for repeat action fires
                bool needTimer = wp != WorkingPart.None;
                // Must change top button colors, or not
                if (animVals.TopBtnBg == topBtnBg) animState.StateTopBtn = -1;
                else
                {
                    animState.StateTopBtn = (overTop || wp == WorkingPart.TopBtn) ? 2 : 0;
                    animState.From.TopBtnBg = animVals.TopBtnBg;
                    animState.From.TopBtnArrow = animVals.TopBtnArrow;
                    animState.To.TopBtnBg = topBtnBg;
                    animState.To.TopBtnArrow = topBtnArrow;
                    needTimer |= true;
                }
                // Must change bottom button colors, or not
                if (animVals.BottomBtnBg == bottomBtnBg) animState.StateBottomBtn = -1;
                else
                {
                    animState.StateBottomBtn = (overBottom || wp == WorkingPart.BottomBtn) ? 2 : 0;
                    animState.From.BottomBtnBg = animVals.BottomBtnBg;
                    animState.From.BottomBtnArrow = animVals.BottomBtnArrow;
                    animState.To.BottomBtnBg = bottomBtnBg;
                    animState.To.BottomBtnArrow = bottomBtnArrow;
                    needTimer |= true;
                }
                // Must change thumb color, or not
                if (animVals.Thumb == thumb) animState.StateThumb = -1;
                else
                {
                    animState.StateThumb = (overThumb || wp == WorkingPart.Thumb) ? 2 : 0;
                    animState.From.Thumb = animVals.Thumb;
                    animState.To.Thumb = thumb;
                    needTimer |= true;
                }
                // Donee. Make sure we got timer, or not.
                if (needTimer) SubscribeToTimer();
                else UnsubscribeFromTimer();
            }
        }

        /// <summary>
        /// Lock object around inertia and thumb drag scroll's members
        /// Ie stuff touched in <see cref="DoScrollTimer"/>, that is, my parent's timer handler
        /// </summary>
        private object animLOScroll = new object();

        /// <summary>
        /// Current scrolling speed.
        /// </summary>
        private float animScrollSpeed = 0;

        /// <summary>
        /// Fuel for thruster to burn.
        /// </summary>
        private float animFuel = 0;

        /// <summary>
        /// <para>Authoritative, no-animation scroll position to be signaled in timer.</para>
        /// <para>Used when scroll thumb is being dragged around. int.MinValue if thumb is not being dragged.</para>
        /// </summary>
        private int animPosInDrag = int.MinValue;

        /// <summary>
        /// <para>Counter for repeat fuel purchases (repeat fire while something is held pressed).</para>
        /// <para>Starts at 0 if we have a repeat order; int.MinValue if we don't.</para>
        /// </summary>
        private int animRepeatCounter = int.MinValue;

        /// <summary>
        /// Amount to purchase if we have a repeat purchase order.
        /// </summary>
        private float animRepeatPurchase = float.MinValue;

        /// <summary>
        /// <para>Barrier to repeat purchases.</para>
        /// <para>If repeat purchase is negative, order is canceled when top of thumb hits this Y value.</para>
        /// <para>If repeat purchase is positive, order is canceled when bottom of thumb hits this Y value.</para>
        /// </summary>
        private int animRepeatBarrierY = int.MinValue;

        /// <summary>
        /// Called in host control's timer event handler to animate inertia-based scrolling.
        /// </summary>
        /// <param name="needTimer">True if timer is still needed.</param>
        /// <param name="valueChanged">True if scroll value changed (host timer must repositing content).</param>
        public void DoScrollTimer(out bool needTimer, out bool valueChanged)
        {
            needTimer = false;
            valueChanged = false;

            lock (animLOScroll)
            {
                // Thumb is pressed, held and dragged around - no inertia
                if (animPosInDrag != int.MinValue)
                {
                    // We sure need timer
                    needTimer = true;
                    // Handle dragging and return
                    if (animPosInDrag != position)
                    {
                        position = animPosInDrag;
                        valueChanged = true;
                    }
                    return;
                }

                // Any repeat purchase?
                if (animRepeatCounter != int.MinValue)
                {
                    // Sure need timer - must keep buying
                    needTimer = true;
                    // Count; at intervals, buy
                    ++animRepeatCounter;
                    // First order is delayed by ticksDelay; repeat orders then come at tickFreq
                    int tickDelay = 5;
                    int tickFreq = 3;
                    if ((animRepeatCounter - tickDelay) % tickFreq == 0) animFuel += animRepeatPurchase;
                }

                // Animated, inertia scrolling from here on
                bool inSpringZoneBefore = position < 0 || position + pageSize > maximum;
                // If we have fuel, keep accelerating until hitting speed limit
                // Burn fuel equivalent to our speed
                if (animFuel > 0)
                {
                    float speedToAdd = 0;
                    if (animScrollSpeed < maxSpeed)
                    {
                        speedToAdd = Math.Min(animFuel, acceleration);
                        if (animScrollSpeed + speedToAdd > maxSpeed) speedToAdd = maxSpeed - animScrollSpeed;
                    }
                    animScrollSpeed += speedToAdd;
                    animFuel -= animScrollSpeed;
                    if (animFuel < 0) animFuel = 0;
                }
                // If we have negative fuel, keep accelerating the other way
                else if (animFuel < 0)
                {
                    float speedToAdd = 0;
                    if (-animScrollSpeed < maxSpeed)
                    {
                        speedToAdd = -Math.Min(-animFuel, acceleration);
                        if (-(animScrollSpeed + speedToAdd) > maxSpeed) speedToAdd = -(maxSpeed - animScrollSpeed);
                    }
                    animScrollSpeed += speedToAdd;
                    animFuel -= animScrollSpeed;
                    if (animFuel > 0) animFuel = 0;
                }
                // If we have no fuel, decelerate
                else
                {
                    if (animScrollSpeed < 0)
                    {
                        animScrollSpeed += deceleration;
                        if (animScrollSpeed > 0) animScrollSpeed = 0;
                    }
                    else if (animScrollSpeed > 0)
                    {
                        animScrollSpeed -= deceleration;
                        if (animScrollSpeed < 0) animScrollSpeed = 0;
                    }
                }

                // So, if speed is not null here, move by that much.
                int iScrollSpeed = (int)animScrollSpeed;
                if (iScrollSpeed != 0 || inSpringZoneBefore)
                {
                    needTimer = true;
                    // Update position; see if that takes us into spring zone
                    valueChanged = true;
                    position = position + iScrollSpeed;
                    bool inSpringZoneAfter = position < 0 || position + pageSize > maximum;
                    // If springiness is not tolerated at all: stop at edge; stop all this scrolling business
                    if (inSpringZoneAfter && springSize == 0)
                    {
                        if (position < 0) position = 0;
                        else if (position + pageSize > maximum) position = maximum - pageSize;
                        animFuel = 0;
                        animScrollSpeed = 0;
                        // Timer not needed - scrolling's over
                        needTimer = false;
                    }
                    // Handle bounceback
                    else if (inSpringZoneBefore)
                    {
                        // Stop at spring's edge
                        if (position < -springSize) position = -springSize;
                        else if (position > maximum - pageSize + springSize) position = maximum - pageSize + springSize;
                        // Normal speed is over; empty tank
                        if (animFuel > 0) animFuel = Math.Min(maxSpeed, animFuel);
                        else animFuel = Math.Max(-maxSpeed, animFuel);
                        animFuel = 0;
                        animScrollSpeed /= 2F;
                        // If we were already in spring zone, move back towards real edge
                        if (inSpringZoneBefore)
                        {
                            int inSpring = position < 0 ? -position : position - maximum + pageSize;
                            int proportionalBack = inSpring / 4;
                            // We move towards real edge with a combination of proportional force plus deceleration
                            int pushBack = proportionalBack + (int)(deceleration);
                            // But never cross over to real territory because of pushback
                            int inSpringAfter = inSpring - pushBack;
                            if (inSpringAfter < 0) inSpringAfter = 0;
                            if (position < 0) position = -inSpringAfter;
                            else position = maximum - pageSize + inSpringAfter;
                        }
                        // Timer still needed to bounce back
                        needTimer = true;
                    }
                    // Cancel repeat purchase if we have to
                    if (animRepeatCounter != int.MinValue)
                    {
                        Rectangle tr = getThumbRect(position);
                        if (animRepeatPurchase < 0 && tr.Top < animRepeatBarrierY) animRepeatCounter = int.MinValue;
                        else if (animRepeatPurchase > 0 && tr.Bottom > animRepeatBarrierY) animRepeatCounter = int.MinValue;
                        if (animRepeatCounter == int.MinValue) animRepeatPurchase = float.MinValue;
                    }
                }
            }
        }

        /// <summary>
        /// Adds fuel to the scrolling momentum tank.
        /// </summary>
        private void doBuyScrollFuel(int howMuch)
        {
            if (howMuch == 0) return;
            lock (animLO)
            {
                // Reversing course
                if (howMuch < 0 && animScrollSpeed > 0 || howMuch > 0 && animScrollSpeed < 0)
                {
                    animFuel = (float)howMuch;
                    animScrollSpeed = 0;
                }
                // Speeding more
                else animFuel += (float)howMuch;
                subscribeScrollTimer();
            }
        }

        /// <summary>
        /// Stops *any* scrolling in progress; leaves flexible band if we're in it.
        /// </summary>
        /// <param name="posInDrag">Mandatory position, or int.MinValue if no position is mandated.</param>
        private void doStopAnyScroll(int posInDrag = int.MinValue)
        {
            lock (animLO)
            {
                // Empty tank, grind to a halt.
                animFuel = 0;
                animScrollSpeed = 0;
                // Leave spring zone.
                if (position < 0) position = 0;
                else if (position + pageSize > maximum) position = maximum - pageSize;
                // Also stop any ongoing repeat purchase
                animRepeatCounter = int.MinValue;
                animRepeatPurchase = float.MinValue;
                // Any mandatory position? Used when scroll thumb starts getting dragged.
                animPosInDrag = posInDrag;
                if (animPosInDrag != int.MinValue) subscribeScrollTimer();
            }
        }

        public static ushort HIWORD(IntPtr l) { return (ushort)((l.ToInt64() >> 16) & 0xFFFF); }
        public static ushort LOWORD(IntPtr l) { return (ushort)((l.ToInt64()) & 0xFFFF); }

        /// <summary>
        /// Pre-filters Windows messages to fish out mouse wheel events.
        /// </summary>
        public bool PreFilterMessage(ref Message m)
        {
            bool r = false;
            if (m.Msg == 0x020A) //WM_MOUSEWHEEL
            {
                Point p = new Point((int)m.LParam);
                int delta = (Int16)HIWORD(m.WParam);
                MouseEventArgs e = new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, delta);
                m.Result = IntPtr.Zero; //don't pass it to the parent window
                onMouseWheel(e);
            }
            return r;
        }

        /// <summary>
        /// Handles the mouse wheel event: adds momentum to animated scrolling.
        /// </summary>
        private void onMouseWheel(MouseEventArgs e)
        {
            // Input from the mouse wheel adds momentum to animated scrolling.
            float extraMomentum = -((float)e.Delta) * ((float)PageSize) / 1500.0F;
            doBuyScrollFuel((int)extraMomentum);
        }

        public override void DoPaint(Graphics g)
        {
            // Get current animation values
            AnimVals av;
            lock (animLO) { av = animVals; }

            g.SmoothingMode = SmoothingMode.None;
            // Background
            using (Brush b = new SolidBrush(ZenParams.ScrollColBg))
            {
                g.FillRectangle(b, 0, 0, Width, Height);
            }
            // Top button
            using (Brush b = new SolidBrush(av.TopBtnBg))
            {
                g.FillRectangle(b, 0, 0, Width, Width);
            }
            using (Brush b = new SolidBrush(av.TopBtnArrow))
            {
                PointF[] pts = new PointF[]
                {
                    new PointF(pad, Width - pad),
                    new PointF(((float)Width) / 2.0F, pad),
                    new PointF(Width - pad, Width - pad),
                    new PointF(pad, Width - pad),
                };
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillPolygon(b, pts);
                g.SmoothingMode = SmoothingMode.None;
            }
            // Bottom button
            using (Brush b = new SolidBrush(av.BottomBtnBg))
            {
                g.FillRectangle(b, 0, Height - Width, Width, Width);
            }
            using (Brush b = new SolidBrush(av.BottomBtnArrow))
            {
                PointF[] pts = new PointF[]
                {
                    new PointF(pad, Height - Width + pad),
                    new PointF(((float)Width) / 2.0F, Height - pad),
                    new PointF(Width - pad, Height - Width + pad),
                    new PointF(pad, Height - Width + pad),
                };
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillPolygon(b, pts);
                g.SmoothingMode = SmoothingMode.None;
            }
            // Scroll thumb
            Rectangle thRect = getThumbRect(position);
            using (Brush b = new SolidBrush(av.Thumb))
            {
                g.FillRectangle(b, pad, thRect.Top + pad / 2, Width - 2 * pad, thRect.Height - pad);
            }
        }

        #endregion
    }
}
