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
    public partial class DictData
    {
        private string sessionStartTime = null;

        public readonly int SessionId;

        private readonly HwCollection hwColl;

        public HwCollection Headwords
        {
            get { return hwColl; }
        }

        /// <summary>
        /// Reads backbone data from "backbone.xml" and initializes database.
        /// </summary>
        public static DictData InitFromXml(string xmlFileName, string dbFileName)
        {
            return new DictData(xmlFileName, dbFileName);
        }

        /// <summary>
        /// Reads headwords from database.
        /// </summary>
        public static DictData InitFromDB(string dbFileName)
        {
            return new DictData(dbFileName);
        }

        private static string sqlSelBackbone =
            @"SELECT info
            FROM [backbone]
            WHERE heads_id=@heads_id;";

        public BackboneEntry GetBackbone(int id)
        {
            SQLiteConnection conn = null;
            SQLiteCommand cmdSelBackbone = null;
            SQLiteDataReader rdr = null;
            try
            {
                conn = new SQLiteConnection(connString);
                conn.Open();
                cmdSelBackbone = new SQLiteCommand(sqlSelBackbone, conn);
                cmdSelBackbone.Parameters.AddWithValue("@heads_id", id);
                rdr = cmdSelBackbone.ExecuteReader();
                rdr.Read();
                string bbXml = (string)rdr["info"];
                return BackboneEntry.ReadFromXmlStr(bbXml);
            }
            finally
            {
                if (rdr != null)
                {
                    if (!rdr.IsClosed) rdr.Close();
                    rdr.Dispose();
                }
                if (cmdSelBackbone != null) cmdSelBackbone.Dispose();
                if (conn != null)
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                    conn.Dispose();
                }
            }
        }

        private static string sqlSelSenses =
            @"SELECT senses
            FROM [dict]
            WHERE heads_id=@heads_id;";

        public string GetSenses(int id)
        {
            SQLiteConnection conn = null;
            SQLiteCommand cmdSelEntry = null;
            SQLiteDataReader rdr = null;
            try
            {
                conn = new SQLiteConnection(connString);
                conn.Open();
                cmdSelEntry = new SQLiteCommand(sqlSelSenses, conn);
                cmdSelEntry.Parameters.AddWithValue("@heads_id", id);
                rdr = cmdSelEntry.ExecuteReader();
                if (rdr.Read()) return (string)rdr["senses"];
                else return string.Empty;
            }
            finally
            {
                if (rdr != null)
                {
                    if (!rdr.IsClosed) rdr.Close();
                    rdr.Dispose();
                }
                if (cmdSelEntry != null) cmdSelEntry.Dispose();
                if (conn != null)
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                    conn.Dispose();
                }
            }
        }

        private static string sqlUpdateSenses =
            @"UPDATE [dict]
            SET senses=:senses
            WHERE heads_id=@heads_id;";

        private static string sqlInsertSenses =
            @"INSERT INTO [dict] (heads_id, senses)
            VALUES (@heads_id, @senses)";

        private static string sqlUpdateHead =
            @"UPDATE [heads]
            SET extract=:extract, status=:status
            WHERE id=@id;";

        private HwData getHwDataById(int id)
        {
            foreach (HwData hwd in hwColl)
                if (hwd.Id == id) return hwd;
            return null;
        }

        public void SaveSenses(int id, string strSenses, HwStatus status, DateTime dtEditStart, DateTime dtNow)
        {
            SQLiteConnection conn = null;
            SQLiteCommand cmdUpdateSenses = null;
            SQLiteCommand cmdInsertSenses = null;
            SQLiteCommand cmdUpdateHead = null;
            try
            {
                conn = new SQLiteConnection(connString);
                conn.Open();
                cmdUpdateSenses = new SQLiteCommand(sqlUpdateSenses, conn);
                cmdUpdateSenses.Parameters.AddWithValue("senses", strSenses);
                cmdUpdateSenses.Parameters.AddWithValue("@heads_id", id);
                if (cmdUpdateSenses.ExecuteNonQuery() == 0)
                {
                    cmdInsertSenses = new SQLiteCommand(sqlInsertSenses, conn);
                    cmdInsertSenses.Parameters.AddWithValue("@heads_id", id);
                    cmdInsertSenses.Parameters.AddWithValue("@senses", strSenses);
                    cmdInsertSenses.ExecuteNonQuery();
                }
                string newExtract = strSenses.Length > 256 ? strSenses.Substring(0, 256) : strSenses;
                HwData data = getHwDataById(id);
                data.Extract = newExtract;
                data.Status = status;
                cmdUpdateHead = new SQLiteCommand(sqlUpdateHead, conn);
                cmdUpdateHead.Parameters.AddWithValue("extract", newExtract);
                cmdUpdateHead.Parameters.AddWithValue("status", status);
                cmdUpdateHead.Parameters.AddWithValue("@id", id);
                cmdUpdateHead.ExecuteNonQuery();

                // Log editing and session time
                logHwTime(id, dtEditStart, dtNow);
            }
            finally
            {
                if (cmdUpdateHead != null) cmdUpdateHead.Dispose();
                if (cmdInsertSenses != null) cmdInsertSenses.Dispose();
                if (cmdUpdateSenses != null) cmdUpdateSenses.Dispose();
                if (conn != null)
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                    conn.Dispose();
                }
            }
        }

        private static string dtToString(DateTime dt)
        {
            // 2014-01-14!17:33:49.1
            string str = "{0:D4}-{1:D2}-{2:D2}!{3:D2}:{4:D2}:{5:D2}.{6}";
            str = string.Format(str, dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond / 100);
            return str;
        }

        private static string sqlUpdateSessionTime =
            @"UPDATE [session_times]
            SET end_time=:end_time
            WHERE id=@id;";

        private static string sqlInsertSessionTime =
            @"INSERT INTO [session_times] (id, start_time, end_time)
            VALUES (@id, @start_time, @end_time)";

        private static string sqlInsertEntryTime =
            @"INSERT INTO [hw_times] (session_id, heads_id, start_time, end_time)
            VALUES (@session_id, @heads_id, @start_time, @end_time)";

        private void logHwTime(int id, DateTime dtEditStart, DateTime dtNow)
        {
            string strEditStart = dtToString(dtEditStart);
            string strNow = dtToString(dtNow);

            SQLiteConnection conn = null;
            SQLiteCommand cmdUpdateSessionTime = null;
            SQLiteCommand cmdInsertSessionTime = null;
            SQLiteCommand cmdInsertEntryTime = null;
            try
            {
                conn = new SQLiteConnection(connString);
                conn.Open();
                // Not the first time saving something in this session
                if (sessionStartTime != null)
                {
                    cmdUpdateSessionTime = new SQLiteCommand(sqlUpdateSessionTime, conn);
                    cmdUpdateSessionTime.Parameters.AddWithValue("end_time", strNow);
                    cmdUpdateSessionTime.Parameters.AddWithValue("@id", SessionId);
                    if (cmdUpdateSessionTime.ExecuteNonQuery() != 1)
                        throw new Exception("Failed to update session time log.");
                }
                // First time in this session
                else
                {
                    sessionStartTime = strEditStart;
                    cmdInsertSessionTime = new SQLiteCommand(sqlInsertSessionTime, conn);
                    cmdInsertSessionTime.Parameters.AddWithValue("@id", SessionId);
                    cmdInsertSessionTime.Parameters.AddWithValue("@start_time", sessionStartTime);
                    cmdInsertSessionTime.Parameters.AddWithValue("@end_time", strNow);
                    if (cmdInsertSessionTime.ExecuteNonQuery() != 1)
                        throw new Exception("Failed to start logging session's time.");
                }
                // Log time to entry
                cmdInsertEntryTime = new SQLiteCommand(sqlInsertEntryTime, conn);
                cmdInsertEntryTime.Parameters.AddWithValue("@session_id", SessionId);
                cmdInsertEntryTime.Parameters.AddWithValue("@heads_id", id);
                cmdInsertEntryTime.Parameters.AddWithValue("@start_time", strEditStart);
                cmdInsertEntryTime.Parameters.AddWithValue("@end_time", strNow);
                if (cmdInsertEntryTime.ExecuteNonQuery() != 1)
                    throw new Exception("Failed to update entry time log.");
            }
            finally
            {
                if (cmdUpdateSessionTime != null) cmdUpdateSessionTime.Dispose();
                if (cmdInsertSessionTime != null) cmdInsertSessionTime.Dispose();
                if (cmdInsertEntryTime != null) cmdInsertEntryTime.Dispose();
                if (conn != null)
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                    conn.Dispose();
                }
            }
        }
    }
}
