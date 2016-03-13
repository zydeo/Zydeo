using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace ZDO.CHSite
{
    /// <summary>
    /// Generic handler for all REST API calls.
    /// </summary>
    public class ApiHandler : IHttpHandler
    {
        /// <summary>
        /// Handler has no state, so it's reusable.
        /// </summary>
        public bool IsReusable { get { return true; } }

        /// <summary>
        /// Processes the request.
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                // Get our action. Throws 4XX errors if need be.
                // 400: Bad request
                // 403: Forbidden (when calling stuff without prior authentication; also on failed login).
                ApiAction action = ApiAction.CreateAction(context);
                // Process the request. Throws whatever if things go south.
                // 4XX
                // 500: Internal server error.
                action.Process();
                // Writes 200 response to stream. Shouldn't throw.
                action.SendResponse();
            }
            catch (ApiException ex)
            {
#if DEBUG
                context.Response.AddHeader("Access-Control-Allow-Origin", "*");
                context.Response.AddHeader("Access-Control-Allow-Headers", "*");
                context.Response.AddHeader("Access-Control-Allow-Credentials", "true");
#endif
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = ex.StatusCode;
                context.Response.Charset = "utf-8";
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.Write(ex.Message);
                context.Response.Flush();
            }
            catch (Exception ex)
            {
                DiagLogger.LogError(ex);
#if DEBUG
                context.Response.AddHeader("Access-Control-Allow-Origin", "*");
                context.Response.AddHeader("Access-Control-Allow-Headers", "*");
                context.Response.AddHeader("Access-Control-Allow-Credentials", "true");
#endif
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = 500;
                context.Response.Charset = "utf-8";
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.Write("Internal server error.");
                context.Response.Flush();
            }
        }
    }
}