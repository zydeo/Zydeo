using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DND.HanziLookup
{
    /**
     * A data repsitory for describing the types of Chinese Characters (ie simplified, traditional, mappings between the two, etc).
     * Generally these will only be built by CharacterTypeParsers when they are done parsing a type file.
     * 
     * @see CharacterTypeParser
     */
    public class CharacterTypeRepository
    {

        public const int GENERIC_TYPE = 0;	// Character is common to both simplified and traditional character sets.
        public const int SIMPLIFIED_TYPE = 1;	// Character is a simplified form.
        public const int TRADITIONAL_TYPE = 2;	// Character is a traditional form.
        public const int EQUIVALENT_TYPE = 3;	// Character is equivalent to another character.
        public const int NOT_FOUND = -1;

        // thinly wraps a Map that maps Characters to TypeDescriptors.
        private readonly Dictionary<char, TypeDescriptor> typeMap;

        /**
         * Instantiate a new CharacterTypeRepository using the map provided.
         * 
         * @param typeMap a Map of Characters to TypeDescriptors.
         */
        public CharacterTypeRepository(Dictionary<char, TypeDescriptor> typeMap)
        {
            this.typeMap = typeMap;
        }

        /**
         * Retrieve the TypeDescriptor associated with the given Character.
         * 
         * @param character the Character whose TypeDescriptor we want
         * @return the TypeDescriptor associated with the Character, null if none found
         */
        public TypeDescriptor Lookup(char character)
        {
            if (typeMap.ContainsKey(character))
                return typeMap[character];
            return null;
        }

        /**
         * Gets the type of the given Character.
         * If the character is considered equivalent to another character,
         * then the type of that equivalent character is returned instead.
         * Return value should be one of the defined constants.
         * 
         * @param character the Character whose type we want to know
         * @return the type of the Character, -1 if the Character wasn't found
         */
        public int GetType(char character)
        {
            TypeDescriptor typeDescriptor = Lookup(character);
            if (null != typeDescriptor)
            {
                if (typeDescriptor.Type == GENERIC_TYPE ||
                   typeDescriptor.Type == SIMPLIFIED_TYPE ||
                   typeDescriptor.Type == TRADITIONAL_TYPE)
                {
                    // Normally we can just return the type set on the TypeDescriptor...
                    return typeDescriptor.Type;
                }
                else if (typeDescriptor.Type == EQUIVALENT_TYPE)
                {
                    // except in the case of an equivalent type.
                    // In that case the type we return is actually the type of the equivalent mapped to.
                    // It's possible that if a mistake mistake in the data file could cause in infinite loop here.
                    return GetType(typeDescriptor.AltUnicode.Value);
                }
            }

            return NOT_FOUND;
        }

        /**
         * A TypeDescriptor defines a Character type and possibly its relationship to another Character.
         */
        public class TypeDescriptor
        {
            public readonly int Type;
            public readonly char Unicode;
            public readonly char? AltUnicode;

            /**
             * Instantiate a new TypeDescriptor with the given data.
             * 
             * GENERIC_TYPE means that the unicode code point is common to both simplified and traditional character sets.  altUnicode should be null.
             * SIMPLIFIED_TYPE means that the unicode code point is a simplified form of the character altUnicode.
             * TRADITIONAL_TYPE means that the unicode code point is a traditional form of the character altUnicode.
             * EQUIVALENT_TYPE means that the unicode code point is equivalent to the character altUnicode.
             * 
             * @param type the type of the Character / relationship 
             * @param character the character described by this TypeDescriptor
             * @param altCharacter another character that the main character shares a relationship to, can be null
             */
            public TypeDescriptor(int type, char character, char? altCharacter)
            {
                Type = type;
                Unicode = character;
                AltUnicode = altCharacter;
            }
        }
    }
}

