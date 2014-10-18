using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using DND.Gui.Zen;

namespace DND.Gui
{
    partial class WritingPad
    {
        private void createMyCursor()
        {
            int dia = (int)(strokeThicknessLogical * Scale);
            if ((dia / 2) * 2 != dia) dia += 1;
            Rectangle rect = new Rectangle(0, 0, 5 * dia + 1, 5 * dia + 1);

            using (Bitmap bmp = new Bitmap(rect.Width, rect.Height))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Color bgCol = Color.FromArgb(0, 255, 255, 255);
                using (Brush b = new SolidBrush(bgCol))
                {
                    g.FillRectangle(b, 0, 0, bmp.Width, bmp.Height);
                }
                g.SmoothingMode = SmoothingMode.HighQuality;
                Color col = Color.Gray;
                using (Brush b = new SolidBrush(col))
                {
                    Rectangle dotRect = new Rectangle(2 * dia, 2 * dia, dia, dia);
                    g.FillEllipse(b, dotRect);
                }
                using (Pen p = new Pen(col))
                {
                    g.DrawEllipse(p, dia + dia / 2, dia + dia / 2, 2 * dia, 2 * dia);
                }
                myCursor = CustomCursor.CreateCursor(bmp, 2 * dia + dia / 2, 2 * dia + dia / 2);
            }
        }

        private PointF bloatFromCenter(PointF p, float bloat)
        {
            if (bloat == 1) return p;
            float dx = p.X - Width / 2.0F;
            float dy = p.Y - Height / 2.0F;
            return new PointF(Width / 2.0F + bloat * dx, Height / 2.0F + bloat * dy);
        }

        private PointF normToReal(PointF pp)
        {
            return new PointF(pp.X * ((float)Size.Width) / canvasScale, pp.Y * ((float)Size.Height) / canvasScale);
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

        private void doPaintHint(Graphics g)
        {
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            using (Brush bTxt = new SolidBrush(Color.FromArgb(Magic.WritingPadHintOpacity, ZenParams.StandardTextColor)))
            using (Brush bBack = new SolidBrush(Color.FromArgb(160, ZenParams.WindowColor)))
            using (Font f = new Font(ZenParams.GenericFontFamily, 10.0F))
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                string str = "Draw here, then pick recognized character below";
                RectangleF rect = new Rectangle(Width / 8, Height / 4, 3 * Width / 4, Height / 2);
                g.FillRectangle(bBack, rect);
                g.DrawString(str, f, bTxt, rect, sf);
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
            using (Brush b = new SolidBrush(ZenParams.WindowColor))
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
            using (Pen p = new Pen(ZenParams.BorderColor))
            {
                g.DrawLine(p, 0, 0, Width, 0);
                g.DrawLine(p, Width - 1, 0, Width - 1, Height);
                g.DrawLine(p, Width - 1, Height - 1, 0, Height - 1);
                g.DrawLine(p, 0, Height - 1, 0, 0);
            }
            // If not in clear animation, and we have no strokes, and mouse is over us, and button is not pressed
            // Draw hint
            if (clearState == 1 && strokes.Count == 0 && currentPoints.Count == 0 && Cursor != myCursor)
                doPaintHint(g);
            // If not in clear animation, paint normal strokes and possibly last,animated stroke
            if (clearState == 1) doPaintStrokesNormal(g, animStates);
            // Otherwise, bloat away last character
            else doPaintStrokesClearing(g, clearState);
        }
    }
}
