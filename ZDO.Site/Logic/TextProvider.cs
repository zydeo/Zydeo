using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;

namespace Site
{
    internal class TextProvider
    {
        private static TextProvider instance = null;

        public static TextProvider Instance
        {
            get { return instance; }
        }

        public static void Init()
        {
            instance = new TextProvider();
        }

        private readonly Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();
        private readonly Dictionary<string, Dictionary<string, string>> snips = new Dictionary<string, Dictionary<string, string>>();

        private readonly Regex reStringLine = new Regex(@"^([^\t]+)[\t]+([^\n]+)$");

        private void initSnippet(string langCode, string snippetName)
        {
            Assembly a = Assembly.GetExecutingAssembly();

            string fileName = "Site.Resources." + snippetName + "_" + langCode + ".txt";
            using (Stream s = a.GetManifestResourceStream(fileName))
            {
                if (s != null)
                {
                    using (StreamReader sr = new StreamReader(s))
                    {
                        if (!snips.ContainsKey(langCode)) snips[langCode] = new Dictionary<string, string>();
                        snips[langCode][snippetName] = sr.ReadToEnd();
                    }
                }
            }
        }

        private void initForLang(string langCode)
        {
            // Key-value pairs parsed now.
            Dictionary<string, string> newStrings = new Dictionary<string, string>();

            // Load language file, parse
            Assembly a = Assembly.GetExecutingAssembly();
            string fileName = "Site.Resources." + langCode + ".txt";
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
            // Store for language
            dict[langCode] = newStrings;
            // Init HTML snippets
            initSnippet(langCode, "welcome");
            initSnippet(langCode, "noresults");
        }


        private TextProvider()
        {
            initForLang("en");
            initForLang("de");
            initForLang("jian");
            initForLang("fan");
        }

        public string GetString(string langCode, string id)
        {
            Dictionary<string, string> enDict = dict["en"];
            Dictionary<string, string> myDict = enDict;
            if (dict.ContainsKey(langCode)) myDict = dict[langCode];
            if (myDict.ContainsKey(id)) return myDict[id];
            else return enDict[id];
        }

        public string GetSnippet(string langCode, string snippetName)
        {
            Dictionary<string, string> enSnips = snips["en"];
            Dictionary<string, string> mySnips = enSnips;
            if (snips.ContainsKey(langCode)) mySnips = snips[langCode];
            if (mySnips.ContainsKey(snippetName)) return mySnips[snippetName];
            else return enSnips[snippetName];
        }
    }
}