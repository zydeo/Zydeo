using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DND.Gui.Zen;

namespace DND.Gui
{
    public partial class WritingPad : ZenControl
    {
        public delegate void StrokesChangedDelegate(object sender, IEnumerable<Stroke> strokes);
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
        }

        public WritingPad(ZenControl owner)
            : base(owner)
        {
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

        PointF bloatFromCenter(PointF p, float bloat)
        {
            if (bloat == 1) return p;
            float dx = p.X - Width / 2.0F;
            float dy = p.Y - Height / 2.0F;
            return new PointF(Width / 2.0F + bloat * dx, Height / 2.0F + bloat * dy);
        }

        private void doPaintStroke(Graphics g, Pen p, ReadOnlyCollection<PointF> points, float bloat)
        {
            PointF lastRP = bloatFromCenter(normToReal(points[0]), bloat);
            if (points.Count == 1)
                g.DrawLine(p, lastRP, lastRP);
            else
            {
                for (int i = 1; i < points.Count; ++i)
                {
                    PointF thisNP = points[i];
                    PointF thisRP = bloatFromCenter(normToReal(thisNP), bloat);
                    g.DrawLine(p, lastRP, thisRP);
                    lastRP = thisRP;
                }
            }
        }

        private void doPaintStrokesNormal(Graphics g, Dictionary<Stroke, float> animStates)
        {
            // All strokes collected so far, plus current points
            // Except strokes being animated: we'll treat those separately
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            float thickness = strokeThicknessLogical * Scale;
            using (Pen p = new Pen(Color.Black, thickness))
            {
                p.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                p.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                // Previous strokes
                foreach (Stroke stroke in strokes)
                {
                    // If stroke is being animated, skip here
                    if (animStates.ContainsKey(stroke)) continue;
                    doPaintStroke(g, p, stroke.Points, 1);
                }
                // Current stroke in progress
                if (currentPoints.Count > 0)
                {
                    doPaintStroke(g, p, new ReadOnlyCollection<PointF>(currentPoints), 1);
                }
            }
            // Strokes being animated: different pen for each of 'em
            foreach (var x in animStates)
            {
                // Anim state goes from 0 to 1. Line goes from thick to normal, light gray to black.
                float thickStart = strokeThicknessAnimStart * Scale;
                float thickNow = thickStart - (thickStart - thickness) * x.Value;
                int brightNow = (int)(strokeBrightnessAnimStart * (1.0F - Math.Pow(x.Value, 3.0)));
                Color colNow = Color.FromArgb(brightNow, brightNow, brightNow);
                using (Pen p = new Pen(colNow, thickNow))
                {
                    p.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    p.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    doPaintStroke(g, p, x.Key.Points, 1);
                }
            }
        }

        private void doPaintStrokesClearing(Graphics g, float clearState)
        {
            float bloat = 1 / (float)Math.Pow(1 + clearState * 5.0F, 2.0F);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            float thickness = strokeThicknessLogical * Scale * bloat;
            int brightNow = (int)(strokeBrightnessAnimStart * Math.Pow(clearState, 3.0));
            Color colNow = Color.FromArgb(brightNow, brightNow, brightNow);
            using (Pen p = new Pen(colNow, thickness))
            {
                p.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                p.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                // Previous strokes
                foreach (Stroke stroke in strokes)
                {
                    doPaintStroke(g, p, stroke.Points, bloat);
                }
            }
        }

        public override void DoPaint(Graphics g)
        {
            // Get strokes under animation - those will get special treatment.
            // Must lock for thread safety
            Dictionary<Stroke, float> animStates = new Dictionary<Stroke, float>();
            lock (strokeAnimStates)
            {
                foreach (var x in strokeAnimStates) animStates[x.Key] = x.Value;
            }

            // Background
            using (Brush b = new SolidBrush(Color.White))
            {
                g.FillRectangle(b, 0, 0, Width, Height);
            }
            // Clear animation affects lines
            float clearState;
            lock (clearAnimLockObj) { clearState = clearAnimState; }
            // If not mid-animation, draw full extent
            if (clearState < 0) clearState = 1;
            // Diagonal lines
            using (Pen p = new Pen(Color.LightGray, 0.5F))
            {
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                if (clearState == 1)
                {
                    g.DrawLine(p, new PointF(0, 0), new PointF(Width, Height));
                    g.DrawLine(p, new PointF(Width, 0), new PointF(0, Height));
                }
                else
                {
                    g.DrawLine(p, new PointF(0, 0), new PointF(Width * clearState / 2.0F, Height * clearState / 2.0F));
                    g.DrawLine(p, new PointF(Width, Height), new PointF(Width - Width * clearState / 2.0F, Height - Height * clearState / 2.0F));
                    g.DrawLine(p, new PointF(Width, 0), new PointF(Width - Width * clearState / 2.0F, Height * clearState / 2.0F));
                    g.DrawLine(p, new PointF(0, Height), new PointF(Width * clearState / 2.0F, Height - Height * clearState / 2.0F));
                }
            }
            // Horizontal and vertical lines
            using (Pen p = new Pen(Color.DarkGray, 0.5F))
            {
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                if (clearState == 1)
                {
                    g.DrawLine(p, new PointF(Width / 2.0F, 0), new PointF(Width / 2.0F, Height));
                    g.DrawLine(p, new PointF(0, Height / 2.0F), new PointF(Width, Height / 2.0F));
                }
                else
                {
                    g.DrawLine(p, new PointF(Width / 2.0F, Height / 2.0F - Height * clearState / 2.0F), new PointF(Width / 2.0F, Height / 2.0F + Height * clearState / 2.0F));
                    g.DrawLine(p, new PointF(Width / 2.0F - Width * clearState / 2.0F, Height / 2.0F), new PointF(Width / 2.0F + Width * clearState / 2.0F, Height / 2.0F));
                }
            }
            // Border
            using (Pen p = new Pen(SystemColors.ControlDarkDark))
            {
                g.DrawLine(p, 0, 0, Width, 0);
                g.DrawLine(p, Width - 1, 0, Width - 1, Height);
                g.DrawLine(p, Width - 1, Height - 1, 0, Height - 1);
                g.DrawLine(p, 0, Height - 1, 0, 0);
            }
            // If not in clear animation, paint normal strokes and possibly last,animated stroke
            if (clearState == 1) doPaintStrokesNormal(g, animStates);
            // Otherwise, bloat away last character
            else doPaintStrokesClearing(g, clearState);
        }

        private PointF normToReal(PointF pp)
        {
            return new PointF(pp.X * ((float)Size.Width) / canvasScale, pp.Y * ((float)Size.Height) / canvasScale);
        }

        public override bool DoMouseDown(Point p, MouseButtons button)
        {
            if (!receiving) return true;

            double x = ((double)p.X) * canvasScale / ((double)Size.Width);
            double y = ((double)p.Y) * canvasScale / ((double)Size.Height);
            currentPoints.Add(new PointF((float)x, (float)y));
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
            if (StrokesChanged != null) StrokesChanged(this, Strokes);
        }

        public override bool DoMouseUp(Point p, MouseButtons button)
        {
            doStrokeOver();
            return true;
        }

        public override void DoMouseLeave()
        {
            doStrokeOver();
        }
    }
}
