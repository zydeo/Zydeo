using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using DND.Common;

namespace DND.CedictEngine
{
    public class DictEngine : ICedictEngine
    {
        private readonly string dictFileName;
        private readonly Index index;

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
                        CedictResult res = new CedictResult(entry,
                            hiliteStart, hiliteLength);
                        resList.Add(res);
                    }
                }
            }
            return resList;
        }

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
                if (script == SearchScript.Simplified || script == SearchScript.Both)
                {
                    foreach (int pos in iii.EntriesHeadwordSimp)
                    {
                        if (posToCountSimp.ContainsKey(pos)) ++posToCountSimp[pos];
                        else posToCountSimp[pos] = 1;
                    }
                }
                if (script == SearchScript.Traditional || script == SearchScript.Both)
                {
                    foreach (int pos in iii.EntriesHeadwordTrad)
                    {
                        if (posToCountTrad.ContainsKey(pos)) ++posToCountTrad[pos];
                        else posToCountTrad[pos] = 1;
                    }
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

        public CedictLookupResult Lookup(string query, SearchScript script, SearchLang lang)
        {
            List<CedictResult> res;
            // For now, always just do hanzi lookup
            // TO-DO: heuristics based on search string; pinyin and EN lookup
            res = doHanziLookupHead(query, script);

            return new CedictLookupResult(new ReadOnlyCollection<CedictResult>(res), lang);
        }
    }
}
