using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using DND.Common;

namespace DND.CedictEngine
{
    /// <summary>
    /// Dictionary engine: loads index from file, provides thread-safe lookup functionality.
    /// </summary>
    public class DictEngine : ICedictEngine
    {
        /// <summary>
        /// Name of binary dictionary file.
        /// </summary>
        private readonly string dictFileName;

        /// <summary>
        /// Index: loaded from dictionary file when object is created.
        /// </summary>
        private readonly Index index;

        /// <summary>
        /// Ctor: initialize from binary dictionary file.
        /// </summary>
        public DictEngine(string dictFileName)
        {
            this.dictFileName = dictFileName;
            using (BinReader br = new BinReader(dictFileName))
            {
                int idxPos = br.ReadInt();
                br.Position = idxPos;
                index = new Index(br);
            }
        }

        /// <summary>
        /// Retrieves hanzi lookup candidates, verifies actual presence of search expression in headword.
        /// </summary>
        List<CedictResult> doLoadVerifyHanzi(IEnumerable<int> poss, string query, SearchScript script)
        {
            List<CedictResult> resList = new List<CedictResult>();
            // Yes, we only open our file on-demand
            // But we do this within each lookup's scope, so lookup stays thread-safe
            using (BinReader br = new BinReader(dictFileName))
            {
                // Look at each entry: load, verify, keep or drop
                foreach (int pos in poss)
                {
                    // Load up entry from file
                    br.Position = pos;
                    CedictEntry entry = new CedictEntry(br);
                    // Figure out position/length of query string in simplified and traditional headwords
                    int hiliteStart = -1;
                    int hiliteLength = 0;
                    hiliteStart = entry.ChSimpl.IndexOf(query);
                    if (hiliteStart != -1) hiliteLength = query.Length;
                    // If not found in simplified, check in traditional
                    if (hiliteLength == 0)
                    {
                        hiliteStart = entry.ChTrad.IndexOf(query);
                        if (hiliteStart != -1) hiliteLength = query.Length;
                    }
                    // Entry is a keeper if either source or target headword contains query
                    if (hiliteLength != 0)
                    {
                        // TO-DO: indicate wrong script in result
                        CedictResult res = new CedictResult(CedictResult.SimpTradWarning.None,
                            entry,
                            hiliteStart, hiliteLength);
                        resList.Add(res);
                    }
                }
            }
            return resList;
        }

        /// <summary>
        /// Compares lookup results after hanzi lookup for sorted presentation.
        /// </summary>
        private static int hrComp(CedictResult a, CedictResult b)
        {
            // Shorter entry comes first
            int lengthCmp = a.Entry.ChSimpl.Length.CompareTo(b.Entry.ChSimpl.Length);
            if (lengthCmp != 0) return lengthCmp;
            // Between equally long headwords where match starts sooner comes first
            int startCmp = a.HanziHiliteStart.CompareTo(b.HanziHiliteStart);
            if (startCmp != 0) return startCmp;
            // Order equally long entries by pinyin lexicographical order
            return a.Entry.PinyinCompare(b.Entry);
        }

        /// <summary>
        /// Retrieves entries (sorted) whose headword contains hanzi from search expression.
        /// </summary>
        List<CedictResult> doHanziLookupHead(string query, SearchScript script)
        {
            // Get every character once - we ignore repeats
            HashSet<char> queryChars = new HashSet<char>();
            foreach (char c in query) queryChars.Add(c);
            // Map from keys (entry positions) to # of query chars found in entry
            Dictionary<int, int> posToCountSimp = new Dictionary<int, int>();
            Dictionary<int, int> posToCountTrad = new Dictionary<int, int>();
            // Look at each character's entry position vector, increment counts
            foreach (char c in queryChars)
            {
                if (!index.IdeoIndex.ContainsKey(c)) continue;
                IdeoIndexItem iii = index.IdeoIndex[c];
                // Count separately for simplified and traditional
                foreach (int pos in iii.EntriesHeadwordSimp)
                {
                    if (posToCountSimp.ContainsKey(pos)) ++posToCountSimp[pos];
                    else posToCountSimp[pos] = 1;
                }
                foreach (int pos in iii.EntriesHeadwordTrad)
                {
                    if (posToCountTrad.ContainsKey(pos)) ++posToCountTrad[pos];
                    else posToCountTrad[pos] = 1;
                }
            }
            // Get positions that contain all chars from query
            HashSet<int> matchingPositions = new HashSet<int>();
            foreach (var x in posToCountSimp) if (x.Value == queryChars.Count) matchingPositions.Add(x.Key);
            foreach (var x in posToCountTrad) if (x.Value == queryChars.Count) matchingPositions.Add(x.Key);
            // Now fetch and verify results
            List<CedictResult> res = doLoadVerifyHanzi(matchingPositions, query, script);
            // Sort hanzi results
            res.Sort((a, b) => hrComp(a, b));
            // Done.
            return res;
        }

        /// <summary>
        /// Returns true if search string has ideographic characters, false otherwise.
        /// </summary>
        private static bool hasIdeo(string str)
        {
            foreach (char c in str)
            {
                // VERY rough "definition" but if works for out purpose
                int cval = (int)c;
                if (cval >= 0x2e80) return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves entries for a Chinese search expression (pinyin vs. hanzi auto-detected)
        /// </summary>
        private List<CedictResult> doChineseLookup(string query, SearchScript script)
        {
            List<CedictResult> res = new List<CedictResult>();
            // If query string has ideographic characters, do hanzi looup
            if (hasIdeo(query)) res = doHanziLookupHead(query, script);
            // Otherwise, do pinyin lookup
            else
            {
                // TO-DO
            }
            // Done
            return res;
        }

        /// <summary>
        /// Retrieves matching entries for a target-language search expression.
        /// </summary>
        private List<CedictResult> doTargetLookup(string query)
        {
            // TO-DO
            return new List<CedictResult>();
        }

        /// <summary>
        /// Find entries that match the search expression.
        /// </summary>
        /// <param name="query">The query string, as entered by the user.</param>
        /// <param name="script">For hanzi lookup: simplified, traditional or both.</param>
        /// <param name="lang">Chinese or target language (English).</param>
        /// <returns>The lookup result.</returns>
        public CedictLookupResult Lookup(string query, SearchScript script, SearchLang lang)
        {
            List<CedictResult> res = new List<CedictResult>();
            
            // Try first in language requested by user
            // If no results that way, try in opposite language
            // Override if lookup in opposite language is successful
            if (lang == SearchLang.Chinese)
            {
                res = doChineseLookup(query, script);
                // We got fish
                if (res.Count > 0)
                    return new CedictLookupResult(new ReadOnlyCollection<CedictResult>(res), lang);
                // OK, try opposite (target)
                res = doTargetLookup(query);
                // We got fish: override
                if (res.Count > 0)
                    return new CedictLookupResult(new ReadOnlyCollection<CedictResult>(res), SearchLang.Target);
            }
            else
            {
                res = doTargetLookup(query);
                // We got fish
                if (res.Count > 0)
                    return new CedictLookupResult(new ReadOnlyCollection<CedictResult>(res), lang);
                // OK, try opposite (target)
                res = doChineseLookup(query, script);
                // We got fish: override
                if (res.Count > 0)
                    return new CedictLookupResult(new ReadOnlyCollection<CedictResult>(res), SearchLang.Chinese);
            }
            // Sorry, no results, no override
            return new CedictLookupResult(new ReadOnlyCollection<CedictResult>(res), lang);
        }
    }
}
