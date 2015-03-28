using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;

namespace ZD.AU
{
    /// <summary>
    /// Provides localized UI strings for the updater UI.
    /// </summary>
    internal class TextProvider
    {
        /// <summary>
        /// Holds UI string for each key.
        /// </summary>
        private Dictionary<string, string> strings;

        /// <summary>
        /// Regex to parse one line in UI strings file.
        /// </summary>
        private Regex reStringLine = new Regex(@"^([^\t]+)[\t]+([^\n]+)$");

        /// <summary>
        /// Loads UI strings for specified language.
        /// </summary>
        private void initForLang(string langCode)
        {
            // New collection: will replace old one in one go.
            Dictionary<string, string> newStrings = new Dictionary<string, string>();

            // Load language file, parse
            Assembly a = Assembly.GetExecutingAssembly();
            string fileName = "ZD.AU.Resources.au_" + langCode + ".txt";
            using (Stream s = a.GetManifestResourceStream(fileName))
            using (StreamReader sr = new StreamReader(s))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line == string.Empty) continue;
                    if (line.StartsWith("#")) continue;
                    Match m = reStringLine.Match(line);
                    if (!m.Success) continue;
                    string escaped = m.Groups[2].Value.Replace(@"\r\n", "\r\n");
                    escaped = escaped.Replace(@"\n", "\r\n");
                    newStrings[m.Groups[1].Value] = escaped;
                }
            }
            // Replace old strings
            strings = newStrings;
        }

        /// <summary>
        /// Ctor: initializes strings from embedded resource.
        /// </summary>
        /// <param name="langCode">2-letter ISO code of UI language.</param>
        public TextProvider(string langCode)
        {
            // Fall back to "en" if initialization fails for a different language code.
            try { initForLang(langCode); }
            catch { initForLang("en"); }
        }

        /// <summary>
        /// Switches UI to specified language. Fires <see cref="LanguageChaged"/> event.
        /// </summary>
        /// <param name="langCode">2-letter ISO code of UI language.</param>
        public void SetLang(string langCode)
        {
            initForLang(langCode);
        }

        /// <summary>
        /// See <see cref="ITextProvider.GetString"/>.
        /// </summary>
        public string GetString(string key)
        {
            if (strings.ContainsKey(key)) return strings[key];
            return key;
        }
    }
}
