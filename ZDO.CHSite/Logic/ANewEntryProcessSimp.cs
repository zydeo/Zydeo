using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

using ZD.Common;

namespace ZDO.CHSite
{
    /// <summary>
    /// 
    /// </summary>
    [ActionName("newentry_processsimp")]
    public class ANewEntryProcessSimp : ApiAction
    {
        /// <summary>
        /// Ctor: init. Boilerplate.
        /// </summary>
        public ANewEntryProcessSimp(HttpContext ctxt) : base(ctxt) { }

        [DataContract]
        public class Result
        {
            [DataMember(Name = "trad")]
            public List<List<string>> Trad = new List<List<string>>();
            [DataMember(Name = "pinyin")]
            public List<List<string>> Pinyin = new List<List<string>>();
            [DataMember(Name = "is_known_headword")]
            public bool IsKnownHeadword = false;
        }

        private static void addIfNew(List<string> lst, string item)
        {
            if (!lst.Contains(item)) lst.Add(item);
        }

        /// <summary>
        /// Retrieves information about (simplified) hanzi.
        /// </summary>
        public override void Process()
        {
            string simp = Req.Params["simp"];
            if (simp == null) throw new ApiException(400, "Missing 'simp' parameter.");
            Result res = new Result();

            // Prepare result: as long as input; empty array for each position
            foreach (char c in simp)
            {
                res.Trad.Add(new List<string>());
                res.Pinyin.Add(new List<string>());
            }

            // Do we have CEDICT headwords for this simplified HW?
            // If yes, put first headword's traditional and pinyin into first layer of result
            // Fill rest of the alternatives with input from additional results
            HeadwordSyll[][] chHeads = Global.HWInfo.GetPossibleHeadwords(simp, false);
            for (int i = 0; i != chHeads.Length; ++i)
            {
                HeadwordSyll[] sylls = chHeads[i];
                for (int j = 0; j != simp.Length; ++j)
                {
                    addIfNew(res.Trad[j], sylls[j].Trad.ToString());
                    addIfNew(res.Pinyin[j], sylls[j].Pinyin);
                }
            }
            // Unihan lookup
            UniHanziInfo[] uhis = Global.HWInfo.GetUnihanInfo(simp);
            // We had no headword: build from Unihan data, but with a twist
            // Make sure first traditional matches most common pinyin
            if (chHeads.Length == 0)
            {
                for (int i = 0; i != uhis.Length; ++i)
                {
                    UniHanziInfo uhi = uhis[i];
                    if (uhi == null) continue;
                    // Add pinyin readings first
                    foreach (PinyinSyllable syll in uhi.Pinyin) addIfNew(res.Pinyin[i], syll.GetDisplayString(true));
                    // Look up traditional chars for this position
                    UniHanziInfo[] tradUhis = Global.HWInfo.GetUnihanInfo(uhi.TradVariants);
                    // Find "best" traditional character: the first one whose pinyin readings include our first pinyin
                    char firstTrad = (char)0;
                    string favoritePinyin = uhi.Pinyin[0].GetDisplayString(true);
                    if (tradUhis != null)
                    {
                        for (int tx = 0; tx != uhi.TradVariants.Length; ++tx)
                        {
                            UniHanziInfo tradUhi = tradUhis[tx];
                            if (tradUhi == null) continue;
                            bool hasFavoritePinyin = false;
                            foreach (PinyinSyllable py in tradUhi.Pinyin)
                            {
                                if (py.GetDisplayString(true) == favoritePinyin)
                                {
                                    hasFavoritePinyin = true;
                                    break;
                                }
                            }
                            if (hasFavoritePinyin)
                            {
                                firstTrad = uhi.TradVariants[tx];
                                break;
                            }
                        }
                    }
                    // Add first traditional, if found
                    if (firstTrad != (char)0) addIfNew(res.Trad[i], firstTrad.ToString());
                    // Add all the remaining traditional variants
                    foreach (char c in uhi.TradVariants) addIfNew(res.Trad[i], c.ToString());
                }
            }
            // We had a headword: fill remaining slots with traditional and pinyin items from Unihan
            else
            {
                res.IsKnownHeadword = true;
                for (int i = 0; i != uhis.Length; ++i)
                {
                    UniHanziInfo uhi = uhis[i];
                    if (uhi == null) continue;
                    foreach (char c in uhi.TradVariants) addIfNew(res.Trad[i], c.ToString());
                    foreach (PinyinSyllable syll in uhi.Pinyin) addIfNew(res.Pinyin[i], syll.GetDisplayString(true));
                }
            }
            // Filter pinyin: only keep those that work with traditional on the first spot
            // Unless intersection is empty - can also happen in this weird world
            for (int i = 0; i != simp.Length; ++i)
            {
                List<string> pyList = res.Pinyin[i];
                if (pyList.Count < 2) continue;
                List<string> tradList = res.Trad[i];
                if (tradList.Count == 0) continue;
                List<string> toRem = new List<string>();
                UniHanziInfo[] tradUhis = Global.HWInfo.GetUnihanInfo(new char[] { tradList[0][0] });
                if (tradUhis == null || tradUhis[0] == null) continue;
                List<string> pinyinsOfTrad = new List<string>();
                foreach (var x in tradUhis[0].Pinyin) pinyinsOfTrad.Add(x.GetDisplayString(true));
                // If we had a match, start from second: don't want to remove what just came from CEDICT
                for (int j = res.IsKnownHeadword ? 1 : 0; j < pyList.Count; ++j)
                {
                    string py = pyList[j];
                    if (!pinyinsOfTrad.Contains(py)) toRem.Add(py);
                }
                if (toRem.Count == pyList.Count) continue;
                foreach (string py in toRem) pyList.Remove(py);
            }

            // Check if there are positions where we have no tradition or pinyin
            // For the purposes of this lookup, we just inject character from input there
            for (int i = 0; i != simp.Length; ++i)
            {
                char c = simp[i];
                if (res.Trad[i].Count == 0) res.Trad[i].Add(c.ToString());
                if (res.Pinyin[i].Count == 0) res.Pinyin[i].Add(c.ToString());
            }

            // Tell our caller
            Res = res;
        }
    }
}