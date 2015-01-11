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
            private readonly HwStatus status;
            private readonly string simp;
            private readonly string trad;
            private readonly string pinyin;
            private string extract;

            public int Id { get { return id; } }
            public HwStatus Status { get { return status; } }
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
            private int idx = 0;

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

            public HwCollection(ReadOnlyCollection<HwData> hwColl)
            {
                this.hwColl = hwColl;
            }

            public int IndexOf(HwData item)
            {
                for (int i = 0; i != hwColl.Count; ++i)
                    if (hwColl[i] == item) return i;
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
                get { return hwColl[index]; }
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
                for (int i = 0; i != hwColl.Count; ++i) array[i + arrayIndex] = hwColl[i];
            }

            public int Count
            {
                get { return hwColl.Count; }
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
                return new HwEnumerator(hwColl);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return hwColl.GetEnumerator();
            }
        }
    }
}
