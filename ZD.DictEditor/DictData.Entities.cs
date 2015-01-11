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
            NotStarted,
            Done,
            Marked,
        }

        public class HwData
        {
            private readonly int id;
            private readonly HwStatus status;
            private readonly string simp;
            private readonly string trad;
            private readonly string pinyin;
            private readonly string extract;

            public int Id { get { return id; } }
            public HwStatus Status { get { return status; } }
            public string Simp { get { return simp; } }
            public string Trad { get { return trad; } }
            public string Pinyin { get { return pinyin; } }
            public string Extract { get { return extract; } }

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

        public class HwBoundData
        {
            private readonly HwData data;
            public HwData Data { get { return data; } set { throw new NotImplementedException(); } }
            public HwBoundData(HwData data)
            {
                if (data == null) throw new ArgumentException("data");
                this.data = data;
            }
        }

        public class HWBoundEnumerator : IEnumerator<HwBoundData>
        {
            private readonly ReadOnlyCollection<HwData> hwColl;
            private int idx = 0;

            public HWBoundEnumerator(ReadOnlyCollection<HwData> hwColl)
            {
                this.hwColl = hwColl;
            }

            public HwBoundData Current
            {
                get { if (idx < hwColl.Count) return new HwBoundData(hwColl[idx]); return null; }
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

        public class HwBoundCollection : IList<HwBoundData>
        {
            private readonly ReadOnlyCollection<HwData> hwColl;

            public HwBoundCollection(ReadOnlyCollection<HwData> hwColl)
            {
                this.hwColl = hwColl;
            }

            public int IndexOf(HwBoundData item)
            {
                for (int i = 0; i != hwColl.Count; ++i)
                    if (hwColl[i] == item.Data) return i;
                return -1;
            }

            public void Insert(int index, HwBoundData item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public HwBoundData this[int index]
            {
                get { return new HwBoundData(hwColl[index]); }
                set { throw new NotImplementedException(); }
            }

            public void Add(HwBoundData item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(HwBoundData item)
            {
                return IndexOf(item) != -1;
            }

            public void CopyTo(HwBoundData[] array, int arrayIndex)
            {
                for (int i = 0; i != hwColl.Count; ++i) array[i + arrayIndex] = new HwBoundData(hwColl[i]);
            }

            public int Count
            {
                get { return hwColl.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool Remove(HwBoundData item)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<HwBoundData> GetEnumerator()
            {
                return new HWBoundEnumerator(hwColl);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return hwColl.GetEnumerator();
            }
        }
    }
}
