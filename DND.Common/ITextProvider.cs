using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DND.Common
{
    /// <summary>
    /// Delegate to handle "language changed" event.
    /// </summary>
    public delegate void LanguageChangedDelegate();

    /// <summary>
    /// Implementor provides localized UI strings for whole application.
    /// </summary>
    public interface ITextProvider
    {
        /// <summary>
        /// Event fired when UI language changes.
        /// </summary>
        event LanguageChangedDelegate LanguageChanged;

        /// <summary>
        /// Returns key's localized UI string.
        /// </summary>
        /// <param name="key">Key (string ID).</param>
        /// <returns>Localized UI string in current display language.</returns>
        string GetString(string key);
    }
}
