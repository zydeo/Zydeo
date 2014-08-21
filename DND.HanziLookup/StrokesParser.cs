using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace DND.HanziLookup
{
    public class StrokesParser : LineParser
    {

        private MemoryStream[] genericByteStreams;
        private MemoryStream[] simplifiedByteStreams;
        private MemoryStream[] traditionalByteStreams;

        private BinaryWriter[] genericOutStreams;
        private BinaryWriter[] simplifiedOutStreams;
        private BinaryWriter[] traditionalOutStreams;

        // Below a couple of reusable arrays.  Allocating them once should save a little.

        // Holds the number of substrokes in the stroke for the given order index.
        // (ie int at index 0 will be the number of substrokes in the first stroke)
        private int[] subStrokesPerStroke = new int[CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT];

        // Instantiate a flat array that we can resuse to hold parsed SubStroke data.
        // This is so we don't reinstantiate a new array for each line.
        // Holds the direction and length of each SubStroke, so it needs twice as many indices as the possible number of SubStrokes.
        private double[] subStrokeDirections = new double[CharacterDescriptor.MAX_CHARACTER_SUB_STROKE_COUNT];
        private double[] subStrokeLengths = new double[CharacterDescriptor.MAX_CHARACTER_SUB_STROKE_COUNT];

        // Store patterns as instance variables so that we can reuse them and don't need to reinstantiate them for every entry.
        // linePattern identifies the unicode code point and allows us to group it apart from the SubStroke data.
        private Regex reLinePattern = new Regex(@"^([a-fA-F0-9]{4})\s+(S|T|B)\s+\|(.*)$");
        // subStrokePattern groups the direction and length of a SubStroke.
        private Regex reSubStrokePattern = new Regex(@"^\s*\((\d+(\.\d{1,10})?)\s*,\s*(\d+(\.\d{1,10})?)\)\s*$");

        /**
         * Build a new parser.
         * @param strokesIn strokes data
         * @param typeRepository the CharacterTypeRepository to get type data from
         * @throws IOException
         */
        public StrokesParser(StreamReader strokesIn)
        {
            this.initStrokes(strokesIn);
        }

        private void initStrokes(StreamReader strokesIn)
        {
            this.prepareStrokeBytes();
            this.Parse(strokesIn);
        }

        /**
         * Write the byte data in this StrokesRepository out to the given output stream.
         * Nothing should have already have been written to the stream, and it will
         * be closed once this method returns.  The data can subsequently be read
         * in using the InputStream constructor.
         * 
         * @see StrokesRepository#StrokesRepository(InputStream)
         */
        public void WriteCompiledOutput(BinaryWriter w)
        {
            byte[][] genericBytes = new byte[CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT][];
            byte[][] simplifiedBytes = new byte[CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT][];
            byte[][] traditionalBytes = new byte[CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT][];

            for (int i = 0; i < CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT; i++)
            {
                genericBytes[i] = this.genericByteStreams[i].ToArray();
                simplifiedBytes[i] = this.simplifiedByteStreams[i].ToArray();
                traditionalBytes[i] = this.traditionalByteStreams[i].ToArray();
            }

            // write out each of the data series one after the other.
            writeStrokes(genericBytes, w);
            writeStrokes(simplifiedBytes, w);
            writeStrokes(traditionalBytes, w);
        }

        private void writeStrokes(byte[][] bytesForSeries, BinaryWriter w)
        {
            for (int strokeCount = 0; strokeCount < CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT; strokeCount++)
            {
                // first write the number of bytes for this stroke count.
                // this is so when reading in we know how many bytes belong to each series.
                int bytesForStrokeCount = bytesForSeries[strokeCount].Length;
                w.Write(bytesForStrokeCount);

                // now actually write out the data
                w.Write(bytesForSeries[strokeCount]);
            }
        }

        private void prepareStrokeBytes()
        {
            this.genericByteStreams = new MemoryStream[CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT];
            this.genericOutStreams = new BinaryWriter[CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT];

            this.simplifiedByteStreams = new MemoryStream[CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT];
            this.simplifiedOutStreams = new BinaryWriter[CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT];

            this.traditionalByteStreams = new MemoryStream[CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT];
            this.traditionalOutStreams = new BinaryWriter[CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT];

            for (int i = 0; i < CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT; i++)
            {
                this.genericByteStreams[i] = new MemoryStream();
                this.genericOutStreams[i] = new BinaryWriter(this.genericByteStreams[i]);

                this.simplifiedByteStreams[i] = new MemoryStream();
                this.simplifiedOutStreams[i] = new BinaryWriter(this.simplifiedByteStreams[i]);

                this.traditionalByteStreams[i] = new MemoryStream();
                this.traditionalOutStreams[i] = new BinaryWriter(this.traditionalByteStreams[i]);
            }
        }

        ///////////////////

        /**
         * Parses a line of text.  Each line should contain the SubStroke data for a character.
         * 
         * The format of a line should be as follows:
         * 
         * Each line is the data for a single character represented by the unicode code point.
         * Strokes follow, separated by "|" characters.
         * Strokes can be divided into SubStrokes, SubStrokes are defined by (direction, length).
         * SubStrokes separated by "#" characters.
         * Direction is in radians, 0 to the right, PI/2 up, etc... length is from 0-1.
         * 
         * @see LineParser#parseLine(String)
         */
        protected override bool ParseLine(int lineNum, string line)
        {
            var match = reLinePattern.Match(line);

            bool parsedOk = true;
            int subStrokeIndex = 0;	// Need to count the total number of SubStrokes so we can write that out.
            if (match.Success)
            {
                // Separate out the unicode code point in the first group from the substroke data in the second group.
                string unicodeString = match.Groups[1].Value;
                char character = (char)Convert.ToInt32(unicodeString, 16);

                string script = match.Groups[2].Value;

                string lineRemainder = match.Groups[3].Value;

                // Strokes are separated by "|" characters, separate them.
                int strokeCount = 0;
                string[] parts = lineRemainder.Split(new char[] { '|' });
                foreach (string nextStroke in parts)
                {
                    if (strokeCount > CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT)
                    {
                        parsedOk = false;
                        break;
                    }

                    // Parse each stroke separately, keep track of SubStroke total.
                    // We need to pass the SubStroke index so that the helper parse methods know where
                    // they should write the SubStroke data in the SubStrokes data array.
                    int subStrokes = this.parseStroke(nextStroke, strokeCount, subStrokeIndex);
                    if (subStrokes > 0)
                    {
                        subStrokeIndex += subStrokes;
                    }
                    else
                    {
                        // Every stroke should have at least one SubStroke, if not the line is incorrectly formatted or something.
                        parsedOk = false;
                    }
                    ++strokeCount;
                }

                if (parsedOk)
                {
                    BinaryWriter dataOut = dataOut = this.genericOutStreams[strokeCount - 1]; ;
                    byte type = 0;
                    if (script == "T")
                    {
                        dataOut = this.traditionalOutStreams[strokeCount - 1];
                        type = 2;
                    }
                    else if (script == "S")
                    {
                        dataOut = this.simplifiedOutStreams[strokeCount - 1];
                        type = 1;
                    }

                    // Write the parsed data out to the byte array, return true if the writing was successful.
                    this.writeStrokeData(dataOut, character, type, strokeCount, subStrokeIndex);
                    return true;
                }
            }

            // Line didn't match the expected format.
            return false;
        }

        /**
         * Parse a Stroke.
         * A Stroke should be composed of one or more SubStrokes separated by "#" characters.
         * 
         * @param strokeText the text of the Stroke
         * @param strokeIndex the index of the current stroke (first stroke is stroke 0)
         * @param baseSubStrokeIndex the index of the first substroke of the substrokes in this stroke
         * @return the number of substrokes int this stroke, -1 to signal a parse problem
         */
        private int parseStroke(string strokeText, int strokeIndex, int baseSubStrokeIndex)
        {
            int subStrokeCount = 0;
            string[] parts = strokeText.Split(new char[] { '#' });
            foreach (string nextPart in parts)
            {
                // We add subStrokeCount * 2 because there are two entries for each SubStroke (direction, length)
                if (subStrokeCount >= CharacterDescriptor.MAX_CHARACTER_SUB_STROKE_COUNT ||
                   !this.parseSubStroke(nextPart, baseSubStrokeIndex + subStrokeCount))
                {
                    // If there isn't room in the array (too many substrokes), or not parsed successfully...
                    // then we return -1 to signal error.

                    return -1;
                }
                ++subStrokeCount;
            }

            // store the number of substrokes in this stroke
            this.subStrokesPerStroke[strokeIndex] = subStrokeCount;

            // SubStroke parsing was apprently successful, return the number of SubStrokes parsed.
            // The number parsed should just be the number of 
            return subStrokeCount;
        }

        /**
         * Parses a SubStroke.  Gets the direction and length, and writes them into the SubStroke data array.
         * 
         * @param subStrokeText the text of the SubStroke
         * @param subStrokeArrayIndex the index to write data into the reusable instance substroke data array.
         * @return true if parsing successful, false otherwise
         */
        private bool parseSubStroke(string subStrokeText, int subStrokeIndex)
        {
            // the pattern of a substroke (direction in radians, length 0-1)
            var match = reSubStrokePattern.Match(subStrokeText);

            if (match.Success)
            {
                double direction = double.Parse(match.Groups[1].Value);
                double length = double.Parse(match.Groups[3].Value);

                this.subStrokeDirections[subStrokeIndex] = direction;
                this.subStrokeLengths[subStrokeIndex] = length;

                return true;
            }

            return false;
        }

        /**
         * Writes the entry into the strokes byte array.
         * Entries are written one after another.  There are no delimiting tokens.
         * The format of an entry in the byte array is as follows:
         * 
         * 2 bytes for the character
         * 1 byte for the type (generic, traditional, simplified)
         * 
         * 1 byte for the number of Strokes
         * 1 byte for the number of SubStrokes
         * Because of the above, maximum number of Strokes/SubStrokes is 2^7 - 1 = 127.
         * This should definitely be enough for Strokes, probably enough for SubStrokes.
         * In any case, this limitation is less than the limitation imposed by the defined constants currently.
         * 
         * Then for each Stroke:
         * 1 byte for the number of SubStrokes in the Stroke
         * 
         * Then for each SubStroke:
         * 2 bytes for direction
         * 2 bytes for length
         * 
         * Could probably get by with 1 byte for number of Strokes and SubStrokes if needed.
         * Any change to this method will need to be matched by changes to StrokesRepository#compareToNextInStream.
         * 
         * @param character the Character that this entry is for
         * @param type the type of the Character (generic, traditiona, simplified, should be one of the constants)
         * @param strokeCount the number of Strokes in this Character entry
         * @param subStrokeCount the number of SubStrokes in this Character entry.
         */
        private void writeStrokeData(BinaryWriter dataOut, char character, int type, int strokeCount, int subStrokeCount)
        {
            // Write out the non-SubStroke data.
            dataOut.Write((short)character);
            dataOut.Write((byte)type);
            dataOut.Write((byte)strokeCount);

            int subStrokeArrayIndex = 0;
            for (int strokes = 0; strokes < strokeCount; strokes++)
            {
                int numSubStrokeForStroke = this.subStrokesPerStroke[strokes];

                //  Write out the number of SubStrokes in this Stroke.
                dataOut.Write((byte)numSubStrokeForStroke);

                for (int substrokes = 0; substrokes < numSubStrokeForStroke; substrokes++)
                {
                    StrokesIO.WriteDirection(this.subStrokeDirections[subStrokeArrayIndex], dataOut);
                    StrokesIO.WriteLength(this.subStrokeLengths[subStrokeArrayIndex], dataOut);
                    subStrokeArrayIndex++;
                }
            }
        }
    }
}

