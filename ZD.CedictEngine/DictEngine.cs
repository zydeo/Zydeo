using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using ZD.Common;

namespace ZD.CedictEngine
{
    /// <summary>
    /// Dictionary engine: loads index from file, provides thread-safe lookup functionality.
    /// </summary>
    public partial class DictEngine : ICedictEngine
    {
        /// <summary>
        /// Name of binary dictionary file.
        /// </summary>
        private readonly string dictFileName;

        /// <summary>
        /// Font coverage info provider. Used to filter results we chars the caller cannot display.
        /// </summary>
        private readonly IFontCoverage cvr;

        /// <summary>
        /// Index: loaded from dictionary file when object is created.
        /// </summary>
        private readonly Index index;

        /// <summary>
        /// Tokenizer for target text (query strings).
        /// </summary>
        private readonly Tokenizer tokenizer;

        /// <summary>
        /// Ctor: initialize from binary dictionary file.
        /// </summary>
        /// <param name="dictFileName">Name of the compiled binary dictionary.</param>
        /// <param name="cvr">Font coverage info provider for lookup filtering.</param>
        public DictEngine(string dictFileName, IFontCoverage cvr)
        {
            this.dictFileName = dictFileName;
            this.cvr = cvr;
            using (BinReader br = new BinReader(dictFileName))
            {
                // Skip release date and entry count
                br.ReadLong();
                br.ReadInt();
                // Skip to position where index starts
                int idxPos = br.ReadInt();
                br.Position = idxPos;
                // Load index
                index = new Index(br);
            }
            // Now, initialize tokenizer with index's word holder
            tokenizer = new Tokenizer(index.WordHolder);
        }

        /// <summary>
        /// Static ctor: load info stored in static members.
        /// </summary>
        static DictEngine()
        {
            loadSyllabary();
        }

        /// <summary>
        /// Entry provider returned to caller after every lookup.
        /// </summary>
        private class EntryProvider : ICedictEntryProvider
        {
            /// <summary>
            /// Binary deserializer holding an open stream of dictionary file.
            /// </summary>
            private readonly BinReader br;

            /// <summary>
            /// Ctor: takes ownership of binary deserializer.
            /// </summary>
            /// <param name="br"></param>
            public EntryProvider(BinReader br)
            {
                if (br == null) throw new ArgumentNullException("br");
                this.br = br;
            }

            /// <summary>
            /// Closes file stream.
            /// </summary>
            public void Dispose()
            {
                br.Dispose();
            }

            /// <summary>
            /// Returns the dictionary entry identified by the provided file position.
            /// </summary>
            public CedictEntry GetEntry(int entryId)
            {
                br.Position = entryId;
                return new CedictEntry(br);
            }
        }

        /// <summary>
        /// Returns true of display font covers all Hanzi in entry; false otherwise.
        /// </summary>
        private bool areHanziCovered(string simp, string trad)
        {
            // Simplified headword
            foreach (char c in simp)
                if (!cvr.GetCoverage(c).HasFlag(FontCoverageFlags.Simp))
                    return false;
            // Traditional headword
            foreach (char c in trad)
                if (!cvr.GetCoverage(c).HasFlag(FontCoverageFlags.Trad))
                    return false;
            return true;
        }

        /// <summary>
        /// Returns true if display font covers all Hanzi in hybrid text; false otherwise.
        /// </summary>
        private bool areHanziCovered(HybridText ht)
        {
            if (ht.IsEmpty) return true;
            for (int i = 0; i != ht.RunCount; ++i)
            {
                TextRun tr = ht.GetRunAt(i);
                TextRunZho trJoe = tr as TextRunZho;
                if (trJoe == null) continue;
                if (trJoe.Simp == null) continue;
                foreach (char c in trJoe.Simp)
                    if (!cvr.GetCoverage(c).HasFlag(FontCoverageFlags.Simp))
                        return false;
                if (trJoe.Trad == null) continue;
                foreach (char c in trJoe.Trad)
                    if (!cvr.GetCoverage(c).HasFlag(FontCoverageFlags.Trad))
                        return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true of display font covers all Hanzi in entry; false otherwise.
        /// </summary>
        private bool areHanziCovered(CedictEntry entry)
        {
            // Simplified and traditional headword
            if (!areHanziCovered(entry.ChSimpl, entry.ChTrad))
                return false;
            // Hanzi in hybrid text of senses
            for (int i = 0; i != entry.SenseCount; ++i)
            {
                CedictSense cs = entry.GetSenseAt(i);
                if (!areHanziCovered(cs.Domain)) return false;
                if (!areHanziCovered(cs.Equiv)) return false;
                if (!areHanziCovered(cs.Note)) return false;
            }
            // We're good to go.
            return true;
        }

        /// <summary>
        /// Retrieves hanzi lookup candidates, verifies actual presence of search expression in headword.
        /// </summary>
        List<ResWithEntry> doLoadVerifyHanzi(BinReader br, IEnumerable<int> poss, string query, SearchScript script)
        {
            List<ResWithEntry> resList = new List<ResWithEntry>();
            // Yes, we only open our file on-demand
            // But we do this within each lookup's scope, so lookup stays thread-safe
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
                    // Drop if there's any unprintable hanzi
                    if (!areHanziCovered(entry)) continue;
                    
                    // TO-DO: indicate wrong script in result
                    CedictResult res = new CedictResult(CedictResult.SimpTradWarning.None,
                        pos, entry.HanziPinyinMap,
                        hiliteStart, hiliteLength);
                    ResWithEntry resWE = new ResWithEntry(res, entry);
                    resList.Add(resWE);
                }
            }
            return resList;
        }

        /// <summary>
        /// Compares lookup results after hanzi lookup for sorted presentation.
        /// </summary>
        private static int hrComp(ResWithEntry a, ResWithEntry b)
        {
            // Shorter entry comes first
            int lengthCmp = a.Entry.ChSimpl.Length.CompareTo(b.Entry.ChSimpl.Length);
            if (lengthCmp != 0) return lengthCmp;
            // Between equally long headwords where match starts sooner comes first
            int startCmp = a.Res.HanziHiliteStart.CompareTo(b.Res.HanziHiliteStart);
            if (startCmp != 0) return startCmp;
            // Order equally long entries by pinyin lexicographical order
            return a.Entry.PinyinCompare(b.Entry);
        }

        /// <summary>
        /// Retrieves entries (sorted) whose headword contains hanzi from search expression.
        /// </summary>
        List<CedictResult> doHanziLookupHead(BinReader br, string query, SearchScript script)
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
            List<ResWithEntry> resWE = doLoadVerifyHanzi(br, matchingPositions, query, script);
            // Sort hanzi results
            resWE.Sort((a, b) => hrComp(a, b));
            // Done.
            List<CedictResult> res = new List<CedictResult>(resWE.Capacity);
            for (int i = 0; i != resWE.Count; ++i) res.Add(resWE[i].Res);
            return res;
        }

        /// <summary>
        /// A lookup result with its loaded entry; needed to be able to sort results before throwing away entry itself.
        /// </summary>
        private struct ResWithEntry
        {
            public readonly CedictResult Res;
            public readonly CedictEntry Entry;
            public ResWithEntry(CedictResult res, CedictEntry entry)
            {
                Res = res;
                Entry = entry;
            }
        }

        /// <summary>
        /// Retrieves pinyin lookup candidates, verifies actual presence of search expression in headword.
        /// </summary>
        List<ResWithEntry> doLoadVerifyPinyin(BinReader br, IEnumerable<int> poss, List<PinyinSyllable> sylls)
        {
            List<ResWithEntry> resList = new List<ResWithEntry>();
            // Yes, we only open our file on-demand
            // But we do this within each lookup's scope, so lookup stays thread-safe
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
                        PinyinSyllable syllEntry = entry.GetPinyinAt(i + j);
                        PinyinSyllable syllQuery = sylls[j];
                        if (syllEntry.Text.ToLowerInvariant() != syllQuery.Text) break;
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

                // Drop if there's any unprintable Hanzi
                if (!areHanziCovered(entry)) continue;

                // Keeper!
                CedictResult res = new CedictResult(pos, entry.HanziPinyinMap, syllStart, sylls.Count);
                ResWithEntry resWE = new ResWithEntry(res, entry);
                resList.Add(resWE);
            }
            return resList;
        }

        /// <summary>
        /// Compares lookup results after pinyin lookup for sorted presentation.
        /// </summary>
        private static int pyComp(ResWithEntry a, ResWithEntry b)
        {
            // Shorter entry comes first
            int lengthCmp = a.Entry.PinyinCount.CompareTo(b.Entry.PinyinCount);
            if (lengthCmp != 0) return lengthCmp;
            // Between equally long headwords where match starts sooner comes first
            int startCmp = a.Res.PinyinHiliteStart.CompareTo(b.Res.PinyinHiliteStart);
            if (startCmp != 0) return startCmp;
            // Order equally long entries by pinyin lexicographical order
            return a.Entry.PinyinCompare(b.Entry);
        }

        /// <summary>
        /// Retrieves entries (sorted) whose headword contains pinyin from search expression.
        /// </summary>
        List<CedictResult> doPinyinLookupHead(BinReader br, List<PinyinSyllable> sylls)
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
            List<PinyinSyllable> querySylls = new List<PinyinSyllable>();
            foreach (var x in syllDict)
                querySylls.Add(new PinyinSyllable(x.Key, x.Value));

            // Map from keys (entry positions) to # of query syllables found in entry
            Dictionary<int, int> posToCount = new Dictionary<int, int>();
            // Look at each query syllable, increment counts for entries in syllable's list(s)
            foreach (PinyinSyllable syll in querySylls)
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
            List<ResWithEntry> resWE = doLoadVerifyPinyin(br, matchingPositions, sylls);
            // Sort pinyin results
            resWE.Sort((a, b) => pyComp(a, b));
            // Done.
            List<CedictResult> res = new List<CedictResult>(resWE.Capacity);
            for (int i = 0; i != resWE.Count; ++i) res.Add(resWE[i].Res);
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
        /// Parses a pinyin query string into normalized syllables.
        /// </summary>
        private static List<PinyinSyllable> doParsePinyin(string query)
        {
            // If query is empty string or WS only: no syllables
            query = query.Trim();
            if (query == string.Empty) return new List<PinyinSyllable>();

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
                // Important: this also eliminates empty syllables
                List<string> numSplit = doPinyinSplitDigits(str);
                // Split the rest by matching known pinyin syllables
                foreach (string str2 in numSplit)
                {
                    List<string> syllSplit = doPinyinSplitSyllables(str2);
                    pinyinSplit.AddRange(syllSplit);
                }
            }
            // Create normalized syllable by separating tone mark, if present
            List<PinyinSyllable> res = new List<PinyinSyllable>();
            foreach (string str in pinyinSplit)
            {
                char c = str[str.Length - 1];
                int val = (int)(c - '0');
                // Tone mark here
                if (val >= 1 && val <= 5 && str.Length > 1)
                {
                    if (val == 5) val = 0;
                    res.Add(new PinyinSyllable(str.Substring(0, str.Length - 1), val));
                }
                // No tone mark: add as unspecified
                else res.Add(new PinyinSyllable(str, -1));
            }
            // If we have syllables ending in "r", split that into separate "r5"
            for (int i = 0; i < res.Count; ++i)
            {
                PinyinSyllable ps = res[i];
                if (ps.Text != "er" && ps.Text.Length > 1 && ps.Text.EndsWith("r"))
                {
                    PinyinSyllable ps1 = new PinyinSyllable(ps.Text.Substring(0, ps.Text.Length - 1), ps.Tone);
                    PinyinSyllable ps2 = new PinyinSyllable("r", 0);
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
        private List<CedictResult> doChineseLookup(BinReader br, string query, SearchScript script)
        {
            List<CedictResult> res = new List<CedictResult>();
            // If query string has ideographic characters, do hanzi looup
            if (hasIdeo(query)) res = doHanziLookupHead(br, query, script);
            // Otherwise, do pinyin lookup
            else
            {
                // Parse pinyin query string
                List<PinyinSyllable> sylls = doParsePinyin(query);
                // Lookup
                res = doPinyinLookupHead(br, sylls);
            }
            // Done
            return res;
        }

        /// <summary>
        /// Retrieves an entry from the dictionary. See <see cref="ICedictEngine.GetEntry"/>.
        /// </summary>
        /// <param name="entryId"></param>
        /// <returns></returns>
        public CedictEntry GetEntry(int entryId)
        {
            using (BinReader br = new BinReader(dictFileName))
            {
                br.Position = entryId;
                return new CedictEntry(br);
            }
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
            // BinReader: I own it until I successfully return results to caller.
            BinReader br = new BinReader(dictFileName);
            EntryProvider ep = new EntryProvider(br);

            try
            {
                // Try first in language requested by user
                // If no results that way, try in opposite language
                // Override if lookup in opposite language is successful
                if (lang == SearchLang.Chinese)
                {
                    res = doChineseLookup(br, query, script);
                    // We got fish
                    if (res.Count > 0)
                        return new CedictLookupResult(ep, new ReadOnlyCollection<CedictResult>(res), lang);
                    // OK, try opposite (target)
                    res = doTargetLookup(br, query);
                    // We got fish: override
                    if (res.Count > 0)
                        return new CedictLookupResult(ep, new ReadOnlyCollection<CedictResult>(res), SearchLang.Target);
                }
                else
                {
                    res = doTargetLookup(br, query);
                    // We got fish
                    if (res.Count > 0)
                        return new CedictLookupResult(ep, new ReadOnlyCollection<CedictResult>(res), lang);
                    // OK, try opposite (target)
                    res = doChineseLookup(br, query, script);
                    // We got fish: override
                    if (res.Count > 0)
                        return new CedictLookupResult(ep, new ReadOnlyCollection<CedictResult>(res), SearchLang.Chinese);
                }
                // Sorry, no results, no override
                return new CedictLookupResult(ep, new ReadOnlyCollection<CedictResult>(res), lang);
            }
            catch
            {
                br.Dispose();
                throw;
            }
        }
    }
}
