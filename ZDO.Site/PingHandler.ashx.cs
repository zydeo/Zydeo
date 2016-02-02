using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Site
{
    public class PingHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string prev, next;
            Global.Dict.GetPrevNextWords("anruf", true, out prev, out next);
            Global.Dict.GetPrevNextWords("卫生", false, out prev, out next);

            context.Response.ContentType = "text/plain";
            context.Response.Write("pong");
        }

        public bool IsReusable { get { return false; } }
    }
}