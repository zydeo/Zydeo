using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DND.Gui.Zen;

namespace DND.Gui
{
    public partial class WritingPad : ZenControl
    {
        public delegate void StrokesChangedDelegate(IEnumerable<Stroke> strokes);
        public event StrokesChangedDelegate StrokesChanged;

        public class Stroke
        {
            private readonly PointF[] points;

            public ReadOnlyCollection<PointF> Points
            {
                get { return new ReadOnlyCollection<PointF>(points); }
            }

            internal Stroke(List<PointF> points)
            {
                this.points = new PointF[points.Count];
                for (int i = 0; i != points.Count; ++i) this.points[i] = points[i];
            }
        }

        /// <summary>
        /// The cursor shown in the writing pad - custom-drawn.
        /// </summary>
        private Cursor myCursor;

        private readonly List<PointF> currentPoints = new List<PointF>();
        const float canvasScale = 250.0F;
        const float strokeThicknessLogical = 5.0F;
        const float strokeThicknessAnimStart = 10.0F;
        const float strokeBrightnessAnimStart = 180.0F;
        private readonly List<Stroke> strokes = new List<Stroke>();
        private bool receiving = true;

        public IEnumerable<Stroke> Strokes
        {
            get { return strokes; }
        }

        public int StrokeCount
        {
            get { return strokes.Count; }
        }

        public void Clear()
        {
            if (strokes.Count == 0) return;
            lock (clearAnimLockObj)
            {
                receiving = false;
                clearAnimState = 0;
                SubscribeToTimer();
            }
            if (StrokesChanged != null) StrokesChanged(new Stroke[0]);
        }

        public void UndoLast()
        {
            if (strokes.Count == 0) return;
            strokes.RemoveAt(strokes.Count - 1);
            // Repaint; tell the world
            MakeMePaint(false, RenderMode.Invalidate);
            if (StrokesChanged != null) StrokesChanged(Strokes);
        }

        public WritingPad(ZenControl owner)
            : base(owner)
        {
            createMyCursor();
        }

        public override void Dispose()
        {
            if (myCursor != null) myCursor.Dispose();
            base.Dispose();
        }
        
        protected override void OnSizeChanged()
        {
            MakeMePaint(true, RenderMode.Invalidate);
        }

        /// <summary>
        /// Holds latest strokes being animated. Value goes from 0 to 1; when 1 is reached, item is removed.
        /// </summary>
        private readonly Dictionary<Stroke, float> strokeAnimStates = new Dictionary<Stroke, float>();

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

        private object clearAnimLockObj = new object();
        private float clearAnimState = -1.0F;

        private bool doAnimClear()
        {
            float state;
            lock (clearAnimLockObj) { state = clearAnimState; }
            // Not in clear animation
            if (state < 0) return false;
            // Clear animation over
            if (state >= 1.0F)
            {
                lock (clearAnimLockObj)
                {
                    clearAnimState = -1.0F;
                    strokes.Clear();
                    receiving = true;
                }
                return true;
            }
            // Nudge on
            lock (clearAnimLockObj)
            {
                clearAnimState = state + 0.1F;
            }
            return true;
        }

        public override void DoTimer()
        {
            bool needsPaint = doAnimLastStroke();
            needsPaint |= doAnimClear();
            // Request a repaint
            if (needsPaint) MakeMePaint(false, RenderMode.Invalidate);
            else UnsubscribeFromTimer();
        }

        public override bool DoMouseDown(Point p, MouseButtons button)
        {
            if (!receiving) return true;

            double x = ((double)p.X) * canvasScale / ((double)Size.Width);
            double y = ((double)p.Y) * canvasScale / ((double)Size.Height);
            currentPoints.Add(new PointF((float)x, (float)y));
            // Needed to show/hide hint
            MakeMePaint(false, RenderMode.Invalidate);
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
            // Needed to show/hide hint
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
            MakeMePaint(false, RenderMode.Invalidate);
        }

        public override void DoMouseLeave()
        {
            Cursor = Cursors.Arrow;
            doStrokeOver();
            MakeMePaint(false, RenderMode.Invalidate);
        }
    }
}
