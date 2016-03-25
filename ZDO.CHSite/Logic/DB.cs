using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Reflection;
using MySql.Data.MySqlClient;

using ZD.CedictEngine;
using ZD.Common;

namespace ZDO.CHSite
{
    public class DB
    {
        /// <summary>
        /// One pre-processed script from DB.Scripts.txt.
        /// </summary>
        private class Command
        {
            /// <summary>
            /// SQL command.
            /// </summary>
            public string Sql;
            /// <summary>
            /// The command's parameters and their types.
            /// </summary>
            public Dictionary<string, MySqlDbType> Params = new Dictionary<string, MySqlDbType>();
        }

        /// <summary>
        /// Pre-processed scripts.
        /// </summary>
        private static readonly Dictionary<string, Command> cmdDict = new Dictionary<string, Command>();

        /// <summary>
        /// Connection string for new connections.
        /// </summary>
        private static readonly string connectionString;

        /// <summary>
        /// Pre-processes scripts from DB.Scripts.txt.
        /// Builds and stores connection string from site config.
        /// </summary>
        static DB()
        {
            // Build connection string.
            MySqlConnectionStringBuilder csb = new MySqlConnectionStringBuilder();
            csb.Server = "localhost";
            csb.Port = 3306;
            csb.Database = "chdb";
            csb.UserID = "chdb_user";
            csb.Password = "pass";
            csb.Pooling = true;
            csb.IgnorePrepare = false;
            connectionString = csb.GetConnectionString(true);

            // Parse embedded resource with scipts.
            Command cmd = null;
            string cmdName = null;
            Assembly a = Assembly.GetExecutingAssembly();
            string fileName = "ZDO.CHSite.Logic.DB.Scripts.txt";
            using (Stream s = a.GetManifestResourceStream(fileName))
            using (StreamReader sr = new StreamReader(s))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("# Script"))
                    {
                        cmdName = line.Substring(9);
                        cmd = new Command();
                    }
                    else if (line.StartsWith("# @"))
                    {
                        string[] parts = line.Substring(2).Split(' ');
                        MySqlDbType dbType;
                        if (parts[1] == "BLOB") dbType = MySqlDbType.Blob;
                        else if (parts[1] == "TINYINT") dbType = MySqlDbType.Byte;
                        else dbType = MySqlDbType.Int32;
                        cmd.Params[parts[0]] = dbType;
                    }
                    else if (line.StartsWith("# End"))
                    {
                        cmdDict[cmdName] = cmd;
                        cmd = null;
                        cmdName = null;
                    }
                    else if (!line.StartsWith("#"))
                    {
                        if (cmd != null) cmd.Sql += line + "\r\n";
                    }
                }
            }
        }

        /// <summary>
        /// Opens a DB connection.
        /// </summary>
        /// <returns></returns>
        public static MySqlConnection GetConn()
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            try { conn.Open(); }
            catch { conn.Dispose(); throw; }
            return conn;
        }

        /// <summary>
        /// Gets a command by instantiating a script from DB.Scripts.txt.
        /// </summary>
        public static MySqlCommand GetCmd(MySqlConnection conn, string cmdName)
        {
            Command cmd = cmdDict[cmdName];
            MySqlCommand res = new MySqlCommand(cmd.Sql, conn);
            foreach (var x in cmd.Params) res.Parameters.Add(x.Key, x.Value);
            if (res.Parameters.Count != 0) res.Prepare();
            return res;
        }

        /// <summary>
        /// Creates tables in the DB.
        /// </summary>
        public static void CreateTables()
        {
            MySqlConnection conn = null;
            MySqlCommand cmd = null;
            try
            {
                conn = GetConn();
                cmd = GetCmd(conn, "CreateDB");
                cmd.ExecuteNonQuery();
                cmd.Dispose(); cmd = null;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }
    }
}