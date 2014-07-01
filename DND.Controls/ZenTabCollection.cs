using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DND.Controls
{
    public class ZenTabCollection : IEnumerable<ZenTab>
    {
        private readonly List<ZenTab> tabs = new List<ZenTab>();
        private readonly IZenTabsChangedListener listener;

        internal ZenTabCollection(IZenTabsChangedListener listener)
        {
            if (listener == null) throw new ArgumentNullException("listener");
            this.listener = listener;
        }

        public ZenTab this[int index]
        {
            get { return tabs[index]; }
        }

        public int Count
        {
            get { return tabs.Count; }
        }

        public void Add(ZenTab tab)
        {
            tabs.Add(tab);
            tab.TabHeaderChanged = tabHeaderChanged;
            listener.ZenTabsChanged();
        }

        private void tabHeaderChanged()
        {
            listener.ZenTabsChanged();
        }

        public IEnumerator<ZenTab> GetEnumerator()
        {
            return tabs.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return tabs.GetEnumerator();
        }
    }
}
