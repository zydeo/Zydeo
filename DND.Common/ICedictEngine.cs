using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DND.Common
{
    public interface ICedictEngine
    {
        CedictLookupResult Lookup(string what, SearchScript script, SearchLang lang);
    }

    public interface ICedictEngineFactory
    {
        ICedictEngine Create(string dictFileName);
    }
}
