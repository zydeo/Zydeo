using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace ZDO.CHSite
{
    // Attribute for derived actions to indicate API name.
    [AttributeUsage(AttributeTargets.Class)]
    public class ActionName : Attribute
    {
        string name;
        public ActionName(string name)
        { this.name = name; }
        public string Name { get { return name; } }
    }

    /// <summary>
    /// Base class for all API action handlers. Also serves as action handler factory.
    /// </summary>
    public abstract class ApiAction
    {
        /// <summary>
        /// Creates action handler by looking at request's "action" parameter.
        /// </summary>
        public static ApiAction CreateAction(HttpContext ctxt)
        {
            string action = ctxt.Request.Params["action"];
            if (!actionMap.ContainsKey(action)) throw new ApiException(400, "Unsupported action: " + action);
            ConstructorInfo ctor = actionMap[action].GetConstructor(new[] { typeof(HttpContext) });
            object instance = ctor.Invoke(new object[] { ctxt });
            return (ApiAction)instance;

        }

        /// <summary>
        /// Static ctor: discover, and register, all action implementors.
        /// </summary>
        static ApiAction()
        {
            Type[] types = Assembly.GetExecutingAssembly().GetExportedTypes();
            List<Type> actions = new List<Type>();
            foreach (Type t in types)
                if (t.IsSubclassOf(typeof(ApiAction))) actions.Add(t);
            foreach (Type t in actions)
            {
                Attribute[] attrs = Attribute.GetCustomAttributes(t);
                foreach (Attribute attr in attrs)
                {
                    ActionName an = attr as ActionName;
                    if (an == null) continue;
                    actionMap[an.Name] = t;
                }
            }
        }

        /// <summary>
        /// Maps from action names to implementing types.
        /// </summary>
        private static Dictionary<string, Type> actionMap = new Dictionary<string, Type>();

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
        /// Result to return to caller, serialized as JSON.
        /// </summary>
        protected object Res = null;
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

            DataContractJsonSerializer js = new DataContractJsonSerializer(Res.GetType());
            MemoryStream ms = new MemoryStream();
            js.WriteObject(ms, Res);
            ms.Position = 0;
            StreamReader sr = new StreamReader(ms);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                Resp.Write(line);
                Resp.Write("\r\n");
            }
            Resp.Flush();
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