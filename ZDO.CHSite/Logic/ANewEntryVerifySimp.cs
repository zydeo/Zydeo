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
    [ActionName("newentry_verifysimp")]
    public class ANewEntryVerifySimp : ApiAction
    {
        private static readonly string errSpacePunct = "A címszó nem tartalmazhat szóközöket vagy központozást";
        private static readonly string errNotHanzi = "Nem írásjegy, nagybetű vagy számjegy:";
        private static readonly string errUnsupported = "Nem támogatott írásjegy:";
        private static readonly string errNotSimp = "Nem egyszerűsített írásjegy:";

        /// <summary>
        /// Ctor: init. Boilerplate.
        /// </summary>
        public ANewEntryVerifySimp(HttpContext ctxt) : base(ctxt) { }

        [DataContract]
        public class Result
        {
            [DataMember(Name = "passed")]
            public bool Passed;
            [DataMember(Name = "errors")]
            public List<string> Errors = null;
        }

        /// <summary>
        /// Verifies if string can serve as a simplified headword.
        /// </summary>
        public override void Process()
        {
            string simp = Req.Params["simp"];
            if (simp == null) throw new ApiException(400, "Missing 'simp' parameter.");
            Result res = new Result();
            UniHanziInfo[] uhis = Global.HWInfo.GetUnihanInfo(simp);

            // Has WS or punctuation
            bool hasSpaceOrPunct = false;
            // Chars that are neither hanzi nor A-Z0-9
            List<char> notHanziOrLD = new List<char>();
            // Unsupported hanzi: no Unihan info
            List<char> unsupportedHanzi = new List<char>();
            // Not simplified
            List<char> notSimp = new List<char>();

            // Check each character
            for (int i = 0; i != simp.Length; ++i)
            {
                char c = simp[i];
                UniHanziInfo uhi = uhis[i];

                // Space or punct
                if (char.IsWhiteSpace(c) || char.IsPunctuation(c)) { hasSpaceOrPunct = true; continue; }
                // Is it even a Hanzi or A-Z0-9?
                bool isHanziOrLD = SqlDict.IsHanzi(c) || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9');
                if (!isHanziOrLD) { notHanziOrLD.Add(c); continue; }
                // No info?
                if (uhi == null) { unsupportedHanzi.Add(c); continue; }
                // Cannot be simplified?
                if (!uhi.CanBeSimp) { notSimp.Add(c); continue; }
            }

            // Passed or not
            if (!hasSpaceOrPunct && notHanziOrLD.Count == 0 && unsupportedHanzi.Count == 0 && notSimp.Count == 0)
                res.Passed = true;
            else
            {
                // Compile our errors
                res.Passed = false;
                res.Errors = new List<string>();
                if (hasSpaceOrPunct) res.Errors.Add(errSpacePunct);
                if (notHanziOrLD.Count != 0)
                {
                    string err = errNotHanzi;
                    foreach (char c in notHanziOrLD) { err += ' '; err += c; }
                    res.Errors.Add(err);
                }
                if (unsupportedHanzi.Count != 0)
                {
                    string err = errUnsupported;
                    foreach (char c in unsupportedHanzi) { err += ' '; err += c; }
                    res.Errors.Add(err);
                }
                if (notSimp.Count != 0)
                {
                    string err = errNotSimp;
                    foreach (char c in notSimp) { err += ' '; err += c; }
                    res.Errors.Add(err);
                }
            }

            // Tell our caller
            Res = res;
        }
    }
}