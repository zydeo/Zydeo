using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DND.HanziLookup
{
    public class CubicCurve2D
    {
        public readonly double X1;
        public readonly double Y1;

        public readonly double CtrlX1;
        public readonly double CtrlY1;

        public readonly double CtrlX2;
        public readonly double CtrlY2;

        public readonly double X2;
        public readonly double Y2;

        public CubicCurve2D(double x1, double y1,
            double ctrlx1, double ctrly1,
            double ctrlx2, double ctrly2,
            double x2, double y2)
        {
            this.X1 = x1;
            this.Y1 = y1;
            this.CtrlX1 = ctrlx1;
            this.CtrlY1 = ctrly1;
            this.CtrlX2 = ctrlx2;
            this.CtrlY2 = ctrly2;
            this.X2 = x2;
            this.Y2 = y2;
        }

        public double GetYOnCurve(double t)
        {
            //double ax = getCubicAx();
            //double bx = getCubicBx();
            //double cx = getCubicCx();

            double ay = getCubicAy();
            double by = getCubicBy();
            double cy = getCubicCy();

            double tSquared = t * t;
            double tCubed = t * tSquared;

            //double x = (ax * tCubed) + (bx * tSquared) + (cx * t) + curve.getX1();
            double y = (ay * tCubed) + (by * tSquared) + (cy * t) + Y1;
            return y;
        }

        public double GetFirstSolutionForX(double x)
        {
            double[] solutions;
            SolveForX(x, out solutions);
            foreach (double d in solutions)
                if (d >= -0.00000001 && d <= 1.00000001)
                {
                    if (d >= 0.0 && d <= 1.0) return d;
                    if (d < 0.0) return 0.0;
                    return 1.0;
                }
            return double.NaN;
        }

        /// Solves the cubic curve giving parameterized t values at
        /// points where the curve has an x value matching the given value.
        /// Writes as many solutions into the given array as will fit. 
        /// 
        /// @param curve the curve
        /// @param x the value of x to solve for
        /// @param solutions an array to write solutions into
        public void SolveForX(double x, out double[] solutions)
        {
            double a = getCubicAx();
            double b = getCubicBx();
            double c = getCubicCx();
            double d = X1 - x;
            /// ax^3 + bx^2 + cx + d = 0

            double f = ((3.0 * c / a) - (b*b / (a*a))) / 3.0;
            double g = ((2.0 * b*b*b / (a*a*a)) - (9.0 * b * c / (a*a)) + (27.0 * d / a)) / 27.0;
            double h = (g * g / 4.0) + (f * f * f / 27.0);

            //!!
            //if (h < 0.2) h = 0.0;

            // There is only one real root
            if (h > 0)
            {
                double u = 0 - g;
                double r = (u / 2) + (Math.Pow(h, 0.5));
                double s6 = (Math.Pow(r, 0.333333333333333333333333333));
                double s8 = s6;
                double t8 = (u / 2) - (Math.Pow(h, 0.5));
                double v7 = (Math.Pow((0 - t8), 0.33333333333333333333));
                double v8 = (v7);
                double x3 = (s8 - v8) - (b / (3 * a));
                solutions = new double[1];
                solutions[0] = x3;

                //double r = -(g / 2.0) + Math.Sqrt(h);
                //double s = Math.Pow(r, 1.0 / 3.0);
                //double t = -(g / 2.0) - Math.Sqrt(h);
                //double u = Math.Pow(t, 1.0 / 3.0);
                //solutions = new double[1];
                //solutions[0] = s + u - (b / (3.0 * a));
            }
            // All 3 roots are real and equal
            else if (f == 0.0 && g == 0.0 && h == 0.0)
            {
                solutions = new double[1];
                solutions[0] = -Math.Pow(d / a, 1.0 / 3.0);
            }
            // All three roots are real (h <= 0)
            else
            {
                double i = Math.Sqrt((g * g / 4.0) - h);
                double j = Math.Pow(i, 1.0 / 3.0);
                double k = Math.Acos(-g / (2 * i));
                double l = j * -1.0;
                double m = Math.Cos(k / 3.0);
                double n = Math.Sqrt(3.0) * Math.Sin(k / 3.0);
                double p = (b / (3.0 * a)) * -1.0;
                solutions = new double[3];
                solutions[0] = 2.0 * j * Math.Cos(k / 3.0) - (b / (3.0 * a));
                solutions[1] = l * (m + n) + p;
                solutions[2] = l * (m - n) + p;
            }
        }

        public double getCubicAx()
        {
            return X2 - X1 - getCubicBx() - getCubicCx();
        }

        public double getCubicAy()
        {
            return Y2 - Y1 - getCubicBy() - getCubicCy();
        }

        public double getCubicBx()
        {
            return 3.0 * (CtrlX2 - CtrlX1) - getCubicCx();
        }

        public double getCubicBy()
        {
            return 3.0 * (CtrlY2 - CtrlY1) - getCubicCy();
        }

        public double getCubicCx()
        {
            return 3.0 * (CtrlX1 - X1);
        }

        public double getCubicCy()
        {
            return 3.0 * (CtrlY1 - Y1);
        }

    }
}
