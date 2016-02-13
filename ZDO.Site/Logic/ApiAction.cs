using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Site
{
    /// <summary>
    /// Base class for all API action handlers. Also serves as action handler factory.
    /// </summary>
    internal abstract class ApiAction
    {
        /// <summary>
        /// Creates action handler by looking at request's "action" parameter.
        /// </summary>
        public static ApiAction CreateAction(HttpContext ctxt)
        {
            string action = ctxt.Request.Params["action"];
            if (action == "hanzi") return new ActionHanzi(ctxt);
            else if (action == null) throw new ApiException(400, "Missing 'action' parameter.");
            else throw new ApiException(400, "Unsupported action: " + action);
        }
        /// <summary>
        /// The request's HTTP context.
        /// </summary>
        protected readonly HttpContext Ctxt;
        /// <summary>
        /// The HTTP request (extraced from context for convenience).
        /// </summary>
        protected readonly HttpRequest Req;
        /// <summary>
        /// The HTTP response (extracted from context for convenience).
        /// </summary>
        protected readonly HttpResponse Resp;
        /// <summary>
        /// JSON to return to caller. Can be left null, then empty 200 response is sent.
        /// </summary>
        protected string Json = null;
        /// <summary>
        /// Ctor: init context.
        /// </summary>
        protected ApiAction(HttpContext ctxt)
        {
            Ctxt = ctxt;
            Req = ctxt.Request;
            Resp = ctxt.Response;
        }
        /// <summary>
        /// Process request. Overridden in implementors.
        /// </summary>
        public abstract void Process();
        /// <summary>
        /// Send response. Overridden in implementors that have something special to say to caller.
        /// </summary>
        public virtual void SendResponse()
        {
#if DEBUG
            Resp.AddHeader("Access-Control-Allow-Origin", "*");
            Resp.AddHeader("Access-Control-Allow-Headers", "*");
            Resp.AddHeader("Access-Control-Allow-Credentials", "true");
#endif
            Resp.StatusCode = 200;
            Resp.Charset = "utf-8";
            Resp.ContentEncoding = Encoding.UTF8;
            Resp.ContentType = "application/json";
            if (Json != null)
            {
                Resp.Write(Json);
                Resp.Flush();
            }
        }
    }

    /// <summary>
    /// API exception for 400 type errors.
    /// </summary>
    internal class ApiException : Exception
    {
        /// <summary>
        /// Status code to return.
        /// </summary>
        public readonly int StatusCode;
        /// <summary>
        /// Ctor: status code and message.
        /// </summary>
        public ApiException(int statusCode, string message) : base(message) { StatusCode = statusCode; }
    }
}