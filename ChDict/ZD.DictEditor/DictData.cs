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

        public void SaveSenses(int id, string strSenses, HwStatus status)
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
    }
}
