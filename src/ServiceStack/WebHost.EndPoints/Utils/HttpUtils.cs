using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using ServiceStack.WebHost.Endpoints.Support;

namespace ServiceStack.WebHost.Endpoints.Utils
{
    internal class HttpUtils
    {
        private class HttpUtilAsyncResult : SimpleAsyncResult
        {
            public HttpUtilAsyncResult(AsyncCallback cb, object state) : base(cb, state) { }

            public Exception Error { get; set; }
        }

        public static IAsyncResult BeginSynchronousHttpHandler(HttpContext context, AsyncCallback callback, object state, Action<HttpContext> handler)
        {
            var result = new HttpUtilAsyncResult(callback, state);
            ThreadPool.QueueUserWorkItem(o =>
            {
                Exception error = null;
                var oldContext = HttpContext.Current;
                HttpContext.Current = context;
                try
                {
                    handler(context);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                finally
                {
                    result.Error = error;
                    result.SetComplete(false);
                    HttpContext.Current = oldContext;
                }
            });
            return result;
        }

        public static void EndSynchronousHttpHandler(IAsyncResult asyncResult)
        {
            var result = (HttpUtilAsyncResult)asyncResult;
            if (!result.IsCompleted) result.AsyncWaitHandle.WaitOne();
            if (result.Error != null) throw result.Error;
        }
    }
}
