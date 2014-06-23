using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DND.HanziLookup
{
    /// <summary>
    /// A CharacterDescriptor is data holder for storing all the data
    /// needed to compare to characters for a match.
    /// 
    /// Most importantly it has the directions and lengths
    /// </summary>
    public class CharacterDescriptor
    {
        // Constants for the total maximum number of strokes/substrokes allowed in a character.
        // We put upper bounds just so we can allocate a reusable matrices and avoid having allocate 
        // new arrays for every single character.  These constants can easily be increased if needed.
        public const int MAX_CHARACTER_STROKE_COUNT = 48;
	    public const int MAX_CHARACTER_SUB_STROKE_COUNT = 64;
	
	    // The actual Character.
	    public char Character;
	
	    // one of CharacterTypeRepository types (traditional, simplified, etc).
	    public int CharacterType;
	
	    public int StrokeCount;	// number of strokes
        public int SubStrokeCount; // number of "substrokes"
	
	    // the directions and lengths of each substroke.
	    // indexed by substroke index - 1
	    private double[] directions = new double[MAX_CHARACTER_SUB_STROKE_COUNT];
	    private double[] lengths	= new double[MAX_CHARACTER_SUB_STROKE_COUNT];
	
        public double[] Directions
        {
            get { return directions; }
        }

        public double[] Lengths
        {
            get { return lengths; }
        }
    }
}
