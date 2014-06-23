using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace DND.HanziLookup
{
    /**
     * LineParser defines some common behavior for some parsers that parse data files line by line.
     * Subclasses will need to implement a parseLine method that handles each individual line of text.
	     * Reads lines successively from the given stream and passes each line to a line parsing method.
     */
    abstract public class LineParser
    {
        public void Parse(StreamReader sr)
        {
		    int lineNum = 0;
            string line;
            while ((line = sr.ReadLine()) != null)
		    {
			    if(ShouldParseLine(lineNum, line))
                {
				    // Pass each non-empty, non comment line to the parsing method.
				    if(!this.ParseLine(lineNum, line))
                    {
					    this.LineError(lineNum, line);
				    }
			    }
			
			    lineNum++;
		    }
        }

        /**
	     * Subclasses implement this method to handle each line of text.
	     * 
	     * @param lineNum the lineNumber
	     * @param line the line of text to parse
	     * @return true if the parsing was successful, false otherwise
	     */
	    abstract protected bool ParseLine(int lineNum, string line);

	    /**
	     * Invoked when there is line is unsuccessfully parsed.
	     * Can be overidden for custom behavior.
	     * 
	     * @param lineNum the line number on which the error occurred
	     * @param line the contents of the line
	     */
	    protected void LineError(int lineNum, string line)
        {
            Console.Error.WriteLine("Error parsing line " + lineNum.ToString() + ": " + line.ToString());
	    }
	
	    /**
	     * Default behavior is to parse lines that are not comments or empty.
	     * Can be overidden for custom behavior.
	     * 
	     * @param lineNum the lineNumber
	     * @param line the line to test
	     * @return true if the line should be parsed, false if it should be ignored
	     */
	    protected bool ShouldParseLine(int lineNum, string line)
        {
	        return !this.IsLineEmpty(line) && !this.IsLineComment(line);
	    }

        private Regex reComment1 = new Regex("^\\s*//.*");
        private Regex reComment2 = new Regex("^\\s*#.*");
        private Regex reEmpty = new Regex("^\\s*$");
	
	    /**
	     * @param line the line to test
	     * @return true if the line is a comment, can be overridden for customized behavior
	     */
	    protected bool IsLineComment(string line)
        {
            // line is a comment if the first non-whitespace is // or #
            if (reComment1.Match(line).Success) return true;
            if (reComment2.Match(line).Success) return true;
            return false;
	    }
	
	    /**
	     * @param line the line to test
	     * @return true if the line is empty, false otherwise
	     */
	    protected bool IsLineEmpty(string line)
        {
	        // line is empty if length 0 or all whitespace
            return reEmpty.Match(line).Success;
	    }
    }
}
