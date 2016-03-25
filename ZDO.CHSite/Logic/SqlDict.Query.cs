using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;

using ZD.Common;
using ZD.CedictEngine;

namespace ZDO.CHSite
{
    public partial class SqlDict
    {
        public class Query : IDisposable
        {
            /// <summary>
            /// DB connection I'll be using throughout lookup. Owned.
            /// </summary>
            private MySqlConnection conn;
            /// <summary>
            /// Transaction: upheld throughout lookup to ensure consistency across queries.
            /// </summary>
            private MySqlTransaction tr;

            // Reused commands
            // ---------------

            private class EntryProvider : ICedictEntryProvider
            {
                private readonly Dictionary<int, CedictEntry> entryDict = new Dictionary<int, CedictEntry>();

                public void AddEntry(int entryId, CedictEntry entry)
                {
                    entryDict[entryId] = entry;
                }

                public CedictEntry GetEntry(int entryId)
                {
                    return entryDict[entryId];
                }

                public void Dispose() { }
            }

            private static void log(string msg)
            {
                string line = "{0}:{1}:{2}.{3:D3} ";
                DateTime d = DateTime.Now;
                line = string.Format(line, d.Hour, d.Minute, d.Second, d.Millisecond);
                //DiagLogger.LogError(line + msg);
                //System.Diagnostics.Debug.WriteLine(line + msg);
            }

            public Query()
            {
                conn = DB.GetConn();
                tr = conn.BeginTransaction(IsolationLevel.Serializable);
            }

            private static void interpretPinyin(string query, out List<PinyinSyllable> qsylls,
                out List<PinyinSyllable> qnorm)
            {
                qsylls = DictEngine.ParsePinyinQuery(query);
                // Get every syllable once - we ignore repeats
                // If a syllable occurs with unspecified tone once, or if it occurs with multiple tone marks
                // -> We only take it as one item with unspecified tone
                // Otherwise, take it as is, with tone mark
                Dictionary<string, int> syllDict = new Dictionary<string, int>();
                foreach (var syll in qsylls)
                {
                    if (!syllDict.ContainsKey(syll.Text)) syllDict[syll.Text] = syll.Tone;
                    else if (syllDict[syll.Text] != syll.Tone) syllDict[syll.Text] = -1;
                }
                qnorm = new List<PinyinSyllable>();
                foreach (var x in syllDict)
                    qnorm.Add(new PinyinSyllable(x.Key, x.Value));
            }

            private Dictionary<string, HashSet<int>> getPinyinCandidates(List<PinyinSyllable> sylls)
            {
                // Prepare
                Dictionary<string, HashSet<int>> res = new Dictionary<string, HashSet<int>>();
                Dictionary<int, string> hashToText = new Dictionary<int, string>();

                // Build custom, single query to get *all* instance that
                // are relevant for any requested syllable
                // Also initialize result dictionary with pinyin keys
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT pinyin_hash, tone, syll_count, blob_id FROM pinyin_instances WHERE");
                bool first = true;
                foreach (PinyinSyllable syll in sylls)
                {
                    // Init dictionary
                    string key = syll.Tone == -1 ? syll.Text : syll.GetDisplayString(false);
                    res[key] = new HashSet<int>();
                    hashToText[CedictEntry.Hash(syll.Text)] = syll.Text;
                    // Build our custom query
                    if (!first) sb.Append(" OR");
                    else first = false;
                    sb.Append(" (pinyin_hash=");
                    sb.Append(CedictEntry.Hash(syll.Text).ToString());
                    if (syll.Tone != -1)
                    {
                        sb.Append(" AND tone=");
                        sb.Append(syll.Tone.ToString());
                    }
                    sb.Append(")");
                }
                sb.Append(";");
                // Compile and execute SQL command
                using (MySqlCommand cmd = new MySqlCommand(sb.ToString(), conn))
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        // Which query syllable is this for?
                        // With or without tone mark.
                        HashSet<int> cands = null;
                        string text = hashToText[rdr.GetInt32(0)];
                        if (res.ContainsKey(text)) cands = res[text];
                        else
                        {
                            text += rdr.GetInt32(1).ToString();
                            cands = res[text];
                        }
                        // Store blob ID
                        cands.Add(rdr.GetInt32(3));
                    }
                }

                // Done
                return res;
            }

            private List<int> intersectCandidates(Dictionary<string, HashSet<int>> candsBySyll)
            {
                List<int> res = new List<int>();
                if (candsBySyll.Count == 0) return res;
                if (candsBySyll.Count == 1)
                {
                    foreach (var x in candsBySyll)
                        res.AddRange(x.Value);
                    return res;
                }
                // Put hash sets in an array; shorter ones first
                HashSet<int>[] sets = new HashSet<int>[candsBySyll.Count];
                int pos = 0;
                foreach (var x in candsBySyll)
                {
                    sets[pos] = x.Value;
                    ++pos;
                }
                Array.Sort(sets, (x, y) => x.Count.CompareTo(y.Count));
                // Look for intersection from left (shorter) to right (longer)
                foreach (int id in sets[0])
                {
                    bool failed = false;
                    for (int i = 1; i < sets.Length; ++i)
                    {
                        if (!sets[i].Contains(id)) { failed = true; break; }
                    }
                    if (!failed) res.Add(id);
                }
                return res;
            }

            public CedictLookupResult Lookup(string query, SearchScript script, SearchLang lang)
            {
                // Prepare
                EntryProvider ep = new EntryProvider();
                List<CedictResult> res = new List<CedictResult>();
                List<CedictAnnotation> anns = new List<CedictAnnotation>();

                log("Begin retrieve candidate IDs for " + query);
                // Interpret query string
                List<PinyinSyllable> qsylls, qnorm;
                interpretPinyin(query, out qsylls, out qnorm);
                // Get instance vectors
                Dictionary<string, HashSet<int>> candsBySyll = getPinyinCandidates(qnorm);
                log("Begin intersecting candidates");
                // Intersect candidates
                List<int> cands = intersectCandidates(candsBySyll);
                log("Begin retrieve and verify candidates (" + cands.Count + ")");
                // Retrieve all candidates; verify on the fly
                //retrieveVerifyPinyin(cands, qsylls, ep);
                log("Done");

                // Done
                return new CedictLookupResult(ep, query, res, anns, lang);
            }

            public void Dispose()
            {
                if (tr != null) tr.Dispose();
                if (conn != null) conn.Dispose();
            }
        }
    }
}