using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.HanziLookup
{
    /// <summary>
	/// WrittenPoints are the constituent points of a WrittenStroke.
	/// WrittenPoints can be marked during character analysis.
	/// If they are marked as pivots, then that means that the point serves as 
	/// the end point of one SubStroke, and the beginning point of another.
	/// 
	/// We mark pivot status and the subStrokeIndex on these point objects
	/// so that we can display this data graphically if we desire to give a
	/// visual que on how the Strokes were divided up.
    /// </summary>
	public class WrittenPoint
    {
        /// <summary>
        /// The index of this SubStroke in the character.
        /// </summary>
        public int SubStrokeIndex;

        /// <summary>
        /// If this point is a pivot.
        /// </summary>
        public bool IsPivot;

        public readonly int X;
        public readonly int Y;
		
		public WrittenPoint(int x, int y)
        {
            X = x;
            Y = y;
		}
		
        /// <remarks>
		/// Normalized length takes into account the size of the WrittenCharacter on the canvas.
		/// For example, if the WrittenCharacter was written small in the upper left portion of the canvas,
		/// then the lengths not be based on the full size of the canvas, but rather only on the relative
		/// size of the WrittenCharacter.
		/// 
		/// @param comparePoint the point to get the normalized distance to from this point
		/// 
		/// @return the normalized length from this point to the compare point
        /// </remarks>
		public double GetDistanceNormalized(WrittenPoint comparePoint,
            double charWidth, double charHeight)
        {
			double width = (double)charWidth;
			double height = (double)charHeight;
			
			// normalizer is a diagonal along a square with sides of size the larger dimension of the bounding box
			double dimensionSquared = width > height ? width * width : height * height;
			double normalizer = Math.Sqrt(dimensionSquared + dimensionSquared);
			
			double distanceNormalized = Distance(comparePoint) / normalizer;

            // shouldn't be longer than 1 if it's normalized
            distanceNormalized = Math.Min(distanceNormalized, 1.0);
			
			return distanceNormalized;
		}

        public double Distance(WrittenPoint comparePoint)
        {
            double dx = X - comparePoint.X;
            double dy = Y - comparePoint.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
		
        /// <remarks>
		/// Calculates the direction in radians between this point and the given point.
		/// 0 is to the right, PI / 2 is up, etc.
		/// 
		/// @param comparePoint the point to get the direction to from this point
		/// @return the direction in radians between this point and the given point.
        /// </remarks>
		public double GetDirection(WrittenPoint comparePoint)
        {
			double dx = X - comparePoint.X;
			double dy = Y - comparePoint.Y;
			
			return Math.PI - Math.Atan2(dy, dx);
		}
	}
}
