using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using ZD.ChDict.Common;

namespace ZD.DictEditor
{
    partial class DictData
    {
        public enum HwStatus
        {
            NotStarted = 0,
            Done = 1,
            Dropped = 2,
            Edited = 3,
            Marked = 4,
        }

        public class HwData
        {
            private readonly int id;
            private HwStatus status;
            private readonly string simp;
            private readonly string trad;
            private readonly string pinyin;
            private string extract;

            public int Id { get { return id; } }
            public HwStatus Status { get { return status; } set { status = value; } }
            public string Simp { get { return simp; } }
            public string Trad { get { return trad; } }
            public string Pinyin { get { return pinyin; } }
            public string Extract { get { return extract; } set { extract = value; } }

            public HwData(int id, HwStatus status, string simp, string trad, string pinyin, string extract)
            {
                if (id < 0) throw new ArgumentException("id");
                if (string.IsNullOrEmpty(simp)) throw new ArgumentException("simp");
                if (string.IsNullOrEmpty(trad)) throw new ArgumentException("trad");
                if (string.IsNullOrEmpty(pinyin)) throw new ArgumentException("pinyion");
                if (extract == null) throw new ArgumentException("extract");

                this.id = id;
                this.status = status;
                this.simp = simp;
                this.trad = trad;
                this.pinyin = pinyin;
                this.extract = extract;
            }
        }

        public class HwEnumerator : IEnumerator<HwData>
        {
            private readonly ReadOnlyCollection<HwData> hwColl;

            private int idx = -1;

            public HwEnumerator(ReadOnlyCollection<HwData> hwColl)
            {
                this.hwColl = hwColl;
            }

            public HwData Current
            {
                get { if (idx < hwColl.Count) return hwColl[idx]; return null; }
            }

            public void Dispose()
            {}

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                ++idx;
                return idx < hwColl.Count;
            }

            public void Reset()
            {
                idx = 0;
            }
        }

        public class HwCollection : IList<HwData>
        {
            private readonly ReadOnlyCollection<HwData> hwColl;
            private ReadOnlyCollection<HwData> filteredHwColl;
            private string simpFilter = string.Empty;
            private bool meFilter = false;

            public HwCollection(ReadOnlyCollection<HwData> hwColl)
            {
                this.hwColl = hwColl;
                filteredHwColl = hwColl;
            }

            public int IndexOf(HwData item)
            {
                for (int i = 0; i != filteredHwColl.Count; ++i)
                    if (filteredHwColl[i] == item) return i;
                return -1;
            }

            public void Insert(int index, HwData item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public HwData this[int index]
            {
                get { return filteredHwColl[index]; }
                set { throw new NotImplementedException(); }
            }

            public void Add(HwData item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(HwData item)
            {
                return IndexOf(item) != -1;
            }

            public void CopyTo(HwData[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { return filteredHwColl.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool Remove(HwData item)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<HwData> GetEnumerator()
            {
                return new HwEnumerator(filteredHwColl);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return new HwEnumerator(filteredHwColl);
            }

            public void GetStatCounts(out int done, out int editedMarked, out int dropped, out int notStarted)
            {
                done = editedMarked = dropped = notStarted = 0;
                foreach (var hw in hwColl)
                {
                    if (hw.Status == HwStatus.Done) ++done;
                    else if (hw.Status == HwStatus.Dropped) ++dropped;
                    else if (hw.Status == HwStatus.NotStarted) ++notStarted;
                    else ++editedMarked;
                }
            }

            private void doFilter()
            {
                List<HwData> fl = new List<HwData>(hwColl.Count);
                foreach (HwData data in hwColl)
                {
                    if (simpFilter != string.Empty && !data.Simp.Contains(simpFilter))
                        continue;
                    if (meFilter && (data.Status != HwStatus.Marked && data.Status != HwStatus.Edited))
                        continue;
                    fl.Add(data);
                }
                filteredHwColl = new ReadOnlyCollection<HwData>(fl);
            }

            public void SetSimpFilter(string filter)
            {
                simpFilter = filter;
                doFilter();
            }

            public bool OnlyMarkedOrEdited
            {
                set
                {
                    meFilter = value;
                    doFilter();
                }
            }
        }
    }
}
