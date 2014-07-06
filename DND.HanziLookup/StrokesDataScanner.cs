using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DND.HanziLookup
{
    /// A StrokesDataScanner is a disposable, stateful Object that can successively
    /// serve up each character in a StrokesDataSource.
    /// 
    /// The implementation of this Object is relatively delicate as it is very tied
    /// to the exact byte format expected from a stroke data byte stream.
    public class StrokesDataScanner
    {
        private BinaryReader strokeDataStream;

        private IEnumerator<long> positionsIter;

        private long position;
        private long endOfStrokeCount;

        // If true then we've reached the end of searching for one of the types of characters.
        // i.e. traditional, and next request we jump to the point in the stream where we
        // can start searching for characters of the next type
        private bool skipToNextTypePosition;

        // If true then we've reached then end of searching the characters with a particular
        // stroke count within one of the character types.  Next request we need to prime
        // for the next stroke count.
        private bool loadNextStrokeCount;

        private int strokeCount;
        private int minStrokes;
        private int maxStrokes;

        /**
            * Create a new StrokesDataScanner for performing a lookup match.
            * 
            * @param searchTraditional true if traditional characters are checked
            * @param searchSimplified true if simplified characters are checked
            * @param minStrokes the minimum number of strokes in a character we should check
            * @param maxStrokes the maximum number of strokes in a character we should check
            */
        public StrokesDataScanner(bool searchTraditional, bool searchSimplified, int minStrokes, int maxStrokes,
            long[] genericPositions, long[] simplifiedPositions, long[] traditionalPositions,
            BinaryReader strokeDataStream)
        {
            int strokeIndex = minStrokes - 1;

            // Make a List of the indices in the stream where we need to start searching.
            List<long> positions = new List<long>();
            positions.Add(genericPositions[strokeIndex]);
            if (searchSimplified) positions.Add(simplifiedPositions[strokeIndex]);
            if (searchTraditional) positions.Add(traditionalPositions[strokeIndex]);

            this.strokeDataStream = strokeDataStream;
            this.positionsIter = positions.GetEnumerator();

            this.position = 0;
            this.skipToNextTypePosition = true;
            this.loadNextStrokeCount = true;

            this.strokeCount = minStrokes;
            this.minStrokes = minStrokes;
            this.maxStrokes = maxStrokes;
        }

        /**
            * Load the next character data in the data stream into the given CharacterDescriptor Object.
            * We load into the given rather than instantiating and returning our own instance because
            * potentially there may be thousands of calls to this method per input lookup.  No sense
            * in creating all that heap action if it's not necessary since we can reuse a CharacterDescriptor
            * instance.
            * 
            * @param descriptor the descriptor to read stroke data into
            * @return true if another character's data was loaded, false if there aren't any more characters
            * @throws IOException
            */
        public bool LoadNextCharacterStrokeData(CharacterDescriptor descriptor)
        {
            if (this.skipToNextTypePosition)
            {
                // Finished one of the character types (i.e. traditional.)
                // We now want to skip to the position for the next type.

                if (!this.positionsIter.MoveNext())
                {
                    // No more character types.  We're done.
                    return false;
                }

                // Get the position of the next character type and skip to it.
                long nextPosition = positionsIter.Current;
                long skipBytes = nextPosition - this.position;
                strokeDataStream.BaseStream.Position += skipBytes;

                this.position = nextPosition;
                this.skipToNextTypePosition = false;
            }

            if (this.loadNextStrokeCount)
            {
                // We've finished reading all the characters for a particular stroke count
                // within a character type.  Prime for reading the next stroke count.

                this.position += 4;	// We're about to read an int to get the size of the next stroke count group,
                // an int is 4 bytes, so advance the position accordingly.

                // Save in the instance the position where the characters for the new stroke count end.
                this.endOfStrokeCount = this.position + this.strokeDataStream.ReadInt32();
                this.loadNextStrokeCount = false;
            }

            if (this.position < this.endOfStrokeCount)
            {
                // If there are more characters to read for a stroke count, then load the next character's data.
                loadNextCharacterDataFromStream(descriptor);

                // Advance the position by the number of bytes read for the character
                this.position += 4	// 2 bytes for the actual unicode character + 1 byte for the type of character + 1 byte for the number of strokes 
                                + descriptor.StrokeCount			// 1 byte for each stroke that tells the number of substrokes in the stroke
                                + (4 * descriptor.SubStrokeCount); 	// 4 bytes for each sub stroke (2 for direction, 2 for length)
            }

            if (this.position == this.endOfStrokeCount)
            {
                // We've reached the characters for a particular stroke count.

                this.loadNextStrokeCount = true;

                if (this.strokeCount == this.maxStrokes)
                {
                    // We've also reached the end of all the characters that we're
                    // going to check for this character type, so on the next request
                    // we'll skip to the next character type.
                    this.skipToNextTypePosition = true;
                    this.strokeCount = this.minStrokes;	// reset

                }
                else this.strokeCount++;
            }

            return true;
        }

        /**
            * Helper method loads the next character data into the given CharacterDescriptor
            * from the given DataInputStream as formatted by a strokes data file.
            * 
            * @param loadInto the CharacterDescriptor instance to load data into
            * @param dataStream the stream to load data from
            * @throws IOException
            */
        private void loadNextCharacterDataFromStream(CharacterDescriptor loadInto)
        {
            char character = (char)strokeDataStream.ReadInt16(); // character is the first two bytes
            int characterType = (int)strokeDataStream.ReadByte(); // character type is the first byte
            int strokeCount = (int)strokeDataStream.ReadByte(); // number of strokes is next
            // the number of strokes is deducible from
            // where we are in the stream, but the stream
            // wasn't originally ordered by stroke count...

            int subStrokeCount = 0;

            double[] directions = loadInto.Directions;
            double[] lengths = loadInto.Lengths;

            // format of substroke data is [sub stroke count per stroke]([direction][length])+
            // there will be a direction,length pair for each of the substrokes
            for (int i = 0; i < strokeCount; i++)
            {
                // for each stroke

                // read the number of sub strokes in the stroke
                int numSubStrokesInStroke = (int)strokeDataStream.ReadByte();

                for (int j = 0; j < numSubStrokesInStroke; j++)
                {
                    // for each sub stroke read out the direction and length

                    double direction = StrokesIO.ReadDirection(strokeDataStream);
                    double length = StrokesIO.ReadLength(strokeDataStream);

                    directions[subStrokeCount] = direction;
                    lengths[subStrokeCount] = length;

                    subStrokeCount++;
                }
            }

            loadInto.Character = character;
            loadInto.CharacterType = characterType;
            loadInto.StrokeCount = strokeCount;
            loadInto.SubStrokeCount = subStrokeCount;
        }
    }
}

