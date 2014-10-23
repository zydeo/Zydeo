using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ZD.Common;
using ZD.Gui.Zen;

namespace ZD.Gui
{
    public partial class WritingPad : ZenControl
    {
        /// <summary>
        /// Delegate for <see cref="StrokesChanged"/> event.
        /// </summary>
        /// <param name="strokes"></param>
        public delegate void StrokesChangedDelegate(IEnumerable<Stroke> strokes);
        /// <summary>
        /// Triggered when strokes have changed (user added something nice to drawing on canvas).
        /// </summary>
        public event StrokesChangedDelegate StrokesChanged;

        // --- My magic. Most important: recognizer apparently works well when canvas is 250x250.
        const float canvasScale = 250.0F;
        const float strokeThicknessLogical = 5.0F;
        const float strokeThicknessAnimStart = 10.0F;
        const float strokeBrightnessAnimStart = 180.0F;
        // --- END My magic.

        /// <summary>
        /// One strokes: a series of points tracing the mouse as user drew with button pressed.
        /// </summary>
        public class Stroke
        {
            /// <summary>
            /// Array of pints.
            /// </summary>
            private readonly PointF[] points;

            /// <summary>
            /// Gets the stroke's points.
            /// </summary>
            public ReadOnlyCollection<PointF> Points
            {
                get { return new ReadOnlyCollection<PointF>(points); }
            }

            /// <summary>
            /// Ctor: initializes immutable instance with stroke's points.
            /// </summary>
            /// <param name="points"></param>
            internal Stroke(List<PointF> points)
            {
                this.points = new PointF[points.Count];
                for (int i = 0; i != points.Count; ++i) this.points[i] = points[i];
            }
        }

        /// <summary>
        /// My localized UI strings provider.
        /// </summary>
        private readonly ITextProvider tprov;

        /// <summary>
        /// The cursor shown in the writing pad - custom-drawn.
        /// </summary>
        private Cursor myCursor;

        /// <summary>
        /// Point gathered since user pressed mouse button (it's still pressed).
        /// </summary>
        private readonly List<PointF> currentPoints = new List<PointF>();

        /// <summary>
        /// Strokes drawn so far since current character was started.
        /// </summary>
        private readonly List<Stroke> strokes = new List<Stroke>();

        /// <summary>
        /// True if control is receing user input. False when animating character; then, input is ignored.
        /// </summary>
        private bool receiving = true;
        /// <summary>
        /// True if user has already drawn something (anything) in control since it was created.
        /// </summary>
        private bool alreadyDrawnSomething = false;

        /// <summary>
        /// Ctor.
        /// </summary>
        public WritingPad(ZenControl owner, ITextProvider tprov)
            : base(owner)
        {
            this.tprov = tprov;
            createMyCursor();
        }

        /// <summary>
        /// Dispose: frees owned resources.
        /// </summary>
        public override void Dispose()
        {
            if (myCursor != null) myCursor.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Gets strokes of the currently drawn character.
        /// </summary>
        public IEnumerable<Stroke> Strokes
        {
            get { return strokes; }
        }

        /// <summary>
        /// Gets number of currently drawn strokes.
        /// </summary>
        public int StrokeCount
        {
            get { return strokes.Count; }
        }

        /// <summary>
        /// Clears data (removes currently drawn character; animates it out of view).
        /// </summary>
        public void Clear()
        {
            if (strokes.Count == 0) return;
            lock (animLO)
            {
                receiving = false;
                clearAnimState = 0;
                SubscribeToTimer();
            }
            if (StrokesChanged != null) StrokesChanged(new Stroke[0]);
        }

        /// <summary>
        /// Removes last drawn stroke.
        /// </summary>
        public void UndoLast()
        {
            if (strokes.Count == 0) return;
            strokes.RemoveAt(strokes.Count - 1);
            // Repaint; tell the world
            MakeMePaint(false, RenderMode.Invalidate);
            if (StrokesChanged != null) StrokesChanged(Strokes);
        }
        
        /// <summary>
        /// Does whatever must be done when control's size changes. Like, redraw.
        /// </summary>
        protected override void OnSizeChanged()
        {
            MakeMePaint(true, RenderMode.Invalidate);
        }

        /// <summary>
        /// Returns whether the hint needs to be shown or not.
        /// </summary>
        private bool isHintNeeded()
        {
            return !alreadyDrawnSomething && strokes.Count == 0 && currentPoints.Count == 0 && Cursor != myCursor;
        }

        /// <summary>
        /// Holds latest strokes being animated. Value goes from 0 to 1; when 1 is reached, item is removed.
        /// </summary>
        private readonly Dictionary<Stroke, float> strokeAnimStates = new Dictionary<Stroke, float>();

        /// <summary>
        /// Nudges on current last finished stroke's animation.
        /// </summary>
        /// <returns>True if timer is still needed.</returns>
        private bool doAnimLastStroke()
        {
            lock (strokeAnimStates)
            {
                if (strokeAnimStates.Count == 0) return false;
                // Remove those that have completed their animation; nudge others on
                List<Stroke> toRemove = new List<Stroke>();
                List<Stroke> toNudge = new List<Stroke>();
                foreach (Stroke stroke in strokeAnimStates.Keys)
                {
                    if (strokeAnimStates[stroke] >= 1.0F) toRemove.Add(stroke);
                    else toNudge.Add(stroke);
                }
                foreach (Stroke stroke in toRemove) strokeAnimStates.Remove(stroke);
                foreach (Stroke stroke in toNudge) strokeAnimStates[stroke] += 0.1F;
            }
            return true;
        }

        /// <summary>
        /// Lock object for all non-stroke-related animation states.
        /// </summary>
        private object animLO = new object();
        /// <summary>
        /// State of current "clear canvas" animation.
        /// </summary>
        private float clearAnimState = -1.0F;
        /// <summary>
        /// Current strength of hint: 0 if no hint, 1 if fully visible.
        /// </summary>
        private float hintAnimState = 1.0F;
        /// <summary>
        /// If true, hint is fading out. If false, it is coming in.
        /// </summary>
        private bool hintAnimOut = false;

        /// <summary>
        /// Nudges on "show/hide hint" animation.
        /// </summary>
        /// <returns>True if timer is still needed.</returns>
        private bool doAnimHint()
        {
            lock (animLO)
            {
                // Done coming in
                if (hintAnimState >= 1.0F && !hintAnimOut)
                {
                    hintAnimState = 1.0F;
                    return false;
                }
                // Done fading out
                if (hintAnimState <= 0 && hintAnimOut)
                {
                    hintAnimState = 0;
                    return false;
                }
                // Nudge on
                if (hintAnimOut) hintAnimState -= 0.1F;
                else hintAnimState += 0.02F;
                return true;
            }
        }

        /// <summary>
        /// Starts the hint animation (fade in or fade out).
        /// </summary>
        /// <param name="showHint">If true, brings hint in. Otherwise, fades it out.</param>
        private void doStartHintAnimation(bool showHint)
        {
            lock (animLO)
            {
                hintAnimOut = !showHint;
                // Start timer only if we actually need to animate
                if (showHint && hintAnimState < 1.0F || !showHint && hintAnimState > 0)
                    SubscribeToTimer();
            }
        }

        /// <summary>
        /// Nudges on "clear canvas" animation.
        /// </summary>
        /// <returns>True if timer is still needed.</returns>
        private bool doAnimClear()
        {
            float state;
            lock (animLO) { state = clearAnimState; }
            // Not in clear animation
            if (state < 0) return false;
            // Clear animation over
            if (state >= 1.0F)
            {
                lock (animLO)
                {
                    clearAnimState = -1.0F;
                    strokes.Clear();
                    receiving = true;
                }
                return true;
            }
            // Nudge on
            lock (animLO)
            {
                clearAnimState = state + 0.1F;
            }
            return true;
        }

        /// <summary>
        /// Handles timer for animations.
        /// </summary>
        public override void DoTimer(out bool? needBackground, out RenderMode? renderMode)
        {
            bool needsPaint = doAnimLastStroke();
            needsPaint |= doAnimClear();
            needsPaint |= doAnimHint();
            // Request a repaint
            if (needsPaint) { needBackground = false; renderMode = RenderMode.Invalidate; }
            else
            {
                needBackground = null; renderMode = null;
                UnsubscribeFromTimer();
            }
        }

        public override bool DoMouseDown(Point p, MouseButtons button)
        {
            if (!receiving) return true;

            double x = ((double)p.X) * canvasScale / ((double)Size.Width);
            double y = ((double)p.Y) * canvasScale / ((double)Size.Height);
            currentPoints.Add(new PointF((float)x, (float)y));
            return true;
        }

        public override bool DoMouseMove(Point p, MouseButtons button)
        {
            if (currentPoints.Count == 0) return true;
            double x = ((double)p.X) * canvasScale / ((double)Size.Width);
            double y = ((double)p.Y) * canvasScale / ((double)Size.Height);
            PointF newPoint = new PointF((float)x, (float)y);
            if (currentPoints.Count == 0) currentPoints.Add(newPoint);
            else if (currentPoints[currentPoints.Count - 1] != newPoint) currentPoints.Add(newPoint);
            MakeMePaint(false, RenderMode.Invalidate);
            return true;
        }

        private static float dist(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private void doStrokeOver()
        {
            Stroke newStroke = null;
            if (currentPoints.Count > 1)
            {
                List<PointF> strokePoints = new List<PointF>();
                strokePoints.Add(currentPoints[0]);
                for (int i = 1; i != currentPoints.Count; ++i)
                {
                    PointF lastPoint = strokePoints[strokePoints.Count - 1];
                    PointF thisCurrPoint = currentPoints[i];
                    if (i == currentPoints.Count - 1 || dist(lastPoint, thisCurrPoint) >= 5.0F)
                        strokePoints.Add(thisCurrPoint);
                }
                newStroke = new Stroke(strokePoints);
                strokes.Add(newStroke);
                alreadyDrawnSomething = true;
            }
            currentPoints.Clear();
            if (newStroke == null) return;
            // Add to strokes being animated
            lock (strokeAnimStates)
            {
                strokeAnimStates[newStroke] = 0;
                SubscribeToTimer();
            }
            // Repaint; tell the world
            MakeMePaint(false, RenderMode.Invalidate);
            if (StrokesChanged != null) StrokesChanged(Strokes);
        }

        public override bool DoMouseUp(Point p, MouseButtons button)
        {
            doStrokeOver();
            return true;
        }

        public override void DoMouseEnter()
        {
            base.DoMouseEnter();
            Cursor = myCursor;
            doStartHintAnimation(false);
            // Needed to show/hide hint
            MakeMePaint(false, RenderMode.Invalidate);
        }

        public override void DoMouseLeave()
        {
            Cursor = Cursors.Arrow;
            doStrokeOver();
            doStartHintAnimation(isHintNeeded());
            // Needed to show/hide hint
            MakeMePaint(false, RenderMode.Invalidate);
        }
    }
}
