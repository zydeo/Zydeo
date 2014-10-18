using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// Script (simplified, traditional or both) for lookup, and for showing results.
    /// </summary>
    public enum SearchScript
    {
        /// <summary>
        /// Simplified hanzi.
        /// </summary>
        Simplified = 0,
        /// <summary>
        /// Traditional hanzi.
        /// </summary>
        Traditional = 1,
        /// <summary>
        /// Simplified as well as traditional.
        /// </summary>
        Both = 2,
    }

    /// <summary>
    /// Language to search in (Chinese or target language).
    /// </summary>
    public enum SearchLang
    {
        /// <summary>
        /// Target language (for CEDICT: English).
        /// </summary>
        Target,
        /// <summary>
        /// Chinese (hanzi or pinyin).
        /// </summary>
        Chinese,
    }
}
