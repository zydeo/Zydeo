using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using DND.Common;

namespace DND.CedictEngine
{
    public class CedictCompiler
    {
        private int lineNum = 0;
        private bool resultsWritten = false;

        private readonly List<CedictEntry> entries = new List<CedictEntry>();
        private readonly Index index = new Index();
        
        public CedictCompiler()
        {
        }

        private void parseSense(string sense, out string domain, out string equiv, out string note)
        {
            equiv = sense;
            domain = note = "";
        }

        private Regex reHead = new Regex(@"^([^ ]+) ([^ ]+) \[([^\]]+)\]$");

        private CedictEntry parseEntry(string strHead, string strBody, StreamWriter logStream)
        {
            // Decompose head
            Match hm = reHead.Match(strHead);
            if (!hm.Success)
            {
                string msg = "Line {0}: ERROR: Invalid header syntax: {1}";
                msg = string.Format(msg, lineNum, strHead);
                logStream.WriteLine(msg);
                return null;
            }
            // Split pinyin by spaces
            string[] pinyinParts = hm.Groups[3].Value.Split(new char[] { ' ' });
            // Validate: simp / trad / pinyin count
            // Not strict about this, exceptions exist
            if (hm.Groups[2].Value.Length != pinyinParts.Length)
            {
                string msg = "Line {0}: Warning: Character / pinyin count mismatch: {1}";
                msg = string.Format(msg, lineNum, strHead);
                logStream.WriteLine(msg);
            }
            // Trad and simp MUST have same # of chars, always
            if (hm.Groups[1].Value.Length != hm.Groups[2].Value.Length)
            {
                string msg = "Line {0}: ERROR: Trad/simp char count mismatch: {1}";
                msg = string.Format(msg, lineNum, strHead);
                logStream.WriteLine(msg);
                return null;
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
                logStream.WriteLine(msg);
            }
            // At least one meaning!
            if (meanings.Count == 0)
            {
                string msg = "Line {0}: ERROR: No sense: {1}";
                msg = string.Format(msg, lineNum, strBody);
                logStream.WriteLine(msg);
                return null;
            }
            // Separate domain, equiv and not in each sense
            List<CedictSense> cedictSenses = new List<CedictSense>();
            foreach (string s in meanings)
            {
                string domain, equiv, note;
                parseSense(s, out domain, out equiv, out note);
                cedictSenses.Add(new CedictSense(domain, equiv, note));
            }
            // Done with entry
            CedictEntry res = new CedictEntry(hm.Groups[2].Value, hm.Groups[1].Value,
                new ReadOnlyCollection<string>(pinyinParts),
                new ReadOnlyCollection<CedictSense>(cedictSenses));
            return res;
        }

        public void ProcessLine(string line, StreamWriter logStream)
        {
            ++lineNum;
            // Comments, empty lines
            if (line.Trim() == "" || line.StartsWith("#")) return;
            // Initial split: header vs body
            int firstSlash = line.IndexOf('/');
            string strHead = line.Substring(0, firstSlash).Trim();
            string strBody = line.Substring(firstSlash + 1).Trim(new char[] { ' ', '/' });
            // Parse entry. If failed, we be done here.
            CedictEntry entry = parseEntry(strHead, strBody, logStream);
            if (entry == null) return;
            // Store and index entry
            int id = entries.Count;
            entries.Add(entry);
            indexEntry(entry, id);
        }

        private void indexEntry(CedictEntry entry, int id)
        {
            foreach (char c in entry.ChSimpl)
            {
                IdeoIndexItem ii;
                if (index.IdeoIndex.ContainsKey(c)) ii = index.IdeoIndex[c];
                else
                {
                    ii = new IdeoIndexItem();
                    index.IdeoIndex[c] = ii;
                }
                // Avoid indexing same entry twice if a char occurs multiple times
                if (ii.EntriesHeadwordSimp.Count == 0 ||
                    ii.EntriesHeadwordSimp[ii.EntriesHeadwordSimp.Count - 1] != id)
                    ii.EntriesHeadwordSimp.Add(id);
            }
            foreach (char c in entry.ChTrad)
            {
                IdeoIndexItem ii;
                if (index.IdeoIndex.ContainsKey(c)) ii = index.IdeoIndex[c];
                else
                {
                    ii = new IdeoIndexItem();
                    index.IdeoIndex[c] = ii;
                }
                // Avoid indexing same entry twice if a char occurs multiple times
                if (ii.EntriesHeadwordTrad.Count == 0 ||
                    ii.EntriesHeadwordTrad[ii.EntriesHeadwordTrad.Count - 1] != id)
                    ii.EntriesHeadwordTrad.Add(id);
            }
            foreach (string pys in entry.Pinyin)
            {
                PinyinIndexItem pi;
                if (index.PinyinIndex.ContainsKey(pys)) pi = index.PinyinIndex[pys];
                else
                {
                    pi = new PinyinIndexItem();
                    index.PinyinIndex[pys] = pi;
                }
                // Avoid indexing same entry twice if a syllable occurs multiple times
                if (pi.Entries.Count == 0 || pi.Entries[pi.Entries.Count - 1] != id)
                    pi.Entries.Add(id);
            }
        }

        private static void replaceIdsWithPositions(List<int> list, Dictionary<int, int> idToPos)
        {
            for (int i = 0; i != list.Count; ++i)
            {
                int id = list[i];
                int pos = idToPos[id];
                list[i] = pos;
            }
        }

        public void WriteResults(string dictFileName, string statsFolder)
        {
            // Cannot do this twice: we'll have replaced entry IDs with file positions in index
            if (resultsWritten) throw new InvalidOperationException("WriteResults already called.");
            resultsWritten = true;

            // ID to file position
            Dictionary<int, int> idToPos = new Dictionary<int, int>();
            using (BinWriter bw = new BinWriter(dictFileName))
            {
                // Placeholder: will return here to save start position of index at end
                bw.WriteInt(-1);
                // Serialize all entries; fill entry ID -> file pos map
                for (int i = 0; i != entries.Count; ++i)
                {
                    idToPos[i] = bw.Position;
                    entries[i].Serialize(bw);
                }
                // Fill in index start position
                int idxPos = bw.Position;
                bw.Position = 0;
                bw.WriteInt(idxPos);
                bw.Position = idxPos;
                // Replace IDs with file positions across index
                foreach (var x in index.IdeoIndex)
                {
                    replaceIdsWithPositions(x.Value.EntriesHeadwordSimp, idToPos);
                    replaceIdsWithPositions(x.Value.EntriesHeadwordTrad, idToPos);
                    replaceIdsWithPositions(x.Value.EntriesText, idToPos);
                }
                foreach (var x in index.PinyinIndex)
                {
                    replaceIdsWithPositions(x.Value.Entries, idToPos);
                }
                // Serialize index
                index.Serialize(bw);
            }
        }
    }
}
