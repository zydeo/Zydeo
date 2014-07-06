using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.HanziLookup
{
	public class CharacterMatch
    {
		public readonly char Character;
		public readonly double Score;
		
		public CharacterMatch(char character, double score)
        {
			Character = character;
			Score = score;
		}
	}
}
