using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using ZD.Common;

namespace ZD.CedictEngine
{
    partial class DictEngine
    {
        /// <summary>
        /// Used in target lookup, contains occurrence information for a single query token.
        /// </summary>
        private class SenseLookupInfo
        {
            public int NumOfQueryTokensInSense;
            public int TokensInSense;
        }

        /// <summary>
        /// Used in target lookup, contains information about an entry that contains at least one matching sense.
        /// </summary>
        private class EntryMatchInfo
        {
            public int EntryId;
            public float BestSenseScore;
            public List<CedictTargetHighlight> TargetHilites = new List<CedictTargetHighlight>();
        }

        /// <summary>
        /// Retrieves matching entries for a target-language search expression.
        /// </summary>
        private List<CedictResult> doTargetLookup(BinReader br, string query)
        {
            // Empty query string: no results
            query = query.Trim();
            if (query == string.Empty) return new List<CedictResult>();

            // Tokenize query string
            HybridText txtQuery = new HybridText(query);
            ReadOnlyCollection<EquivToken> txtTokenized = tokenizer.Tokenize(txtQuery);
            // Get query string's token IDs
            bool anyUnknown = false;
            HashSet<int> idSet = new HashSet<int>();
            foreach (EquivToken eqt in txtTokenized)
            {
                if (eqt.TokenId == WordHolder.IdUnknown || eqt.TokenId == index.WordHolder.IdZho)
                { anyUnknown = true; break; }
                idSet.Add(eqt.TokenId);
            }
            // Any unknown tokens - no match, we know that immediately
            List<CedictResult> res = new List<CedictResult>();
            if (anyUnknown) return res;
            // Collect IDs of tokenized senses that contain one or more of our query IDs
            Dictionary<int, SenseLookupInfo> senseTokenCounts = new Dictionary<int, SenseLookupInfo>();
            bool firstToken = true;
            // For each token...
            foreach (int tokenId in idSet)
            {
                // Get sense instances where it occurs
                List<SenseInfo> instances = index.SenseIndex[tokenId].GetOrLoadInstances(br);
                foreach (SenseInfo si in instances)
                {
                    SenseLookupInfo sli;
                    // We already have a count for this token ID
                    if (senseTokenCounts.ContainsKey(si.TokenizedSenseId))
                        ++senseTokenCounts[si.TokenizedSenseId].NumOfQueryTokensInSense;
                    // Or this is the first time we're seeing it
                    // We only record counts for the first token
                    // We're looking for senses that contain *all* query tokens
                    else if (firstToken)
                    {
                        sli = new SenseLookupInfo
                        {
                            NumOfQueryTokensInSense = 0,
                            TokensInSense = si.TokensInSense
                        };
                        senseTokenCounts[si.TokenizedSenseId] = sli;
                        ++sli.NumOfQueryTokensInSense;
                    }
                }
                firstToken = false;
            }
            // Keep those sense IDs (positions) that contain all of our query tokens
            // We already eliminated some candidates through "firstToken" trick before, but not all
            List<int> sensePosList = new List<int>();
            foreach (var x in senseTokenCounts)
            {
                if (x.Value.NumOfQueryTokensInSense == idSet.Count)
                    sensePosList.Add(x.Key);
            }
            // Load each tokenized sense to find out:
            // - whether entry is a real match
            // - entry ID
            // - best score for entry (multiple senses may hold query string)
            // - highlights
            Dictionary<int, EntryMatchInfo> entryIdToInfo = new Dictionary<int, EntryMatchInfo>();
            foreach (int senseId in sensePosList)
                doVerifyTarget(txtTokenized, senseId, entryIdToInfo, br);

            // Sort entry IDs by their best score
            List<EntryMatchInfo> entryInfoList = new List<EntryMatchInfo>();
            foreach (var x in entryIdToInfo) entryInfoList.Add(x.Value);
            //entryInfoList.Add(new EntryMatchInfo { EntryId = x.Key, BestSenseScore = x.Value.BestSenseScore });
            entryInfoList.Sort((a, b) => b.BestSenseScore.CompareTo(a.BestSenseScore));
            // Load entries, wrap into results
            foreach (EntryMatchInfo emi in entryInfoList)
            {
                CedictResult cr = new CedictResult(emi.EntryId,
                    new ReadOnlyCollection<CedictTargetHighlight>(emi.TargetHilites));
                res.Add(cr);
            }
            return res;
        }

        /// <summary>
        /// <para>Verifies if a sense that contains all query tokens is really a match.</para>
        /// </summary>
        /// <param name="txtTokenized">The tokenized query text.</param>
        /// <param name="sensePos">The data position of the tokenized sense to verify.</param>
        /// <param name="entryIdToInfo">Container for kept entry matches.</param>
        /// <param name="br">Binary data source to read up tokenized sense.</param>
        private void doVerifyTarget(ReadOnlyCollection<EquivToken> txtTokenized,
            int sensePos,
            Dictionary<int, EntryMatchInfo> entryIdToInfo,
            BinReader br)
        {
            // Load tokenized sense
            br.Position = sensePos;
            TokenizedSense ts = new TokenizedSense(br);
            // Find query tokens in tokenized sense
            // This will be our highlight too!
            CedictTargetHighlight hilite = doFindTargetQuery(txtTokenized, ts);
            // No highlight: no match
            if (hilite == null) return;
            // Score is length of query (in tokens) divided by count of tokense in sense
            float score = ((float)txtTokenized.Count) / ((float)ts.EquivTokens.Count);
            // If we found query string, it's a match; we can go on and record best score and hilight
            if (!entryIdToInfo.ContainsKey(ts.EntryId))
            {
                EntryMatchInfo emi = new EntryMatchInfo
                {
                    EntryId = ts.EntryId,
                    BestSenseScore = score,
                };
                emi.TargetHilites.Add(hilite);
                entryIdToInfo[ts.EntryId] = emi;
            }
            else
            {
                EntryMatchInfo emi = entryIdToInfo[ts.EntryId];
                if (score > emi.BestSenseScore)
                    emi.BestSenseScore = score;
                emi.TargetHilites.Add(hilite);
            }
        }

        /// <summary>
        /// <para>Looks for query text in tokenized sense, returns corresponding target highlight if found.</para>
        /// <para>If not found (sense doesn't contain query as a sequence), returns null.</para>
        /// </summary>
        private CedictTargetHighlight doFindTargetQuery(ReadOnlyCollection<EquivToken> txtTokenized,
            TokenizedSense ts)
        {
            for (int i = 0; i <= ts.EquivTokens.Count - txtTokenized.Count; ++i)
            {
                int j = 0;
                for (; j != txtTokenized.Count; ++j)
                {
                    if (txtTokenized[j].TokenId != ts.EquivTokens[i + j].TokenId)
                        break;
                }
                // If we found full query text: create highlight now
                if (j == txtTokenized.Count)
                {
                    // Query is a single token
                    if (txtTokenized.Count == 1)
                    {
                        return new CedictTargetHighlight(ts.SenseIx, ts.EquivTokens[i].RunIx,
                            ts.EquivTokens[i].StartInRun, ts.EquivTokens[i].LengthInRun);
                    }
                    // Query is multiple tokens
                    else
                    {
                        // Sanity check: all tokens in tokenized sense must be from same text run
                        // We don't even index across multiple runs
                        // And definitely don't look up queries that have Hanzi in the middle
                        if (ts.EquivTokens[i].RunIx != ts.EquivTokens[i + j - 1].RunIx)
                            throw new Exception("Entire query string should be within a single text run in sense's equiv.");
                        int hlStart = ts.EquivTokens[i].StartInRun;
                        int hlEnd = ts.EquivTokens[i + j - 1].StartInRun + ts.EquivTokens[i + j - 1].LengthInRun;
                        return new CedictTargetHighlight(ts.SenseIx, ts.EquivTokens[i].RunIx,
                            hlStart, hlEnd - hlStart);
                    }
                }
            }
            // Sequence not found
            return null;
        }
    }
}
