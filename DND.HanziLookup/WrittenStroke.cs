using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.HanziLookup
{
	/// <summary>
    /// A WrittenStroke holds onto a list of points.
	/// It can use those points analyze itself and build a List of SubStrokes.
	/// 
	/// Analyzing and building a SubStroke List is a two-stage process.
	/// The Stroke must be analyzed before the List can be built.
	/// The reason analyzing and marking is separate from building the List is mostly 
	/// so that we could graphically display the SubStroke segments if we chose to.
    /// </summary>
	public class WrittenStroke
    {
		
		// pointList contains WrittenPoints
		private List<WrittenPoint> pointList = new List<WrittenPoint>();
		
		// Flag to see if this stroke has already been analyzed
		private bool isAnalyzed = false;
		
		public WrittenStroke()
        {
			// noop
		}
		
        public List<WrittenPoint> PointList
        {
            get { return pointList; }
        }
		
		public bool IsAnalyzed
        {
            get { return isAnalyzed; }
		}

		public void AddPoint(WrittenPoint point,
            ref double charLeftX, ref double charRightX,
            ref double charTopY, ref double charBottomY)
        {
			int pointX = point.X;
			int pointY = point.Y;
			
			// Expand the bounding box coordinates for this WrittenCharacter in necessary.
			charLeftX = Math.Min(pointX, charLeftX);
			charRightX = Math.Max(pointX, charRightX);
			charTopY = Math.Min(pointY, charTopY);
			charBottomY = Math.Max(pointY, charBottomY);
			
			this.pointList.Add(point);
		}
		
		// Defines the minimum length of a SubStroke segment.
		// If a two pivot points are within this length, the first of the pivots will be unmarked as a pivot.
		private const double MIN_SEGMENT_LENGTH = 12.5;
		
		// Used to find abrupt corners in a stroke that delimit two SubStrokes.
		private const double MAX_LOCAL_LENGTH_RATIO = 1.1;
		
		// Used to find a gradual transition between one SubStroke and another at a curve.
        private const double MAX_RUNNING_LENGTH_RATIO = 1.09;
		
		public List<SubStrokeDescriptor> GetSubStrokes(double charWidth, double charHeight)
        {
			if(!this.isAnalyzed) this.AnalyzeAndMark();

            List<SubStrokeDescriptor> subStrokes = new List<SubStrokeDescriptor>();
			
			// Any WrittenStroke should have at least two points, (a single point cannot constitute a Stroke).
			// We should therefore be safe calling an iterator without checking for the first point.
			WrittenPoint previousPoint = pointList[0];
				
            for (int i = 1; i != pointList.Count; ++i)
			{
				WrittenPoint nextPoint = pointList[i];
					
				if(nextPoint.IsPivot)
                {
					// The direction from each previous point to each successive point, in radians.
					double direction = previousPoint.GetDirection(nextPoint);
						
					// Use the normalized length, to account for relative character size.
					double normalizedLength = previousPoint.GetDistanceNormalized(nextPoint, charWidth, charHeight);

					SubStrokeDescriptor subStroke = new SubStrokeDescriptor(direction, normalizedLength);
					subStrokes.Add(subStroke);

					previousPoint = nextPoint;
				}
			}
			
			return subStrokes;
		}
		
        /// <summary>
		/// Analyzes the given WrittenStroke and marks its constituent WrittenPoints to demarcate the SubStrokes.
		/// Points that demarcate between the SubStroke segments are marked as pivot points.
		/// These pivot points can later be used to build up a List of SubStroke objects.
        /// </summary>
		public void AnalyzeAndMark()
        {
            var pointIter = pointList.GetEnumerator();
			
			// It should be impossible for a stroke to have < 2 points, so we are safe calling next() twice.
            pointIter.MoveNext();
			WrittenPoint firstPoint = pointIter.Current;
			WrittenPoint previousPoint = firstPoint;
            pointIter.MoveNext();
			WrittenPoint pivotPoint = pointIter.Current;
			
			// The first point of a Stroke is always a pivot point.
            firstPoint.IsPivot = true;
			int subStrokeIndex = 1;
			
			// The first point and the next point are always part of the first SubStroke.
			firstPoint.SubStrokeIndex = subStrokeIndex;
			pivotPoint.SubStrokeIndex = subStrokeIndex;
			
			// localLength keeps track of the immediate distance between the latest three points.
			// We can use the localLength to find an abrupt change in SubStrokes, such as at a corner.
			// We do this by checking localLength against the distance between the first and last
			// of the three points.  If localLength is more than a certain amount longer than the
			// length between the first and last point, then there must have been a corner of some kind.
			double localLength = firstPoint.Distance(pivotPoint);
			
			// runningLength keeps track of the length between the start of the current SubStroke
			// and the point we are currently examining.  If the runningLength becomes a certain
			// amount longer than the straight distance between the first point and the current
			// point, then there is a new SubStroke.  This accounts for a more gradual change
			// from one SubStroke segment to another, such as at a longish curve.
			double runningLength = localLength;
			
			// Iterate over the points, marking the appropriate ones as pivots.
			while(pointIter.MoveNext())
            {
				WrittenPoint nextPoint = pointIter.Current;
				
				// pivotPoint is the point we're currently examining to see if it's a pivot.
				// We get the distance between this point and the next point and add it
				// to the length sums we're using.
				double pivotLength = pivotPoint.Distance(nextPoint);
				localLength += pivotLength;
				runningLength += pivotLength;
				
				// Check the lengths against the ratios.  If the lengths are a certain among
				// longer than a straight line between the first and last point, then we
				// mark the point as a pivot.
                double distFromPrevious = previousPoint.Distance(nextPoint);
                double distFromFirst = firstPoint.Distance(nextPoint);
				if (localLength > MAX_LOCAL_LENGTH_RATIO * distFromPrevious ||
				   runningLength > MAX_RUNNING_LENGTH_RATIO * distFromFirst)
                {
					
					if (previousPoint.IsPivot && previousPoint.Distance(pivotPoint) < MIN_SEGMENT_LENGTH)
                    {
						// If the previous point was a pivot and was very close to this point,
						// which we are about to mark as a pivot, then unmark the previous point as a pivot.
						// Also need to decrement the SubStroke that it belongs to since it's not part of
						// the new SubStroke that begins at this pivot.
                        previousPoint.IsPivot = false;
						previousPoint.SubStrokeIndex = subStrokeIndex - 1;
					}
                    else
                    {
						// If we didn't have to unmark a previous pivot, then the we can increment the SubStrokeIndex.
						// If we did unmark a previous pivot, then the old count still applies and we don't need to increment.
						subStrokeIndex++;
					}
						
					pivotPoint.IsPivot = true;
					
					// A new SubStroke has begun, so the runningLength gets reset.
                    runningLength = pivotLength;
					
					firstPoint = pivotPoint;
				} 
				
				localLength = pivotLength;		// Always update the localLength, since it deals with the last three seen points.
				
				previousPoint = pivotPoint;
				pivotPoint = nextPoint;
				
				pivotPoint.SubStrokeIndex = subStrokeIndex;
			}
				
			// last point (currently referenced by pivotPoint) has to be a pivot
			pivotPoint.IsPivot = true;
			
			// Point before the final point may need to be handled specially.
			// Often mouse action will produce an unintended small segment at the end.
			// We'll want to unmark the previous point if it's also a pivot and very close to the lat point.
			// However if the previous point is the first point of the stroke, then don't unmark it, because then we'd only have one pivot.
			if (previousPoint.IsPivot &&
			    previousPoint.Distance(pivotPoint) < MIN_SEGMENT_LENGTH &&
			    previousPoint != this.pointList[0])
            {
				previousPoint.IsPivot = false;
				pivotPoint.SubStrokeIndex = subStrokeIndex - 1;
			}
			
			// Mark the stroke as analyzed so that it won't need to be analyzed again.
			this.isAnalyzed = true;
		}
	}
}
