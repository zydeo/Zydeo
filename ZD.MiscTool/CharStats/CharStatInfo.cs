using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.MiscTool
{
    /// <summary>
    /// Represents a character's function as part of the traditional or simplified set, or both.
    /// </summary>
    internal enum SimpTradType
    {
        /// <summary>
        /// Character is either generic (simp = trad), or has a function
        /// both as a simplified and as a traditional form.
        /// </summary>
        Simp,
        /// <summary>
        /// Character is only traditional.
        /// </summary>
        Trad,
        /// <summary>
        /// Character is only simplified.
        /// </summary>
        Both,
        /// <summary>
        /// No character type information in HanziLookup.
        /// </summary>
        NoInfo,
    }

    /// <summary>
    /// Information gathered from strokes and dictionary files about a single character.
    /// </summary>
    internal class CharStatInfo
    {
        /// <summary>
        /// The character's simplified/traditional function, from HanziLookup's types file.
        /// </summary>
        public SimpTradType HLType = SimpTradType.NoInfo;

        /// <summary>
        /// True if character occurs in any headword in the dictionary file.
        /// </summary>
        public bool InDict
        {
            get { return DictSimpCount + DictTradCount > 0; }
        }

        /// <summary>
        /// True if character has strokes info in HanziLookup.
        /// </summary>
        public bool InStrokes = false;

        /// <summary>
        /// <para>True if...</para>
        /// <para>HanziLookup thinks char is only simplified, but it occurs in traditional lemma, OR</para>
        /// <para>HanziLookup thinks char is only traditional, but it occurs in simplified lemma, OR</para>
        /// </summary>
        public bool SimpTradMismatch = false;

        /// <summary>
        /// The number of headwords in which the character occurs in the simplified lemma.
        /// </summary>
        public int DictSimpCount = 0;

        /// <summary>
        /// The number of headwords in which the character occurs in the traditional lemma.
        /// </summary>
        public int DictTradCount = 0;
    }
}
