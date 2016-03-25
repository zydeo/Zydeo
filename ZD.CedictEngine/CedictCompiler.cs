using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using ZD.Common;

namespace ZD.CedictEngine
{
    /// <summary>
    /// Parses Cedict text file and compiles indexed dictionary in binary format.
    /// </summary>
    public partial class CedictCompiler : IDisposable
    {
        /// <summary>
        /// Current line number, so we can indicate errors/warnings
        /// </summary>
        private int lineNum = 0;

        /// <summary>
        /// <para>True if compiler already serialized results. This is just a bit of a consistency check:</para>
        /// <para>Results must only be written once, and no new lines may be parsed afterwards.</para>
        /// <para>Writing results converts indexes into file positions.</para>
        /// </summary>
        private bool resultsWritten = false;

        /// <summary>
        /// Parsed entries; all kept in memory during parse.
        /// </summary>
        private readonly List<CedictEntry> entries = new List<CedictEntry>();

        /// <summary>
        /// Tokenized senses; all kept in memory during parse.
        /// </summary>
        private readonly List<TokenizedSense> tsenses = new List<TokenizedSense>();

        /// <summary>
        /// Index created during parse.
        /// </summary>
        private readonly Index index = new Index();

        /// <summary>
        /// Entry statistics collected during parsing.
        /// </summary>
        private readonly Stats stats = new Stats();

        /// <summary>
        /// Decomposes headword: hanzi and pinyin.
        /// </summary>
        private static Regex reHead = new Regex(@"^([^ ]+) ([^ ]+) \[([^\]]+)\]$");

        /// <summary>
        /// My tokenizer;
        /// </summary>
        private readonly Tokenizer tokenizer;

        /// <summary>
        /// Ctor: initialize.
        /// </summary>
        public CedictCompiler()
        {
            // At this stage, index will have been initialized in-line.
            // I can refer to its word holder to initialize my tokenizer.
            tokenizer = new Tokenizer(index.WordHolder);
            // Init temp file for hanzi data
            hanziTempFileName = Path.GetTempFileName();
            hanziTempWriter = new BinWriter(hanziTempFileName);
        }

        /// <summary>
        /// Dispose object: clean up temporary files etc.
        /// </summary>
        public void Dispose()
        {
            if (hanziTempWriter != null) hanziTempWriter.Dispose();
            File.Delete(hanziTempFileName);
        }

        /// <summary>
        /// Verifies that line contains no Unicode surrogates. Needed for data hygiene if input is dirty.
        /// </summary>
        private static bool surrogateCheck(string line, StreamWriter logStream, int lineNum)
        {
            bool surrFound = false;
            foreach (char c in line)
            {
                int val = (int)c;
                if (val >= 0xd800 && val <= 0xdfff) { surrFound = true; break; }
            }
            if (!surrFound) return true;
            if (logStream != null)
            {
                string msg = "Line {0}: ERROR: Unicode surrogate found";
                msg = string.Format(msg, lineNum);
                logStream.WriteLine(msg);
            }
            return false;
        }

        /// <summary>
        /// Parses an entry (line) that has been separated into headword and rest.
        /// </summary>
        private static CedictEntry parseEntry(string strHead, string strBody, StreamWriter logStream, int lineNum)
        {
            // Decompose head
            Match hm = reHead.Match(strHead);
            if (!hm.Success)
            {
                string msg = "Line {0}: ERROR: Invalid header syntax: {1}";
                msg = string.Format(msg, lineNum, strHead);
                if (logStream != null) logStream.WriteLine(msg);
                return null;
            }

            // Split pinyin by spaces
            string[] pinyinParts = hm.Groups[3].Value.Split(new char[] { ' ' });

            // Convert pinyin to our normalized format
            PinyinSyllable[] pinyinSylls;
            List<int> pinyinMap;
            normalizePinyin(pinyinParts, out pinyinSylls, out pinyinMap);
            // Weird syllables found > warning
            if (Array.FindIndex(pinyinSylls, x => x.Tone == -1) != -1)
            {
                string msg = "Line {0}: Warning: Weird pinyin syllable: {1}";
                msg = string.Format(msg, lineNum, strHead);
                if (logStream != null) logStream.WriteLine(msg);
            }
            // Trad and simp MUST have same # of chars, always
            if (hm.Groups[1].Value.Length != hm.Groups[2].Value.Length)
            {
                string msg = "Line {0}: ERROR: Trad/simp char count mismatch: {1}";
                msg = string.Format(msg, lineNum, strHead);
                if (logStream != null) logStream.WriteLine(msg);
                return null;
            }
            // Transform map so it says, for each hanzi, which pinyin syllable it corresponds to
            // Some chars in hanzi may have no pinyin: when hanzi includes a non-ideagraphic character
            short[] hanziToPinyin = transformPinyinMap(hm.Groups[1].Value, pinyinMap);
            // Headword MUST have same number of ideo characters as non-weird pinyin syllables
            if (hanziToPinyin == null)
            {
                string msg = "Line {0}: Warning: Failed to match hanzi to pinyin: {1}";
                msg = string.Format(msg, lineNum, strHead);
                if (logStream != null) logStream.WriteLine(msg);
            }
            // Split meanings by slash
            string[] meaningsRaw = strBody.Split(new char[] { '/' });
            List<string> meanings = new List<string>();
            foreach (string s in meaningsRaw)
                if (s.Trim() != "") meanings.Add(s.Trim());
            if (meaningsRaw.Length != meanings.Count)
            {
                string msg = "Line {0}: Warning: Empty sense in entry: {1}";
                msg = string.Format(msg, lineNum, strBody);
                if (logStream != null) logStream.WriteLine(msg);
            }
            // At least one meaning!
            if (meanings.Count == 0)
            {
                string msg = "Line {0}: ERROR: No sense: {1}";
                msg = string.Format(msg, lineNum, strBody);
                if (logStream != null) logStream.WriteLine(msg);
                return null;
            }
            // Separate domain, equiv and not in each sense
            List<CedictSense> cedictSenses = new List<CedictSense>();
            foreach (string s in meanings)
            {
                string domain, equiv, note;
                trimSense(s, out domain, out equiv, out note);
                // Equiv is empty: merits at least a warning
                if (equiv == "")
                {
                    string msg = "Line {0}: Warning: No equivalent in sense, only domain/notes: {1}";
                    msg = string.Format(msg, lineNum, s);
                    if (logStream != null) logStream.WriteLine(msg);
                }
                // Convert all parts of sense to hybrid text
                HybridText hDomain = plainTextToHybrid(domain, lineNum, logStream);
                HybridText hEquiv = plainTextToHybrid(equiv, lineNum, logStream);
                HybridText hNote = plainTextToHybrid(note, lineNum, logStream);
                // Store new sense - unless we failed to parse anything properly
                if (hDomain != null && hEquiv != null && hNote != null)
                {
                    cedictSenses.Add(new CedictSense(hDomain, hEquiv, hNote));
                }
            }
            // If there are no senses, we failed. But that will have been logged before, so just return null.
            if (cedictSenses.Count == 0) return null;
            // Done with entry
            CedictEntry res = new CedictEntry(hm.Groups[2].Value, hm.Groups[1].Value,
                new ReadOnlyCollection<PinyinSyllable>(pinyinSylls),
                new ReadOnlyCollection<CedictSense>(cedictSenses),
                hanziToPinyin);
            return res;
        }

        /// <summary>
        /// Returns true if character is ideographic (Hanzi).
        /// </summary>
        private static bool isIdeo(char c)
        {
            // VERY rough "definition" but if works for out purpose
            int cval = (int)c;
            return cval >= 0x2e80;
        }

        /// <summary>
        /// Returns true if string has ideographic (Hanzi) characters.
        /// </summary>
        private static bool hasIdeo(string str)
        {
            foreach (char c in str)
                if (isIdeo(c)) return true;
            return false;
        }

        /// <summary>
        /// <para>Receives hanzi and pinyin map from <see cref="normalizePinyin"/>.</para>
        /// <para>Returns list as long as hanzi. Each number in list is info for a hanzi,</para>
        /// <para>identifying the corresponding pinyin syllable.</para>
        /// <para>Non-ideo chars in hanzi have no pinyin syllable.</para>
        /// </summary>
        private static short[] transformPinyinMap(string hanzi, List<int> mapIn)
        {
            if (hanzi.Length >= short.MaxValue || mapIn.Count >= short.MaxValue)
                throw new Exception("Hanzi too long, or too many pinyin syllables.");

            short[] res = new short[hanzi.Length];
            short ppos = 0; // position in incoming map; that map has an entry for each normal pinyin syllable
            for (int i = 0; i != hanzi.Length; ++i)
            {
                char c = hanzi[i];
                // Character is not ideographic: no corresponding pinyin
                if (!isIdeo(c))
                {
                    res[i] = -1;
                    continue;
                }
                // We have run out of pinyin map: BAD
                if (ppos >= mapIn.Count) return null;
                // We've got this hanzi's pinyin syllable
                res[i] = (short)mapIn[ppos];
                ++ppos;
            }
            // At this stage we must have consumed incoming map
            // Otherwise: BAD
            if (ppos != mapIn.Count) return null;
            return res;
        }

        /// <summary>
        /// Delegate so we can access <see cref="normalizePinyin"/> in embedded classes.
        /// </summary>
        private delegate void NormalizePinyinDelegate(string[] parts, out PinyinSyllable[] syllsArr, out List<int> pinyinMap);

        /// <summary>
        /// Normalizes array of Cedict-style pinyin syllables into our format.
        /// </summary>
        private static void normalizePinyin(string[] parts, out PinyinSyllable[] syllsArr, out List<int> pinyinMap)
        {
            // What this function does:
            // - Separates tone mark from text (unless it's a "weird" syllable
            // - Replaces "u:" with "v"
            // - Replaces "ü" with "v"
            // - Maps every non-weird input syllable to r5-merged output syllables
            //   List has as many values as there are non-weird input syllables
            //   Values in list point into "sylls" output array
            //   Up to two positions can have same value (for r5 appending)
            pinyinMap = new List<int>();
            List<PinyinSyllable> sylls = new List<PinyinSyllable>();
            foreach (string ps in parts)
            {
                // Does not end with a tone mark (1 thru 5): weird
                char chrLast = ps[ps.Length - 1];
                if (chrLast < '1' || chrLast > '5')
                {
                    sylls.Add(new PinyinSyllable(ps, -1));
                    continue;
                }
                // Separate tone and text
                string text = ps.Substring(0, ps.Length - 1);
                int tone = ((int)chrLast) - ((int)'0');
                // Neutral tone for us is 0, not five
                if (tone == 5) tone = 0;
                // "u:" is for us "v"
                // "ü" is for us "v"
                text = text.Replace("u:", "v");
                text = text.Replace("U:", "V");
                text = text.Replace("ü", "v");
                text = text.Replace("Ü", "V");
                // Store new syllable
                sylls.Add(new PinyinSyllable(text, tone));
                // Add to map
                pinyinMap.Add(sylls.Count - 1);
            }
            // Result: the syllables as an array.
            syllsArr = sylls.ToArray();
        }

        /// <summary>
        /// Sanitize one line; split into head and body.
        /// </summary>
        private static void sanitizeAndSplit(string line, out string strHead, out string strBody)
        {
            // Comments, empty lines, some basic normalization
            line = line.Replace(' ', ' '); // NBSP
            line = line.Replace('“', '"'); // Curly quote
            line = line.Replace('”', '"'); // Curly quote

            // Initial split: header vs body
            int firstSlash = line.IndexOf('/');
            strHead = line.Substring(0, firstSlash).Trim();
            strBody = line.Substring(firstSlash + 1).Trim(new char[] { ' ', '/' });
        }

        /// <summary>
        /// Parse a single entry. Return null if rejected for whatever reason.
        /// </summary>
        /// <param name="line">Line to parse.</param>
        /// <param name="lineNum">Line number in input.</param>
        /// <param name="swLog">Stream to log warnings. Can be null.</param>
        /// <param name="swDrop">Stream to record dropped entries (failed to parse). Can be null.</param>
        public static CedictEntry ParseEntry(string line, int lineNum, StreamWriter swLog, StreamWriter swDrop)
        {
            // Empty lines
            if (line.Trim() == "" || line.StartsWith("#")) return null;
            // Cannot handle code points about 0xffff
            if (!surrogateCheck(line, swLog, lineNum)) return null;
            // Sanitization and initial split
            string strHead, strBody;
            sanitizeAndSplit(line, out strHead, out strBody);
            // Parse entry. If failed > null.
            CedictEntry entry = null;
            try { entry = parseEntry(strHead, strBody, swLog, lineNum); }
            catch { if (swDrop != null) swDrop.WriteLine(line); }
            return entry;
        }

        /// <summary>
        /// Processes one line of the text-based Cedict input file.
        /// </summary>
        public void ProcessLine(string line, StreamWriter logStream, StreamWriter swKept, StreamWriter swDrop)
        {
            string origLine = line;

            // Must not parse new lines once results have been written
            if (resultsWritten) throw new Exception("WriteResults already called, cannot parse additional lines.");

            ++lineNum;

            // Empty lines
            if (line.Trim() == "" || line.StartsWith("#")) return;
            // Cannot handle code points about 0xffff
            if (!surrogateCheck(origLine, logStream, lineNum))
            {
                swDrop.WriteLine(origLine);
                return;
            }

            // Sanitization and initial split
            string strHead, strBody;
            sanitizeAndSplit(line, out strHead, out strBody);

            // Parse entry. If failed, we be done here.
            CedictEntry entry;
            try
            {
                entry = parseEntry(strHead, strBody, logStream, lineNum);
            }
            catch
            {
                // Failed to parse: dropped
                swDrop.WriteLine(origLine);
                return;
            }
            if (entry == null) return;
            // Store and index entry
            int id = entries.Count;
            entries.Add(entry);
            indexEntry(entry, id);
            // Log as kept
            swKept.WriteLine(origLine);
            // Update statistics
            stats.CalculateEntryStats(entry);
        }

        /// <summary>
        /// Indexes one parsed Cedict entry (hanzi, pinyin and target-language indexes).
        /// </summary>
        private void indexEntry(CedictEntry entry, int id)
        {
            // Collect different chars in both headwords
            HashSet<char> simpSet = new HashSet<char>();
            foreach (char c in entry.ChSimpl) simpSet.Add(c);
            if (simpSet.Count > byte.MaxValue) throw new Exception("Simplified headword too long; max: 255.");
            byte simpCount = (byte)simpSet.Count;
            HashSet<char> tradSet = new HashSet<char>();
            foreach (char c in entry.ChTrad) tradSet.Add(c);
            if (tradSet.Count > byte.MaxValue) throw new Exception("Traditional headword too long; max: 255.");
            byte tradCount = (byte)tradSet.Count;
            // Index character of simplified headword
            foreach (char c in simpSet)
            {
                IdeoIndexItem ii;
                if (index.IdeoIndex.ContainsKey(c)) ii = index.IdeoIndex[c];
                else
                {
                    ii = new IdeoIndexItem();
                    index.IdeoIndex[c] = ii;
                }
                ii.EntriesHeadwordSimp.Add(new IdeoEntryPtr { EntryIdx = id, HwCharCount = simpCount });
            }
            // Index characters of traditional headword
            foreach (char c in tradSet)
            {
                IdeoIndexItem ii;
                if (index.IdeoIndex.ContainsKey(c)) ii = index.IdeoIndex[c];
                else
                {
                    ii = new IdeoIndexItem();
                    index.IdeoIndex[c] = ii;
                }
                ii.EntriesHeadwordTrad.Add(new IdeoEntryPtr { EntryIdx = id, HwCharCount = tradCount });
            }
            // Index pinyin syllables
            foreach (PinyinSyllable pys in entry.Pinyin)
            {
                PinyinIndexItem pi;
                // Index contains lower-case syllables
                string textLo = pys.Text.ToLowerInvariant();
                if (index.PinyinIndex.ContainsKey(textLo)) pi = index.PinyinIndex[textLo];
                else
                {
                    pi = new PinyinIndexItem();
                    index.PinyinIndex[textLo] = pi;
                }
                // Figure out which list in index item - by tone
                List<int> entryList;
                if (pys.Tone == -1) entryList = pi.EntriesNT;
                else if (pys.Tone == 0) entryList = pi.Entries0;
                else if (pys.Tone == 1) entryList = pi.Entries1;
                else if (pys.Tone == 2) entryList = pi.Entries2;
                else if (pys.Tone == 3) entryList = pi.Entries3;
                else if (pys.Tone == 4) entryList = pi.Entries4;
                else throw new Exception("Invalid tone: " + pys.Tone.ToString());
                // Avoid indexing same entry twice if a syllable occurs multiple times
                if (entryList.Count == 0 || entryList[entryList.Count - 1] != id)
                    entryList.Add(id);
            }
            // Index equiv of each sense
            int senseIx = -1;
            foreach (CedictSense sense in entry.Senses)
            {
                ++senseIx;
                // Empty equiv: nothing to index
                if (sense.Equiv.IsEmpty) continue;
                // Tokenize
                ReadOnlyCollection<EquivToken> tokens = tokenizer.Tokenize(sense.Equiv);
                // Index sense
                indexSense(tokens, id, senseIx);
            }
        }

        private void indexSense(ReadOnlyCollection<EquivToken> tokens, int entryId, int senseIx)
        {
            // If there are no non-Chinese, non-number tokens: nothing to save, nothing to index
            bool relevant = false;
            foreach (EquivToken eqt in tokens)
            {
                if (eqt.TokenId != index.WordHolder.IdZho && eqt.TokenId != index.WordHolder.IdNum)
                { relevant = true; break; }
            }
            if (!relevant) return;

            // Keep tokenized sense in memory
            int senseId = tsenses.Count;
            TokenizedSense ts = new TokenizedSense(entryId, senseIx, tokens);
            tsenses.Add(ts);
            // Add to instance list of each token in list
            // First get set of different token IDs - we don't index dupes
            HashSet<int> tokenIdSet = new HashSet<int>();
            foreach (EquivToken eqt in tokens) tokenIdSet.Add(eqt.TokenId);
            // Now, index each distinct ID
            foreach (int tokenId in tokenIdSet)
            {
                SenseIndexItem sii;
                if (!index.SenseIndex.ContainsKey(tokenId))
                {
                    sii = new SenseIndexItem();
                    index.SenseIndex[tokenId] = sii;
                }
                else sii = index.SenseIndex[tokenId];
                if (tokenIdSet.Count > byte.MaxValue)
                    throw new Exception("Sense's token count out of byte range: " + tokenIdSet.Count.ToString());
                SenseInfo senseInfo = new SenseInfo
                {
                    TokenizedSenseId = senseId,
                    TokensInSense = (byte)tokenIdSet.Count,
                };
                sii.Instances.Add(senseInfo);
            }
        }

        /// <summary>
        /// Replaces entry IDs with file positions in list. Called when finalizing index.
        /// </summary>
        private static void replaceIdsWithPositions(List<int> list, Dictionary<int, int> idToPos)
        {
            for (int i = 0; i != list.Count; ++i)
            {
                int id = list[i];
                int pos = idToPos[id];
                list[i] = pos;
            }
        }

        /// <summary>
        /// Replaces entry IDs with file positions in list. Called when finalizing index.
        /// </summary>
        private static void replaceIdsWithPositions(List<IdeoEntryPtr> list, Dictionary<int, int> idToPos)
        {
            for (int i = 0; i != list.Count; ++i)
            {
                int id = list[i].EntryIdx;
                byte cnt = list[i].HwCharCount;
                int pos = idToPos[id];
                list[i] = new IdeoEntryPtr { EntryIdx = pos, HwCharCount = cnt };
            }
        }

        /// <summary>
        /// Writes parsed and indexed dictionary to compiled binary file.
        /// </summary>
        public void WriteResults(DateTime date, string dictFileName, string statsFolder)
        {
            // Cannot do this twice: we'll have replaced entry IDs with file positions in index
            if (resultsWritten) throw new InvalidOperationException("WriteResults already called.");
            resultsWritten = true;

            // First, statistics
            stats.WriteStats(statsFolder);

            // Start index of Hanzi repository in file
            int hrepoIdxPos;

            // ID to file position
            Dictionary<int, int> entryIdToPos = new Dictionary<int, int>();
            Dictionary<int, int> senseIdToPos = new Dictionary<int, int>();
            using (BinWriter bw = new BinWriter(dictFileName))
            {
                // Write date and entry count
                bw.WriteLong(date.Ticks);
                bw.WriteInt(entries.Count);
                int returnPos = bw.Position;
                // Placeholder: will return here to save start position of index at end
                bw.WriteInt(-1);
                // Placeholder for hanzi repo position: will return here at very end
                hrepoIdxPos = bw.Position;
                bw.WriteInt(-1);
                // Serialize all entries; fill entry ID -> file pos map
                for (int i = 0; i != entries.Count; ++i)
                {
                    entryIdToPos[i] = bw.Position;
                    entries[i].Serialize(bw);
                }
                // Replace entry IDs with file positions in all tokenized senses
                for (int i = 0; i != tsenses.Count; ++i)
                {
                    tsenses[i].EntryId = entryIdToPos[tsenses[i].EntryId];
                }
                // Serialize all tokenized senses; fill sense ID -> file pos map
                for (int i = 0; i != tsenses.Count; ++i)
                {
                    senseIdToPos[i] = bw.Position;
                    tsenses[i].Serialize(bw);
                }
                // Fill in index start position
                int idxPos = bw.Position;
                bw.Position = returnPos;
                bw.WriteInt(idxPos);
                bw.Position = idxPos;
                // Replace IDs with file positions across index
                foreach (var x in index.IdeoIndex)
                {
                    replaceIdsWithPositions(x.Value.EntriesHeadwordSimp, entryIdToPos);
                    replaceIdsWithPositions(x.Value.EntriesHeadwordTrad, entryIdToPos);
                    replaceIdsWithPositions(x.Value.EntriesSense, entryIdToPos);
                }
                foreach (var x in index.PinyinIndex)
                {
                    replaceIdsWithPositions(x.Value.EntriesNT, entryIdToPos);
                    replaceIdsWithPositions(x.Value.Entries0, entryIdToPos);
                    replaceIdsWithPositions(x.Value.Entries1, entryIdToPos);
                    replaceIdsWithPositions(x.Value.Entries2, entryIdToPos);
                    replaceIdsWithPositions(x.Value.Entries3, entryIdToPos);
                    replaceIdsWithPositions(x.Value.Entries4, entryIdToPos);
                }
                foreach (var x in index.SenseIndex)
                {
                    List<SenseInfo> instances = x.Value.Instances;
                    for (int i = 0; i != instances.Count; ++i)
                    {
                        SenseInfo senseInfo = instances[i];
                        senseInfo.TokenizedSenseId = senseIdToPos[senseInfo.TokenizedSenseId];
                        instances[i] = senseInfo;
                    }
                }
                // Serialize index
                index.Serialize(bw);
                // Copy serialized hanzi repository from temp file
                writeHanziRepo(bw, hrepoIdxPos);
            }
        }
    }
}
