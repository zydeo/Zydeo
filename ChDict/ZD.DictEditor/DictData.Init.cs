using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Data;
using System.Data.SQLite;

using ZD.ChDict.Common;

namespace ZD.DictEditor
{
    partial class DictData
    {
        private readonly string connString;

        private string buildConnString(string dbFileName, bool failIfMissing)
        {
            SQLiteConnectionStringBuilder scsb = new SQLiteConnectionStringBuilder();
            scsb.DataSource = dbFileName;
            scsb.DateTimeFormat = SQLiteDateFormats.ISO8601;
            scsb.FailIfMissing = failIfMissing;
            return scsb.ConnectionString;
        }

        private static string sqlCreateTblHeads =
            @"CREATE TABLE [heads] (
            [id] INTEGER NOT NULL PRIMARY KEY,
            [status] INTEGER NOT NULL,
            [simp] VARCHAR(32) NOT NULL,
            [trad] VARCHAR(32) NOT NULL,
            [pinyin] VARCHAR(256) NOT NULL,
            [extract] VARCHAR(256) NOT NULL
            );";

        private static string sqlCreateTblBackbone =
            @"CREATE TABLE [backbone] (
            [heads_id] INTEGER NOT NULL,
            [info] VARCHAR(8192) NOT NULL
            );";

        private static string sqlCreateTblDict =
            @"CREATE TABLE [dict] (
            [heads_id] INTEGER NOT NULL,
            [senses] VARCHAR(1024) NOT NULL
            );";

        private void createDB()
        {
            SQLiteConnection conn = null;
            SQLiteCommand cmd = null;
            try
            {
                conn = new SQLiteConnection(connString);
                conn.Open();
                cmd = new SQLiteCommand(sqlCreateTblHeads, conn);
                cmd.ExecuteNonQuery(); cmd.Dispose(); cmd = null;
                cmd = new SQLiteCommand(sqlCreateTblBackbone, conn);
                cmd.ExecuteNonQuery(); cmd.Dispose(); cmd = null;
                cmd = new SQLiteCommand(sqlCreateTblDict, conn);
                cmd.ExecuteNonQuery(); cmd.Dispose(); cmd = null;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (conn != null)
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                    conn.Dispose();
                }
            }
        }

        private static string sqlInsHead =
            @"INSERT INTO [heads] (id, status, simp, trad, pinyin, extract)
            VALUES(@id, @status, @simp, @trad, @pinyin, @extract);";

        private static string sqlInsBackbone =
            @"INSERT INTO [backbone] (heads_id, info)
            VALUES(@heads_id, @info);";

        /// <summary>
        /// Reads backbone data from XML file and initializes database.
        /// </summary>
        private DictData(string xmlFileName, string dbFileName)
        {
            // SQL connection, DB
            if (File.Exists(dbFileName))
            {
                // TO-DO: TMP
                File.Delete(dbFileName);
                //throw new Exception("Database already exists");
            }
            connString = buildConnString(dbFileName, false);
            createDB();

            // Read data from backbone; keep in memory, store in DB
            SQLiteConnection conn = null;
            SQLiteCommand cmdInsHead = null;
            SQLiteCommand cmdInsBackbone = null;
            SQLiteTransaction tr = null;
            try
            {
                // DB stuff
                conn = new SQLiteConnection(connString);
                conn.Open();
                cmdInsHead = new SQLiteCommand(sqlInsHead, conn);
                cmdInsBackbone = new SQLiteCommand(sqlInsBackbone, conn);
                tr = conn.BeginTransaction();
                // Process input
                List<HwData> hwList = new List<HwData>();
                int id = 0;
                using (StreamReader sr = new StreamReader(xmlFileName))
                using (XmlTextReader xr = new XmlTextReader(sr))
                {
                    while (true)
                    {
                        if (xr.NodeType != XmlNodeType.Element) { xr.Read(); continue; }
                        if (xr.Name != "backbone") continue;
                        break;
                    }
                    xr.Read();
                    while (xr.NodeType != XmlNodeType.Element) xr.Read();
                    BackboneEntry be;
                    while ((be = BackboneEntry.ReadFromXml(xr)) != null)
                    {
                        // Headword stays in collection
                        HwData hwd = new HwData(id, HwStatus.NotStarted, be.Simp, be.Trad, be.Pinyin, string.Empty);
                        hwList.Add(hwd);
                        // Headword record in DB
                        cmdInsHead.Parameters.Clear();
                        cmdInsHead.Parameters.AddWithValue("@id", id);
                        cmdInsHead.Parameters.AddWithValue("@status", (int)HwStatus.NotStarted);
                        cmdInsHead.Parameters.AddWithValue("@simp", be.Simp);
                        cmdInsHead.Parameters.AddWithValue("@trad", be.Trad);
                        cmdInsHead.Parameters.AddWithValue("@pinyin", be.Pinyin);
                        cmdInsHead.Parameters.AddWithValue("@extract", string.Empty);
                        cmdInsHead.ExecuteNonQuery();
                        // Backbone in DB: re-serialized XML
                        string bbXml = be.WriteToXmlStr();
                        cmdInsBackbone.Parameters.Clear();
                        cmdInsBackbone.Parameters.AddWithValue("@heads_id", id);
                        cmdInsBackbone.Parameters.AddWithValue("@info", bbXml);
                        cmdInsBackbone.ExecuteNonQuery();

                        // Move on
                        id += 10;
                    }
                }
                hwColl = new HwCollection(new ReadOnlyCollection<HwData>(hwList));
                tr.Commit(); tr.Dispose(); tr = null;
            }
            finally
            {
                if (tr != null) { tr.Dispose(); }
                if (cmdInsBackbone != null) cmdInsBackbone.Dispose();
                if (cmdInsHead != null) cmdInsHead.Dispose();
                if (conn != null)
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                    conn.Dispose();
                }
            }
        }

        private static string sqlSelHeads =
            @"SELECT id, status, simp, trad, pinyin, extract
            FROM [heads];";

        /// <summary>
        /// Reads headwords from database.
        /// </summary>
        private DictData(string dbFileName)
        {
            List<HwData> hwList = new List<HwData>();
            connString = buildConnString(dbFileName, true);
            SQLiteConnection conn = null;
            SQLiteCommand cmdSelHeads = null;
            SQLiteDataReader rdr = null;
            try
            {
                conn = new SQLiteConnection(connString);
                conn.Open();
                cmdSelHeads = new SQLiteCommand(sqlSelHeads, conn);
                rdr = cmdSelHeads.ExecuteReader();
                while (rdr.Read())
                {
                    int id = (int)(long)(rdr["id"]);
                    HwStatus status = (HwStatus)(long)(rdr["status"]);
                    string simp = (string)rdr["simp"];
                    string trad = (string)rdr["trad"];
                    string pinyin = (string)rdr["pinyin"];
                    string extract = (string)rdr["extract"];
                    HwData hwd = new HwData(id, status, simp, trad, pinyin, extract);
                    hwList.Add(hwd);
                }
                rdr.Close(); rdr.Dispose(); rdr = null;
                hwColl = new HwCollection(new ReadOnlyCollection<HwData>(hwList));
            }
            finally
            {
                if (rdr != null)
                {
                    if (!rdr.IsClosed) rdr.Close();
                    rdr.Dispose();
                }
                if (cmdSelHeads != null) cmdSelHeads.Dispose();
                if (conn != null)
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                    conn.Dispose();
                }
            }
        }
    }
}
