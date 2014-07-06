using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.HanziLookup
{
    /// <summary>
    /// Represents the handwriting input.
    /// </summary>
    /// <remarks>
    /// It stores its data in WrittenStroke objects, which themselves are composed of WrittenPoints.
    /// It can analyze and interpret itself and build a CharacterDescriptor object which can
    /// be compared against the StrokesRepository.
    /// Once input is completed, these objects are analyzed and there useful data is distilled
    /// into CharacterInputDescriptor objects.
    /// </remarks>
    public class WrittenCharacter
    {
	    // Edges to keep track of the coordinates of the bounding box of the character.
	    // Used to normalize lengths so that a character can be written in any size as long as proportional.
	    public double LeftX;
        public double RightX;
        public double TopY;
        public double BottomY;
		
	    // List of WrittenStrokes.
	    private List<WrittenStroke> strokeList = new List<WrittenStroke>();
	
	    public WrittenCharacter()
        {
		    resetEdges();
	    }
	
	    public List<WrittenStroke> StrokeList
        {
            get { return strokeList; }
	    }
	
	    public void AddStroke(WrittenStroke stroke)
        {
		    strokeList.Add(stroke);
	    }
	
	    public void Clear()
        {
		    strokeList.Clear();
		    resetEdges();
	    }
	
	    /// <summary>
        /// Resets the edges.  Any new point will be more/less than these reset values.
        /// </summary>
	    private void resetEdges()
        {
		    this.LeftX = Double.PositiveInfinity;
		    this.RightX = Double.NegativeInfinity;
		    this.TopY = Double.PositiveInfinity;
		    this.BottomY = Double.NegativeInfinity;
	    }
	
	    private void analyzeAndMark()
        {
            foreach (WrittenStroke nextStroke in strokeList)
            {
			    if (!nextStroke.IsAnalyzed)
                {
				    // If the written character has not been analyzed yet, we need to analyze it.
				    nextStroke.AnalyzeAndMark();
			    }
		    }
	    }
	
        /// <summary>
	    /// Translate this WrittenCharacter into a CharacterDescriptor.
	    /// The written data is distilled into SubStrokes in the CharacterDescriptor.
	    /// The CharacterDescriptor can be used against StrokesRepository to find the closest matches.
	    /// 
	    /// @return a CharacterDescriptor translated from this WrittenCharacter.
        /// </summary>
	    public CharacterDescriptor BuildCharacterDescriptor()
        {
		    int strokeCount = this.strokeList.Count;
		    int subStrokeCount = 0;
		
		    CharacterDescriptor descriptor = new CharacterDescriptor();

            double[] directions = descriptor.Directions;
		    double[] lengths = descriptor.Lengths;
		
		    // Iterate over the WrittenStrokes, and translate them into CharacterDescriptor.SubStrokes.
		    // Add all of the CharacterDescriptor.SubStrokes to the version.
		    // When we run out of substroke positions we truncate all the remaining stroke and substroke information.
            foreach (WrittenStroke nextStroke in strokeList)
            {
			    // Add each substroke's direction and length to the arrays.
			    // All substrokes are lumped sequentially.  What strokes they
			    // were a part of is not factored into the algorithm.
			    // Don't run off the end of the array, if we do we just truncate.
			    var subStrokes = nextStroke.GetSubStrokes(RightX - LeftX, BottomY - TopY);
                foreach (var subStroke in subStrokes)
                {
				    directions[subStrokeCount] = subStroke.Direction;
				    lengths[subStrokeCount] = subStroke.Length;
                    ++subStrokeCount;
			    }
		    }
		
		    descriptor.StrokeCount = strokeCount;
		    descriptor.SubStrokeCount = subStrokeCount;
		
		    return descriptor;
	    }
    }
}
