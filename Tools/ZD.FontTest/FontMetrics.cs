using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ZD.FontTest
{
    public class FontMetrics
    {
        public readonly double Baseline;
        public readonly double Height;
        public readonly double CapsHeight;
        public readonly double UnderlinePosition;
        public readonly double XHeight;
        public readonly double TopSideBearing;
        public readonly double BottomSideBearing;

        public FontMetrics(Typeface tf, GlyphTypeface gtf)
        {
            CapsHeight = tf.CapsHeight;
            UnderlinePosition = tf.UnderlinePosition;
            XHeight = tf.XHeight;
            Baseline = gtf.Baseline;
            Height = gtf.Height;
            TopSideBearing = gtf.TopSideBearings[1000];
            BottomSideBearing = gtf.BottomSideBearings[1000];
        }

        private static string mns(string str)
        {
            if (str.StartsWith("-")) return str;
            return " " + str;
        }

        public string Print()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Baseline                 " + mns(Baseline.ToString("N5")));
            sb.AppendLine("Height                   " + mns(Height.ToString("N5")));
            sb.AppendLine("CapsHeight               " + mns(CapsHeight.ToString("N5")));
            sb.AppendLine("UnderlinePosition        " + mns(UnderlinePosition.ToString("N5")));
            sb.AppendLine("XHeight                  " + mns(XHeight.ToString("N5")));
            sb.AppendLine("TopSideBearing           " + mns(TopSideBearing.ToString("N5")));
            sb.AppendLine("BottomSideBearing        " + mns(BottomSideBearing.ToString("N5")));
            return sb.ToString();
        }
    }
}
