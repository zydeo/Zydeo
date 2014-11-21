using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZD.Common;

namespace ZD.CedictEngine
{
    partial class DictEngine
    {
        /// <summary>
        /// Contains information about a compiled CEDICT dictionary.
        /// </summary>
        public class CedictInfo : ICedictInfo
        {
            private readonly DateTime date;
            private readonly int entryCount;

            /// <summary>
            /// Publication date of the CEDICT data.
            /// </summary>
            public DateTime Date
            {
                get { return date; }
            }

            /// <summary>
            /// Number of entries in the dictionary.
            /// </summary>
            public int EntryCount
            {
                get { return entryCount; }
            }

            /// <summary>
            /// Ctor: init immutable instance.
            /// </summary>
            internal CedictInfo(DateTime date, int entryCount)
            {
                this.date = date;
                this.entryCount = entryCount;
            }
        }

        /// <summary>
        /// Retrieves informatio about the dictionary, wihtout loading indexes.
        /// </summary>
        public static ICedictInfo GetInfo(string dictFileName)
        {
            return new CedictInfo(DateTime.Now, 95782);
        }
    }
}
