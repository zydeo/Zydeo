using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using MySql.Data.MySqlClient;

using ZD.Common;
using ZD.CedictEngine;

namespace ZDO.CHSite
{
	public partial class SqlDict
	{
		public class Importer : IDisposable
        {
			/// <summary>
			/// DB connection I'll be using throughout import. Owned.
			/// </summary>
			private MySqlConnection conn;
			/// <summary>
			/// Current DB transaction.
			/// </summary>
            private MySqlTransaction tr;

			// Reused commands
            private MySqlCommand cmdInsBinaryEntry;
            private MySqlCommand cmdInsHanziInstance;
            private MySqlCommand cmdInsPinyinInstance;
			// ---------------

			/// <summary>
			/// See <see cref="LogFileName"/>.
			/// </summary>
            private string logFileName;
			/// <summary>
            /// See <see cref="DroppedFileName"/>.
            /// </summary>
            private string droppedFileName;
			/// <summary>
			/// Log stream.
			/// </summary>
            private StreamWriter swLog;
			/// <summary>
			/// Collects dropped lines.
			/// </summary>
            private StreamWriter swDropped;
			/// <summary>
			/// See <see cref="LineNum"/>.
			/// </summary>
            private int lineNum = 0;

			/// <summary>
			/// Full path to file with import log.
			/// </summary>
			public string LogFileName { get { return logFileName; } }
			/// <summary>
			/// Full path to file with dropped entries (either failed, or duplicates).
			/// </summary>
            public string DroppedFileName { get { return droppedFileName; } }
			/// <summary>
			/// Number of lines processed.
			/// </summary>
            public int LineNum { get { return lineNum; } }

			/// <summary>
			/// Ctor: open DB connnection, create output files etc.
			/// </summary>
			public Importer()
            {
                conn = DB.GetConn();
                tr = conn.BeginTransaction();
                cmdInsBinaryEntry = DB.GetCmd(conn, "InsBinaryEntry");
                cmdInsHanziInstance = DB.GetCmd(conn, "InsHanziInstance");
                cmdInsPinyinInstance = DB.GetCmd(conn, "InsPinyinInstance");

                DateTime dt = DateTime.UtcNow;
                string dtStr = "{0:D4}-{1:D2}-{2:D2}!{3:D2}-{4:D2}-{5:D2}.{6:D3}";
                dtStr = string.Format(dtStr, dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
                logFileName = Path.Combine(Global.WorkFolder, "Import-" + dtStr + "-log.txt");
                droppedFileName = Path.Combine(Global.WorkFolder, "Import-" + dtStr + "-dropped.txt");
                swLog = new StreamWriter(logFileName);
                swDropped = new StreamWriter(droppedFileName);
			}

			/// <summary>
			/// Process one line in the CEDICT format: parse, and store/index in dictionary.
			/// </summary>
			public void AddEntry(string line)
            {
                ++lineNum;
				// Cycle through transactions
				if (lineNum % 5000 == 0)
                {
                    tr.Commit(); tr.Dispose(); tr = null;
                    tr = conn.BeginTransaction();
                }
				// Parse line from CEDICT format
                CedictEntry entry = CedictCompiler.ParseEntry(line, lineNum, swLog, swDropped);
                if (entry == null) return;
				// Check restrictions - can end up dropped entry
                if (!checkRestrictions(entry)) return;
				// TO-DO: check against duplicates
				// Serialize, store in DB
                MemoryStream ms = new MemoryStream();
                BinWriter bw = new BinWriter(ms);
                entry.Serialize(bw);
                cmdInsBinaryEntry.Parameters["@data"].Value = ms.ToArray();
                cmdInsBinaryEntry.ExecuteNonQuery();
                int entryId = (int)cmdInsBinaryEntry.LastInsertedId;
				// Index different parts of the entry
				indexHanzi(entry, entryId);
                indexPinyin(entry, entryId);
            }

			private bool checkRestrictions(CedictEntry entry)
            {
                // TO-DO: max sizes etc.
                return true;
            }

            /// <summary>
            /// Add Pinyin to DB's index/instance tables.
            /// </summary>
            private void indexPinyin(CedictEntry entry, int entryId)
            {
				// Count only one occurrence
                List<PinyinSyllable> uniqueList = new List<PinyinSyllable>();
				foreach (PinyinSyllable ps in entry.Pinyin)
                {
					// Normalize to lower case
                    PinyinSyllable normps = new PinyinSyllable(ps.Text.ToLowerInvariant(), ps.Tone);
					// Add one instance
                    bool onList = false;
					foreach (PinyinSyllable x in uniqueList)
						if (x.Text == normps.Text && x.Tone == normps.Tone)
                        { onList = true; break; }
                    if (!onList) uniqueList.Add(normps);
                }
				// Index each item we have on unique list
                cmdInsPinyinInstance.Parameters["@syll_count"].Value = uniqueList.Count;
                cmdInsPinyinInstance.Parameters["@blob_id"].Value = entryId;
				foreach (PinyinSyllable ps in uniqueList)
                {
                    int hash = CedictEntry.Hash(ps.Text);
                    cmdInsPinyinInstance.Parameters["@pinyin_hash"].Value = hash;
                    cmdInsPinyinInstance.Parameters["@tone"].Value = ps.Tone;
                    cmdInsPinyinInstance.ExecuteNonQuery();
                }
            }

			/// <summary>
			/// Add Hanzi to DB's index/instance tables.
			/// </summary>
			private void indexHanzi(CedictEntry entry, int entryId)
            {
				// Distinct Hanzi in simplified and traditional HW
                HashSet<char> simpSet = new HashSet<char>();
                foreach (char c in entry.ChSimpl) simpSet.Add(c);
                int simpCount = simpSet.Count;
                HashSet<char> tradSet = new HashSet<char>();
                foreach (char c in entry.ChTrad) tradSet.Add(c);
                int tradCount = tradSet.Count;
				// Extract intersection
                HashSet<char> cmnSet = new HashSet<char>();
                List<char> toReam = new List<char>();
				foreach (char c in simpSet)
                {
					if (tradSet.Contains(c))
                    {
                        cmnSet.Add(c);
                        toReam.Add(c);
                    }
                }
                foreach (char c in toReam) { simpSet.Remove(c); tradSet.Remove(c); }
				// Index each Hanzi
                cmdInsHanziInstance.Parameters["@simp_count"].Value = simpCount;
                cmdInsHanziInstance.Parameters["@trad_count"].Value = tradCount;
                cmdInsHanziInstance.Parameters["@blob_id"].Value = entryId;
                foreach (char c in simpSet)
                {
                    cmdInsHanziInstance.Parameters["@hanzi"].Value = (int)c;
                    cmdInsHanziInstance.Parameters["@simptrad"].Value = (byte)1;
                    cmdInsHanziInstance.ExecuteNonQuery();
                }
                foreach (char c in tradSet)
                {
                    cmdInsHanziInstance.Parameters["@hanzi"].Value = (int)c;
                    cmdInsHanziInstance.Parameters["@simptrad"].Value = (byte)2;
                    cmdInsHanziInstance.ExecuteNonQuery();
                }
                foreach (char c in cmnSet)
                {
                    cmdInsHanziInstance.Parameters["@hanzi"].Value = (int)c;
                    cmdInsHanziInstance.Parameters["@simptrad"].Value = (byte)3;
                    cmdInsHanziInstance.ExecuteNonQuery();
                }
            }

			/// <summary>
			/// Finalize pending transaction at the end.
			/// </summary>
			public void CommitRest()
            {
                tr.Commit(); tr.Dispose(); tr = null;
            }

			/// <summary>
			/// Close files, clean up DB resources.
			/// </summary>
            public void Dispose()
            {
                if (swDropped != null) swDropped.Dispose();
                if (swLog != null) swLog.Dispose();

                if (cmdInsPinyinInstance != null) cmdInsPinyinInstance.Dispose();
                if (cmdInsHanziInstance != null) cmdInsHanziInstance.Dispose();
                if (cmdInsBinaryEntry != null) cmdInsBinaryEntry.Dispose();
                if (tr != null) { tr.Rollback(); tr.Dispose(); }
                if (conn != null) conn.Dispose();
            }
        }
	}
}