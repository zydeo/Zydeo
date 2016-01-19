using System;
using System.Collections.Generic;
using System.Web;
using System.Text;

using ZD.Common;

namespace Site
{
    /// <summary>
    /// Returns information about a Hanzi (stroke order, decomposition etc.)
    /// </summary>
    internal class ActionHanzi : ApiAction
    {
        /// <summary>
        /// Ctor: init. Boilerplate.
        /// </summary>
        public ActionHanzi(HttpContext ctxt) : base(ctxt) { }

        /// <summary>
        /// Retrieves information about hanzi.
        /// </summary>
        public override void Process()
        {
            string hanzi = Req.Params["hanzi"];
            if (hanzi == null) throw new ApiException(400, "Missing 'hanzi' parameter.");
            if (hanzi.Length != 1) throw new ApiException(400, "'hanzi' parameter must contain a single character.");
            // Look up hanzi
            HanziInfo hi = Global.Dict.GetHanziInfo(hanzi[0]);
            // Log query
            QueryLogger.Instance.LogHanzi(Req.UserHostAddress, hanzi[0], hi != null);
            // Produce response
            if (hi == null) Json = "null";
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("{");
                sb.AppendLine("  \"strokes\":");
                sb.AppendLine("  [");
                for (int i = 0; i != hi.Strokes.Count; ++i)
                {
                    OneStroke stroke = hi.Strokes[i];
                    sb.Append("    ");
                    sb.Append('"');
                    sb.Append(stroke.SVG);
                    sb.Append('"');
                    if (i != hi.Strokes.Count - 1) sb.Append(",");
                    sb.AppendLine();
                }
                sb.AppendLine("  ],");
                sb.Append("  \"medians\": [");
                for (int i = 0; i != hi.Strokes.Count; ++i)
                {
                    OneStroke stroke = hi.Strokes[i];
                    sb.Append("[");
                    for (int j = 0; j != stroke.Median.Count; ++j)
                    {
                        Tuple<short, short> median = stroke.Median[j];
                        sb.Append("[");
                        sb.Append(median.Item1.ToString());
                        sb.Append(",");
                        sb.Append(median.Item2.ToString());
                        sb.Append("]");
                        if (j != stroke.Median.Count - 1) sb.Append(",");
                    }
                    sb.Append("]");
                    if (i != hi.Strokes.Count - 1) sb.Append(",");
                }
                sb.AppendLine("]");
                sb.AppendLine("}");
                // Finished, this is what we'll return
                Json = sb.ToString();
            }
        }
    }
}