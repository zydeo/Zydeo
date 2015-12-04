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
            context.Response.ContentType = "text/plain";
            context.Response.Write("pong");
        }

        public bool IsReusable { get { return false; } }
    }
}