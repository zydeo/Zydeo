using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DND.Common
{
    public interface ICedictEngine : IDisposable
    {
        CedictLookupResult Lookup(string what, SearchScript script, SearchLang lang);
    }
}
