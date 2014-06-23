using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DND.HanziLookup
{
    /**
     * A single character can have several representations in the strokes data.
     * (say because there are multiple acceptable stroke orderings that we want to support).
     * But we want to be able to compute the closest X matches to a character
     * without having duplicates however (since two representations of the same character
     * will each have scores computed independently).  CharacterMatchCollectors
     * wrap the priority queue of results and make sure that only the particular match for
     * a character with the highest score is kept.
     */
    internal class CharacterMatchCollector
    {
        // a map of Characters to the current CharacterMatch
        private Dictionary<char, CharacterMatch> matchMap = new Dictionary<char, CharacterMatch>();

        private List<CharacterMatch> orderedMatches = new List<CharacterMatch>();

        private readonly int maxSize;

        public CharacterMatchCollector(int maxSize)
        {
            this.maxSize = maxSize;
        }

        /**
         * Add the given CharacterMatch to this collector.
         * The guts of this method will handle removal of duplicates and will throw out low scoring matches.
         * 
         * @param match the match to add
         * @return true if the match if the top matches were changed, false if already at maxSize and the given match had lowest score
         */
        public bool AddMatch(CharacterMatch match)
        {
            // First check the matchMap to see if there is already a CharacterMatch for the relevant Character.
            if (matchMap.ContainsKey(match.Character))
            {
                CharacterMatch existingMatch = matchMap[match.Character];
                if (match.Score > existingMatch.Score)
                {
                    orderedMatches.Remove(existingMatch);
                    matchMap[match.Character] = match;
                    orderedMatches.Add(match);
                    orderedMatches.Sort((x, y) => y.Score.CompareTo(x.Score));
                }
                return false;
            }
            matchMap[match.Character] = match;
            orderedMatches.Add(match);
            orderedMatches.Sort((x, y) => y.Score.CompareTo(x.Score));

            if (orderedMatches.Count <= maxSize)
                return false;

            CharacterMatch toRemove = orderedMatches[orderedMatches.Count - 1];
            orderedMatches.RemoveAt(orderedMatches.Count - 1);
            matchMap.Remove(toRemove.Character);
            return true;
        }

        /**
         * Get the set of top matches.  This should only be called once all calls to addMatch have already ocurred.
         * @return
         */
        public char[] GetMatches()
        {
            char[] res = new char[orderedMatches.Count];
            for (int i = 0; i != orderedMatches.Count; ++i) res[i] = orderedMatches[i].Character;
            return res;
        }
    }
}
