using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DND.Controls
{
    public partial class WritingPad : ZenControl
    {
        public delegate void StrokesChangedDelegate(object sender, IEnumerable<Stroke> strokes);
        public event StrokesChangedDelegate StrokesChanged;

        public class Stroke
        {
            private readonly PointF[] points;
            public IEnumerable<PointF> Points
            {
                get { return points; }
            }

            public int PointCount
            {
                get { return points.Length; }
            }

            public PointF GetPointAt(int index)
            {
                return points[index];
            }

            internal Stroke(List<PointF> points)
            {
                this.points = new PointF[points.Count];
                for (int i = 0; i != points.Count; ++i) this.points[i] = points[i];
            }
        }

        private readonly List<Stroke> strokes = new List<Stroke>();

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
            strokes.Clear();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        public WritingPad(float scale, IZenControlOwner owner)
            : base(scale, owner)
        {
        }
        
        protected override void OnSizeChanged()
        {
            MakeMePaint(true, RenderMode.Invalidate);
        }

        public override void DoPaint(Graphics g)
        {
            Region oldClip = g.Clip;
            Matrix oldTransform = g.Transform;
            Rectangle rect = AbsRect;
            g.Clip = new Region(new Rectangle(AbsLeft, AbsTop, Width, Height));
            g.TranslateTransform(rect.X, rect.Y);

            // Background
            using (Brush b = new SolidBrush(Color.White))
            {
                g.FillRectangle(b, 0, 0, rect.Width, rect.Height);
            }
            // Lines to structure character space
            using (Pen p = new Pen(Color.LightGray, 0.5F))
            {
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                g.DrawLine(p, new PointF(0, 0), new PointF(Size.Width, Size.Height));
                g.DrawLine(p, new PointF(Size.Width, 0), new Point(0, Size.Height));
            }
            // Border
            using (Pen p = new Pen(SystemColors.ControlDarkDark))
            {
                g.DrawLine(p, 0, 0, Width, 0);
                g.DrawLine(p, Width - 1, 0, Width - 1, Height);
                g.DrawLine(p, Width - 1, Height - 1, 0, Height - 1);
                g.DrawLine(p, 0, Height - 1, 0, 0);
            }
            using (Pen p = new Pen(Color.DarkGray, 0.5F))
            {
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                g.DrawLine(p, new PointF(((float)Size.Width) / 2.0F, 0), new PointF(((float)Size.Width) / 2.0F, Size.Height));
                g.DrawLine(p, new PointF(0, ((float)Size.Height) / 2.0F), new PointF(Size.Width, ((float)Size.Height) / 2.0F));
            }
            // All strokes collectd so far, plus current points
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            using (Pen p = new Pen(Color.Black, 5.0F))
            {
                p.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                p.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                // Previous strokes
                foreach (Stroke stroke in strokes)
                {
                    PointF lastRP = normToReal(stroke.GetPointAt(0));
                    if (stroke.PointCount == 1)
                        g.DrawLine(p, lastRP, lastRP);
                    else
                    {
                        for (int i = 1; i != stroke.PointCount; ++i)
                        {
                            PointF thisNP = stroke.GetPointAt(i);
                            PointF thisRP = normToReal(thisNP);
                            g.DrawLine(p, lastRP, thisRP);
                            lastRP = thisRP;
                        }
                    }
                }
                // Current stroke in progress
                if (currentPoints.Count > 0)
                {
                    PointF lastRP = normToReal(currentPoints[0]);
                    if (currentPoints.Count == 1)
                        g.DrawLine(p, lastRP, lastRP);
                    else
                    {
                        for (int i = 1; i != currentPoints.Count; ++i)
                        {
                            PointF thisNP = currentPoints[i];
                            PointF thisRP = normToReal(thisNP);
                            g.DrawLine(p, lastRP, thisRP);
                            lastRP = thisRP;
                        }
                    }
                }
            }

            g.Transform = oldTransform;
            g.Clip = oldClip;
        }

        private PointF normToReal(PointF pp)
        {
            return new PointF(pp.X * ((float)Size.Width) / canvasScale, pp.Y * ((float)Size.Height) / canvasScale);
        }

        private readonly List<PointF> currentPoints = new List<PointF>();

        const float canvasScale = 250.0F;

        public override bool DoMouseDown(Point p, MouseButtons button)
        {
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

        public override bool DoMouseUp(Point p, MouseButtons button)
        {
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
                Stroke newStroke = new Stroke(strokePoints);
                strokes.Add(newStroke);
            }
            currentPoints.Clear();
            MakeMePaint(false, RenderMode.Invalidate);
            if (StrokesChanged != null) StrokesChanged(this, Strokes);
            return true;
        }
    }
}
