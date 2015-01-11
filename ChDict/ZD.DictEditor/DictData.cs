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
    }
}
