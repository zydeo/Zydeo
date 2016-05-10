using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;

namespace ZDO.CHSite
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
        private readonly Regex reStringLine = new Regex(@"^([^\t]+)[\t]+([^\n]+)$");

        private void initForLang(string langCode)
        {
            // Key-value pairs parsed now.
            Dictionary<string, string> newStrings = new Dictionary<string, string>();

            // Load language file, parse
            Assembly a = Assembly.GetExecutingAssembly();
            string fileName = "ZDO.CHSite.Resources." + langCode + ".txt";
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
        }

        private TextProvider()
        {
            initForLang("hu");
            initForLang("en");
        }

        public string GetString(string langCode, string id)
        {
            Dictionary<string, string> huDict = dict["hu"];
            Dictionary<string, string> myDict = huDict;
            if (dict.ContainsKey(langCode)) myDict = dict[langCode];
            if (myDict.ContainsKey(id)) return myDict[id];
            else return huDict[id];
        }
    }
}