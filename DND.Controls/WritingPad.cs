using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DND.Controls
{
    public partial class WritingPad : Control
    {
        public delegate void StrokesChangedDelegate(Control sender, IEnumerable<Stroke> strokes);
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
        private Bitmap dbuffer;

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
            Invalidate();
        }

        public WritingPad()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (components != null) components.Dispose();
                if (dbuffer != null) { dbuffer.Dispose(); dbuffer = null; }
            }
        }
        
        protected override void OnSizeChanged(EventArgs e)
        {
            if (dbuffer != null)
            {
                dbuffer.Dispose();
                dbuffer = null;
            }
            base.OnSizeChanged(e);
            Invalidate();
        }

        private void doPaint(Graphics g)
        {
            // Background
            using (Brush b = new SolidBrush(SystemColors.Window))
            {
                g.FillRectangle(b, ClientRectangle);
            }
            // Lines to structure character space
            using (Pen p = new Pen(Color.LightGray, 0.5F))
            {
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                g.DrawLine(p, new PointF(0, 0), new PointF(ClientSize.Width, ClientSize.Height));
                g.DrawLine(p, new PointF(ClientSize.Width, 0), new Point(0, ClientSize.Height));
            }
            using (Pen p = new Pen(Color.DarkGray, 0.5F))
            {
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                g.DrawLine(p, new PointF(((float)ClientSize.Width) / 2.0F, 0), new PointF(((float)ClientSize.Width) / 2.0F, ClientSize.Height));
                g.DrawLine(p, new PointF(0, ((float)ClientSize.Height) / 2.0F), new PointF(ClientSize.Width, ((float)ClientSize.Height) / 2.0F));
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
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //base.OnPaintBackground(pevent);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            // Do all the remaining drawing through my own hand-made double-buffering for speed
            if (dbuffer == null)
                dbuffer = new Bitmap(Width, Height);
            using (Graphics g = Graphics.FromImage(dbuffer))
            {
                doPaint(g);
            }
            pe.Graphics.DrawImageUnscaled(dbuffer, 0, 0);
        }

        private PointF normToReal(PointF pp)
        {
            return new PointF(pp.X * ((float)ClientSize.Width) / scale, pp.Y * ((float)ClientSize.Height) / scale);
        }

        private readonly List<PointF> currentPoints = new List<PointF>();

        const float scale = 250.0F;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            double x = ((double)e.X) * scale / ((double)ClientSize.Width);
            double y = ((double)e.Y) * scale / ((double)ClientSize.Height);
            currentPoints.Add(new PointF((float)x, (float)y));
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (currentPoints.Count == 0) return;
            double x = ((double)e.X) * scale / ((double)ClientSize.Width);
            double y = ((double)e.Y) * scale / ((double)ClientSize.Height);
            PointF newPoint = new PointF((float)x, (float)y);
            if (currentPoints.Count == 0) currentPoints.Add(newPoint);
            else if (currentPoints[currentPoints.Count - 1] != newPoint) currentPoints.Add(newPoint);
            Invalidate();
        }

        private static float dist(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
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
            Invalidate();
            if (StrokesChanged != null) StrokesChanged(this, Strokes);
        }
    }
}
