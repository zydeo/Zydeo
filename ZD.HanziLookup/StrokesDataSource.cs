using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZD.HanziLookup
{
    /// A StrokesDataSource is lookup engine's mechanism for retrieving stroke data
    /// from an arbitrary InputStream of stroke byte data.  Where the data comes
    /// from is abstracted into the StrokesStreamProvider implementation provided.
    /// 
    /// Once constructed a StrokesDataSource instance can return StrokesDataScanners.
    /// The scanner is a disposable Object that is used once for each lookup.  It's
    /// job is to successively serve up CharacterDescriptors as read from the stream.
    /// 
    /// This replaces the StrokesRepository in previous versions.  The StrokesRepository
    /// always held all of the stroke data in memory.  This abstraction gives the ability
    /// to decide if the data stream comes from an in-memory source or from elsewhere.
    public class StrokesDataSource
    {
        // Arrays contain the byte indexes in the stream where the characters with each number
        // of strokes begins.  i.e. traditional characters with 8 strokes begin at byte index
        // traditionalPositions[8 - 1] in the strokes stream.  We index the positions once
        // on instantiation and then can use the indices each subsequent lookup to speed things up.
        private long[] genericPositions = new long[CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT];
        private long[] simplifiedPositions = new long[CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT];
        private long[] traditionalPositions = new long[CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT];

        private readonly BinaryReader dataStream;

        /**
         * Create a new StrokesDataSource whose strokes data is derived from the
         * InputStream returned by the given StrokesStreamProvider.
         * 
         * @param streamProvider
         * @throws IOException on an exception reading from the strokes data stream
         */
        public StrokesDataSource(BinaryReader dataStream)
        {
            this.dataStream = dataStream;
            this.indexPositions();
        }

        public void Reset()
        {
            dataStream.BaseStream.Position = 0;
        }

        /**
         * Index and store in the instance the indexes in the provided InputStream where
         * characters with various stroke counts begin.
         * @throws IOException
         */
        private void indexPositions()
        {
            long bytePosition = 0;

            // This assumes the byte stream is in the correct form (generic characters, then
            // simplified, then traditional, with characters grouped in each category by their
            // stroke count.
            bytePosition = loadPositions(this.genericPositions, dataStream, bytePosition);
            bytePosition = loadPositions(this.simplifiedPositions, dataStream, bytePosition);
            bytePosition = loadPositions(this.traditionalPositions, dataStream, bytePosition);
        }

        private long loadPositions(long[] positions, BinaryReader inStream, long bytePosition)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = bytePosition;

                // The first byte in each character grouping tells how many bytes are in that group.
                // We use this to jump to the next grouping.
                int bytesForSeries = inStream.ReadInt32();
                bytePosition += bytesForSeries + 4;

                // Don't care about the actual character stroke data now, so just jump over it.
                inStream.BaseStream.Position += bytesForSeries;
            }

            return bytePosition;
        }

        /**
         * Obtain a StrokesDataScanner instance.
         * The instance can be tuned to return data faster if it can filter
         * out some of the data according to the parameters.
         * 
         * @param searchTraditional true if traditional characters are checked
         * @param searchSimplified true if simplified characters are checked
         * @param minStrokes the minimum number of strokes in a character we should check
         * @param maxStrokes the maximum number of strokes in a character we should check
         * @return a scanner
         */
        public StrokesDataScanner GetStrokesScanner(bool searchTraditional, bool searchSimplified,
            int minStrokes, int maxStrokes)
        {
            // bounds checking shouldn't be necessary, but just in case
            minStrokes = Math.Max(1, minStrokes);
            maxStrokes = Math.Min(CharacterDescriptor.MAX_CHARACTER_STROKE_COUNT, maxStrokes);

            return new StrokesDataScanner(searchTraditional, searchSimplified, minStrokes, maxStrokes,
                genericPositions, simplifiedPositions, traditionalPositions,
                dataStream);
        }
    }
}

