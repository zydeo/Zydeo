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
                // If there's a hanzi that's not in index, we'll sure have not hits!
                if (!index.IdeoIndex.ContainsKey(c))
                    return new List<CedictResult>();
                
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
        /// Retrieves pinyin lookup candidates, verifies actual presence of search expression in headword.
        /// </summary>
        List<CedictResult> doLoadVerifyPinyin(IEnumerable<int> poss, List<CedictPinyinSyllable> sylls)
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

                    // Find query syllables in entry
                    int syllStart = -1;
                    for (int i = 0; i <= entry.PinyinCount - sylls.Count; ++i)
                    {
                        int j;
                        for (j = 0; j != sylls.Count; ++j)
                        {
                            CedictPinyinSyllable syllEntry = entry.GetPinyinAt(i + j);
                            CedictPinyinSyllable syllQuery = sylls[j];
                            if (syllEntry.Text != syllQuery.Text) break;
                            if (syllQuery.Tone != -1 && syllEntry.Tone != syllQuery.Tone) break;
                        }
                        if (j == sylls.Count)
                        {
                            syllStart = i;
                            break;
                        }
                    }
                    // Entry is a keeper if query syllables found
                    if (syllStart == -1) continue;
                    // Keeper!
                    CedictResult res = new CedictResult(entry, syllStart, sylls.Count);
                    resList.Add(res);
                }
            }
            return resList;
        }

        /// <summary>
        /// Compares lookup results after pinyin lookup for sorted presentation.
        /// </summary>
        private static int pyComp(CedictResult a, CedictResult b)
        {
            // Shorter entry comes first
            int lengthCmp = a.Entry.PinyinCount.CompareTo(b.Entry.PinyinCount);
            if (lengthCmp != 0) return lengthCmp;
            // Between equally long headwords where match starts sooner comes first
            int startCmp = a.PinyinHiliteStart.CompareTo(b.PinyinHiliteStart);
            if (startCmp != 0) return startCmp;
            // Order equally long entries by pinyin lexicographical order
            return a.Entry.PinyinCompare(b.Entry);
        }

        /// <summary>
        /// Retrieves entries (sorted) whose headword contains pinyin from search expression.
        /// </summary>
        List<CedictResult> doPinyinLookupHead(List<CedictPinyinSyllable> sylls)
        {
            // Get every syllable once - we ignore repeats
            // If a syllable occurs with unspecified tone once, or if it occurs with multiple tone marks
            // -> We only take it as once item with unspecified tone
            // Otherwise, take it as is, with tone mark
            Dictionary<string, int> syllDict = new Dictionary<string, int>();
            foreach (var syll in sylls)
            {
                if (!syllDict.ContainsKey(syll.Text)) syllDict[syll.Text] = syll.Tone;
                else if (syllDict[syll.Text] != syll.Tone) syllDict[syll.Text] = -1;
            }
            List<CedictPinyinSyllable> querySylls = new List<CedictPinyinSyllable>();
            foreach (var x in syllDict)
                querySylls.Add(new CedictPinyinSyllable(x.Key, x.Value));

            // Map from keys (entry positions) to # of query syllables found in entry
            Dictionary<int, int> posToCount = new Dictionary<int, int>();
            // Look at each query syllable, increment counts for entries in syllable's list(s)
            foreach (CedictPinyinSyllable syll in querySylls)
            {
                // If this syllable is not index, we sure won't have any hits
                if (!index.PinyinIndex.ContainsKey(syll.Text))
                    return new List<CedictResult>();
                
                PinyinIndexItem pii = index.PinyinIndex[syll.Text];
                // Query specifies a tone mark: just that list
                if (syll.Tone != -1)
                {
                    List<int> instanceList;
                    if (syll.Tone == 0) instanceList = pii.Entries0;
                    else if (syll.Tone == 1) instanceList = pii.Entries1;
                    else if (syll.Tone == 2) instanceList = pii.Entries2;
                    else if (syll.Tone == 3) instanceList = pii.Entries3;
                    else if (syll.Tone == 4) instanceList = pii.Entries4;
                    else throw new Exception("Invalid tone: " + syll.Tone.ToString());
                    foreach (int pos in instanceList)
                    {
                        if (!posToCount.ContainsKey(pos)) posToCount[pos] = 1;
                        else ++posToCount[pos];
                    }
                }
                // Query does not specify a tone mark
                // Get union of instance vectors, increment each position once
                else
                {
                    HashSet<int> posSet = new HashSet<int>();
                    foreach (int pos in pii.Entries0) posSet.Add(pos);
                    foreach (int pos in pii.Entries1) posSet.Add(pos);
                    foreach (int pos in pii.Entries2) posSet.Add(pos);
                    foreach (int pos in pii.Entries3) posSet.Add(pos);
                    foreach (int pos in pii.Entries4) posSet.Add(pos);
                    foreach (int pos in pii.EntriesNT) posSet.Add(pos);
                    foreach (int pos in posSet)
                    {
                        if (!posToCount.ContainsKey(pos)) posToCount[pos] = 1;
                        else ++posToCount[pos];
                    }
                }
            }
            // Get positions that contain all chars from query
            HashSet<int> matchingPositions = new HashSet<int>();
            foreach (var x in posToCount) if (x.Value == querySylls.Count) matchingPositions.Add(x.Key);
            // Now fetch and verify results
            List<CedictResult> res = doLoadVerifyPinyin(matchingPositions, sylls);
            // Sort pinyin results
            res.Sort((a, b) => pyComp(a, b));
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
        /// Split string into assumed pinyin syllables by tone marks
        /// </summary>
        private static List<string> doPinyinSplitDigits(string str)
        {
            List<string> res = new List<string>();
            string syll = "";
            foreach (char c in str)
            {
                syll += c;
                bool isToneMark = (c >= '0' && c <= '5');
                if (isToneMark)
                {
                    res.Add(syll);
                    syll = "";
                }
            }
            if (syll != string.Empty) res.Add(syll);
            return res;
        }

        /// <summary>
        /// Split string into possible multiple pinyin syllables, or return as whole if not possible.
        /// </summary>
        private static List<string> doPinyinSplitSyllables(string str)
        {
            // TO-DO
            List<string> res = new List<string>();
            res.Add(str);
            return res;
        }

        private static List<CedictPinyinSyllable> doParsePinyin(string query)
        {
            // If query is empty string or WS only: no syllables
            query = query.Trim();
            if (query == string.Empty) return new List<CedictPinyinSyllable>();

            // Only deal with lower-case
            query = query.ToLowerInvariant();
            // Convert "u:" > "v" and "ü" > "v"
            query = query.Replace("u:", "v");
            query = query.Replace("ü", "v");

            // Split by syllables and apostrophes
            string[] explicitSplit = query.Split(new char[] { ' ', '\'', '’' });
            // Further split each part, in case input did not have spaces
            List<string> pinyinSplit = new List<string>();
            foreach (string str in explicitSplit)
            {
                // Find numbers 1 thru 5: tone marks always come at end of syllable
                List<string> numSplit = doPinyinSplitDigits(str);
                // Split the rest by matching known pinyin syllables
                foreach (string str2 in numSplit)
                {
                    List<string> syllSplit = doPinyinSplitSyllables(str2);
                    pinyinSplit.AddRange(syllSplit);
                }
            }
            // Create normalized syllable by separating tone mark, if present
            List<CedictPinyinSyllable> res = new List<CedictPinyinSyllable>();
            foreach (string str in pinyinSplit)
            {
                char c = str[str.Length - 1];
                int val = (int)(c - '0');
                // Tone mark here
                if (val >= 1 && val <= 5 && str.Length > 1)
                {
                    if (val == 5) val = 0;
                    res.Add(new CedictPinyinSyllable(str.Substring(0, str.Length - 1), val));
                }
                // No tone mark: add as unspecified
                else res.Add(new CedictPinyinSyllable(str, -1));
            }
            // If we have syllables ending in "r", split that into separate "r5"
            for (int i = 0; i < res.Count; ++i)
            {
                CedictPinyinSyllable ps = res[i];
                if (ps.Text != "er" && ps.Text.Length > 1 && ps.Text.EndsWith("r"))
                {
                    CedictPinyinSyllable ps1 = new CedictPinyinSyllable(ps.Text.Substring(0, ps.Text.Length - 1), ps.Tone);
                    CedictPinyinSyllable ps2 = new CedictPinyinSyllable("r", 0);
                    res[i] = ps1;
                    res.Insert(i + 1, ps2);
                }
            }
            // Done
            return res;
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
                // Parse pinyin query string
                List<CedictPinyinSyllable> sylls = doParsePinyin(query);
                // Lookup
                res = doPinyinLookupHead(sylls);
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
