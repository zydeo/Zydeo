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
        * @author Jordan Kiang
        * (c) 2005
        * 
        * CharacterTypeParser parses an input stream (file) defining types and relationships between Chinese characters.
        * The result of parsing is a Map that maps from a Character to a CharacterTypeRepository.TypeDescriptor.
        * 
        * After parsing, buildCharacterTypeRepository can be called.
        * It instantiates and returns a new CharacterTypeRepository using the parsed data.
        * Once the CharacterTypeRepository has been retrieved, the CharacterTypeParser is no longer needed.
        * 
        * @see kiang.io.LineParser
        * @see hanzilookup.data.CharacterTypeRepository
        */
    public class CharacterTypeParser : LineParser
    {

        // Data stuffed into the typeMap, can be retreived via getTypeMap after parsing.
        private Dictionary<char, CharacterTypeRepository.TypeDescriptor> typeMap = new Dictionary<char, CharacterTypeRepository.TypeDescriptor>();

        // Regular expression line pattern we expect the line to conform to.  Used to verify the format of the line and to retrieve groups.
        // We store this as an instance variable so we don't instantiate a new Pattern for each parsed line.
        private Regex reLinePattern = new Regex("^([a-f0-9]{4})\\s*\\|\\s*(\\d)(\\s*\\|\\s*([a-f0-9]{4}))?\\s*$");

        /**
            * Builds and parses type information by reading from the given typeStream.
            * The given input stream does not need to be buffered, as it will be wrapped
            * in a BufferedInputStream in the super class parse implementation.
            * Note that this the stream is closed once parsing is finished.
            * 
            * @param typeStreamIn the InputStream to read the type data from
            * 
            * @see LineParser#parse(InputStream)
            */
        public CharacterTypeParser(StreamReader typeStreamIn)
        {
            this.Parse(typeStreamIn);
        }

        /**
            * Builds a CharacterTypeRepository using the map that was parsed.
            * Calling this method before parsing takes place, or after a failed parse will give undefined results.
            * 
            * @return a new CharacterTypeRepository
            */
        public CharacterTypeRepository BuildCharacterTypeRepository()
        {
            CharacterTypeRepository typeRepository = new CharacterTypeRepository(this.typeMap);
            return typeRepository;
        }

        ////////////////////

        /**
            * Parses a line of type data.
            * Each line should correspond to one CharacterTypeRepository.TypeDescriptor.
            * If the parsing is successful, the map will be updated with the newly parsed TypeDescriptor.
            * 
            * Each line of the input file should define one type/relationship.  The format of a line is:
            * [unicode] | [type] (| [altunicode])?
            * 
            * [unicode] and [altunicode] should be unicode code points.
            * 
            * [type] should be one of the following:
            * [type] 0 indicates that the unicode is generic to both the simplified and traditional character sets.
            * example: 4e00 | 0
            * 
            * [type] 1 indicates that the [unicode] on the left is a simplified form of the [altunicode] traditional form on the right.
            * example: 6c49 | 1 | 6f22
            *
            * [type] 2 indicates that the [unicode] on the left is a traditional form of the [altunicode] simplified form on the right.
            * example: 6f22 | 2 | 6c49
            *
            * [type] 3 indicates that the unicode is an equivalent form to the [altunicode] on the right.
            * example: 8aac | 3 | 8aaa
            * 
            * @param line the line to parse
            * @param lineNum the line number
            * 
            * @return true if parsing successful, false otherwise
            * 
            * @see LineParser#parseLine(String)
            */
        protected override bool ParseLine(int lineNum, String line)
        {
            bool parseSuccessful = false;

            var match = reLinePattern.Match(line);
            if (match.Success)
            {
                string unicodeString = match.Groups[1].Value; // unicode code point occupies the first group
                string typeString = match.Groups[2].Value;	 // type occupies the second group

                // Since the strings matched the pattern, we don't have to worry about NumberFormatExceptions.
                char unicode = (char)Convert.ToInt32(unicodeString, 16);	// parses the 4 character code point string to a Character
                int type = int.Parse(typeString);
                char alternateUnicode = (char)0;
                bool hasAlternateUnicode =false;

                if (CharacterTypeRepository.GENERIC_TYPE == type)
                {
                    // Don't need to do anything, there is no alternateUnicode for a unified type.
                    parseSuccessful = true;
                }
                else if (CharacterTypeRepository.SIMPLIFIED_TYPE == type ||
                        CharacterTypeRepository.TRADITIONAL_TYPE == type ||
                        CharacterTypeRepository.EQUIVALENT_TYPE == type)
                {
                    // We do the same thing for the three other types:
                    // We need to additionally read in the alternate unicode code point that defines the relationship.

                    string altUnicodeString = match.Groups[4].Value;
                    if (null != altUnicodeString)
                    {
                        alternateUnicode = (char)Convert.ToInt32(altUnicodeString, 16);
                        hasAlternateUnicode = true;
                        parseSuccessful = true;
                    }
                }

                if (parseSuccessful)
                {
                    // If parsing was successful, we can use the parsed data to instantiate a new TypeDescriptor.
                    char? altUni = null;
                    if (hasAlternateUnicode) altUni = alternateUnicode;
                    CharacterTypeRepository.TypeDescriptor charType = new CharacterTypeRepository.TypeDescriptor(type, unicode, altUni);
                    typeMap[unicode] = charType;
                    return true;
                }
            }

            // Line wasn't of the correct form and/or wasn't parsed correctly.
            return false;
        }
    }
}

