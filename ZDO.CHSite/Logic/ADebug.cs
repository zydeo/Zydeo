using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Threading;

using ZD.Common;

namespace ZDO.CHSite
{
//#if DEBUG
    [ActionName("debug")]
    public class ADebug : ApiAction
    {
        public ADebug(HttpContext ctxt) : base(ctxt) { }

        public override void Process()
        {
            string what = Req.Params["what"];
            if (what == "recreate_db")
            {
                DB.CreateTables();
            }
            else if (what == "index_hdd") doIndexHDD();
            else if (what == "progress_index_hdd") doProgressIndexHDD();
            else if (what == "query_page") doQueryPage();
        }

        #region Query history page

        [DataContract]
        public class HistoryItem
        {
            [DataMember(Name = "when")]
            public string When;
            [DataMember(Name = "headword")]
            public string Head;
            [DataMember(Name = "note")]
            public string Note;
            [DataMember(Name = "code")]
            public int Code;
        }

        [DataContract]
        public class QueryPageRes
        {
            [DataMember(Name = "summary")]
            public string Summary;
            [DataMember(Name = "items")]
            public List<HistoryItem> Items = new List<HistoryItem>();
        }

        private void doQueryPage()
        {
            int page = int.Parse(Req.Params["page"]);
            List<SqlDict.ChangeItem> changes;
            DateTime dtStart = DateTime.UtcNow;
            using (SqlDict.History hist = new SqlDict.History())
            {
                changes = hist.GetChangePage(page * 100, 100);
            }
            TimeSpan ts = DateTime.UtcNow.Subtract(dtStart);
            QueryPageRes res = new QueryPageRes();
            res.Summary = "Query executed in " + ((int)ts.TotalMilliseconds) + " msec; " + changes.Count + " results.";
            foreach (SqlDict.ChangeItem ci in changes)
            {
                res.Items.Add(new HistoryItem
                {
                    When = ci.When.ToShortDateString() + " " + ci.When.ToLongTimeString(),
                    Head = ci.EntryHead,
                    Note = ci.Note,
                    Code = (int)ci.ChangeType,
                });
            }

            // Tell our caller
            Res = res;
        }

        #endregion

        #region Index HDD

        private static int indexLineCount;

        [DataContract]
        public class IndexProgress
        {
            [DataMember(Name = "progress")]
            public string Progress;
            [DataMember(Name = "done")]
            public bool Done;
        }

        private void doProgressIndexHDD()
        {
            string progress;
            if (indexLineCount > 0)
            {
                progress = "Working, {0} lines processed.";
                progress = string.Format(progress, indexLineCount);
            }
            else
            {
                progress = "Done: {0} lines.";
                progress = string.Format(progress, -indexLineCount);
            }
            IndexProgress res = new IndexProgress { Progress = progress, Done = indexLineCount < 0 };

            // Tell our caller
            Res = res;
        }

        private void doIndexHDD()
        {
            ThreadPool.QueueUserWorkItem(funIndexHDD);
        }

        private void funIndexHDD(object o)
        {
            indexLineCount = 0;
            string hddPath = HttpRuntime.AppDomainAppPath;
            hddPath = Path.Combine(hddPath, "_data");
            //hddPath = Path.Combine(hddPath, "handesmall.u8");
            hddPath = Path.Combine(hddPath, "handedict_nb_sani03.u8");
            using (SqlDict.BulkBuilder imp = new SqlDict.BulkBuilder(0, "Importing stuff.", false))
            using (StreamReader sr = new StreamReader(hddPath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    imp.AddEntry(line);
                    ++indexLineCount;
                }
                imp.CommitRest();
            }
            indexLineCount = -indexLineCount;
        }

        #endregion
    }
//#endif
}