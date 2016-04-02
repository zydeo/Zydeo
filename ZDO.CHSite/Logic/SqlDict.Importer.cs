using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
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
            private MySqlCommand cmdInsEntry;
            private MySqlCommand cmdInsModif;
            private MySqlCommand cmdUpdLastModif;
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

            // DBG
            private readonly bool prmIndex;
            private readonly bool prmPopulate;

			/// <summary>
			/// Ctor: open DB connnection, create output files etc.
			/// </summary>
			public Importer(bool doIndex, bool doPopulate)
            {
                this.prmIndex = doIndex;
                this.prmPopulate = doPopulate;

                conn = DB.GetConn();
                tr = conn.BeginTransaction();
                cmdInsBinaryEntry = DB.GetCmd(conn, "InsBinaryEntry");
                cmdInsHanziInstance = DB.GetCmd(conn, "InsHanziInstance");
                cmdInsPinyinInstance = DB.GetCmd(conn, "InsPinyinInstance");
                cmdInsEntry = DB.GetCmd(conn, "InsEntry");
                cmdInsModif = DB.GetCmd(conn, "InsModif");
                cmdUpdLastModif = DB.GetCmd(conn, "UpdLastModif");

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
				if (lineNum % 3000 == 0)
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

				// Serialize, store in DB, index
                int binId = 0;
                if (prmIndex) binId = doIndex(entry);
                // Populate entries table
                int entryId = 0;
                if (prmPopulate) entryId = doPopulate(entry, binId);
            }

            private int doPopulate(CedictEntry entry, int binId)
            {
                StringBuilder sbHead = new StringBuilder();
                StringBuilder sbTrg = new StringBuilder();
                sbHead.Append(entry.ChTrad);
                sbHead.Append(' ');
                sbHead.Append(entry.ChSimpl);
                sbHead.Append(" [");
                bool first = true;
                foreach (var py in entry.Pinyin)
                {
                    if (!first) sbHead.Append(' ');
                    else first = false;
                    sbHead.Append(py.GetDisplayString(false));
                }
                sbHead.Append(']');
                string strHead = sbHead.ToString();
                int hashSimp = CedictEntry.Hash(entry.ChSimpl);
                foreach (var sense in entry.Senses)
                {
                    sbTrg.Append('/');
                    sbTrg.Append(sense.GetPlainText());
                }
                sbTrg.Append('/');
                string strTrg = sbTrg.ToString();
                cmdInsEntry.Parameters["@hw"].Value = strHead;
                cmdInsEntry.Parameters["@trg"].Value = strTrg;
                cmdInsEntry.Parameters["@simp_hash"].Value = hashSimp;
                cmdInsEntry.Parameters["@status"].Value = 0;
                cmdInsEntry.Parameters["@deleted"].Value = 0;
                cmdInsEntry.Parameters["@bin_id"].Value = binId;
                cmdInsEntry.ExecuteNonQuery();
                int entryId = (int)cmdInsEntry.LastInsertedId;

                int lastModifId = doAddFiveChanges(entryId);

                return entryId;
            }

            private int doAddFiveChanges(int entryId)
            {
                DateTime utcNow = DateTime.UtcNow;
                cmdInsModif.Parameters["@hw_before"].Value = null;
                cmdInsModif.Parameters["@trg_before"].Value = null;
                cmdInsModif.Parameters["@timestamp"].Value = utcNow;
                cmdInsModif.Parameters["@user_id"].Value = 0;
                cmdInsModif.Parameters["@note"].Value = "Yes I wrote a note. This note belongs to this change.";
                cmdInsModif.Parameters["@chg"].Value = 0;
                cmdInsModif.Parameters["@entry_id"].Value = entryId;
                cmdInsModif.ExecuteNonQuery();

                cmdInsModif.Parameters["@hw_before"].Value = "你好 你好 [ni3 hao3]";
                cmdInsModif.Parameters["@trg_before"].Value = "this was the entry's previous translation";
                cmdInsModif.Parameters["@chg"].Value = 3;
                for (int i = 0; i != 4; ++i)
                {
                    cmdInsModif.ExecuteNonQuery();
                }
                int modifId = (int)cmdInsModif.LastInsertedId;
                cmdUpdLastModif.Parameters["@entry_id"].Value = entryId;
                cmdUpdLastModif.Parameters["@last_modif_id"].Value = modifId;
                cmdUpdLastModif.ExecuteNonQuery();
                return modifId;
            }

            private int doIndex(CedictEntry entry)
            {
                MemoryStream ms = new MemoryStream();
                BinWriter bw = new BinWriter(ms);
                entry.Serialize(bw);
                cmdInsBinaryEntry.Parameters["@data"].Value = ms.ToArray();
                cmdInsBinaryEntry.ExecuteNonQuery();
                int binId = (int)cmdInsBinaryEntry.LastInsertedId;
                // Index different parts of the entry
                indexHanzi(entry, binId);
                indexPinyin(entry, binId);
                return binId;
            }

			private bool checkRestrictions(CedictEntry entry)
            {
                // TO-DO: max sizes etc.
                if (entry.ChSimpl.Length > 16) return false;
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

                if (cmdUpdLastModif != null) cmdUpdLastModif.Dispose();
                if (cmdInsModif != null) cmdInsModif.Dispose();
                if (cmdInsEntry != null) cmdInsEntry.Dispose();
                if (cmdInsPinyinInstance != null) cmdInsPinyinInstance.Dispose();
                if (cmdInsHanziInstance != null) cmdInsHanziInstance.Dispose();
                if (cmdInsBinaryEntry != null) cmdInsBinaryEntry.Dispose();
                if (tr != null) { tr.Rollback(); tr.Dispose(); }
                if (conn != null) conn.Dispose();
            }
        }
	}
}