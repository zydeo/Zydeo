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
        public class ChangeItem
        {
            public DateTime When;
            public int ChangeId;
            public string Head;
        }

        public class History : IDisposable
        {
            private MySqlConnection conn;

            // Reused commands
            private MySqlCommand cmdSelChangePage;
            // ---------------

            public History()
            {
                conn = DB.GetConn();
                cmdSelChangePage = DB.GetCmd(conn, "SelModifPage");

            }

            public List<ChangeItem> GetChangePage(int pageStart, int pageLen)
            {
                List<ChangeItem> res = new List<ChangeItem>();
				cmdSelChangePage.Parameters["@page_start"].Value = pageStart;
                cmdSelChangePage.Parameters["@page_len"].Value = pageLen;
				using (MySqlDataReader rdr = cmdSelChangePage.ExecuteReader())
                {
					while (rdr.Read())
                    {
                        ChangeItem ci = new ChangeItem
                        {
                            When = rdr.GetDateTime(0),
                            ChangeId = rdr.GetInt32(1),
                            Head = rdr.GetString(2)
                        };
                        res.Add(ci);
                    }
                }
                return res;
            }

            public void Dispose()
            {
                if (cmdSelChangePage != null) cmdSelChangePage.Dispose();
                if (conn != null) conn.Dispose();
            }
        }
    }
}