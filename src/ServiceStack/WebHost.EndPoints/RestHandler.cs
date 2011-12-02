using System;
using System.Threading;
using ServiceStack.Common.Web;
using ServiceStack.Logging;
using ServiceStack.ServiceHost;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints.Extensions;
using ServiceStack.WebHost.Endpoints.Support;

namespace ServiceStack.WebHost.Endpoints
{
	public class RestHandler 
		: EndpointHandlerBase
	{
		public RestHandler()
		{
			this.HandlerAttributes = EndpointAttributes.SyncReply;
		}

		private static readonly ILog Log = LogManager.GetLogger(typeof(RestHandler));

		public static IRestPath FindMatchingRestPath(string httpMethod, string pathInfo)
		{
			var controller = ServiceManager != null
				? ServiceManager.ServiceController
				: EndpointHost.Config.ServiceController;

			return controller.GetRestPathForRequest(httpMethod, pathInfo);
		}

		public IRestPath GetRestPath(string httpMethod, string pathInfo)
		{
			if (this.RestPath == null)
			{
				this.RestPath = FindMatchingRestPath(httpMethod, pathInfo);
			}
			return this.RestPath;
		}

		internal IRestPath RestPath { get; set; }

        public override void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            Exception error = null;

            ProcessRequestAsync(httpReq, httpRes, operationName, e => error = e);

            if (error != null) throw error;
        }

        public override void ProcessRequestAsync(IHttpRequest httpReq, IHttpResponse httpRes, string operationName, Action<Exception> doneCallback)
        {
            bool callDoneCallback = true;
            var responseContentType = EndpointHost.Config.DefaultContentType;
            Exception error = null;
            try
            {
                var restPath = GetRestPath(httpReq.HttpMethod, httpReq.PathInfo);
                if (restPath == null)
                    throw new NotSupportedException("No RestPath found for: " + httpReq.HttpMethod + " " + httpReq.PathInfo);

                operationName = restPath.RequestType.Name;

                var callback = httpReq.QueryString["callback"];
                var doJsonp = EndpointHost.Config.AllowJsonpRequests
                              && !string.IsNullOrEmpty(callback);

                responseContentType = httpReq.ResponseContentType;
                EndpointHost.Config.AssertContentType(responseContentType);

                var request = GetRequest(httpReq, restPath);
                if (EndpointHost.ApplyRequestFilters(httpReq, httpRes, request)) return;

                GetResponseAsync(httpReq, request, (response, ex) =>
                {
                    Exception finalError = null;
                    try
                    {
                        if (ex == null)
                        {
                            PostProcessRequestAsync(httpReq, httpRes, operationName, response, responseContentType, callback, doJsonp);
                        }
                        else
                        {
                            if (!EndpointHost.Config.WriteErrorsToResponse)
                            {
                                finalError = ex;
                            }
                            else
                            {
                                HandleException(responseContentType, httpRes, operationName, ex);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        finalError = e;
                    }
                    finally
                    {
                        doneCallback(finalError);
                    }
                });
                callDoneCallback = false;
                return;
            }
            catch (Exception ex)
            {
                if (EndpointHost.Config.WriteErrorsToResponse)
                {
                    try
                    {
                        HandleException(responseContentType, httpRes, operationName, ex);
                    }
                    catch (Exception e)
                    {
                        error = e;
                    }
                }
                else
                {
                    error = ex;
                }
            }
            finally
            {
                if (callDoneCallback) doneCallback(error);
            }
        }

        void PostProcessRequestAsync(IHttpRequest httpReq, IHttpResponse httpRes, string operationName,
            object response, string responseContentType, string callback, bool doJsonp)
        {
            try
            {
                if (EndpointHost.ApplyResponseFilters(httpReq, httpRes, response)) return;

                if (responseContentType.Contains("jsv") && !string.IsNullOrEmpty(httpReq.QueryString["debug"]))
                {
                    JsvSyncReplyHandler.WriteDebugResponse(httpRes, response);
                    return;
                }

                if (doJsonp)
                    httpRes.WriteToResponse(httpReq, response, (callback + "(").ToUtf8Bytes(), ")".ToUtf8Bytes());
                else
                    httpRes.WriteToResponse(httpReq, response);
            }
            catch (Exception ex)
            {
                if (!EndpointHost.Config.WriteErrorsToResponse) throw;
                HandleException(responseContentType, httpRes, operationName, ex);
            }
        }

        private void GetResponseAsync(IHttpRequest httpReq, object request, Action<object, Exception> callback)
        {
			var requestContentType = ContentType.GetEndpointAttributes(httpReq.ResponseContentType);
            ExecuteServiceAsync(request, HandlerAttributes | requestContentType | GetEndpointAttributes(httpReq), httpReq, callback);
        }

		public override object GetResponse(IHttpRequest httpReq, object request)
		{
			var requestContentType = ContentType.GetEndpointAttributes(httpReq.ResponseContentType);

			return ExecuteService(request,
				HandlerAttributes | requestContentType | GetEndpointAttributes(httpReq), httpReq);
		}

		private static object GetRequest(IHttpRequest httpReq, IRestPath restPath)
		{
			var requestParams = httpReq.GetRequestParams();

			object requestDto = null;

			if (!string.IsNullOrEmpty(httpReq.ContentType) && httpReq.ContentLength > 0)
			{
				var requestDeserializer = EndpointHost.AppHost.ContentTypeFilters.GetStreamDeserializer(httpReq.ContentType);
				if (requestDeserializer != null)
				{
					requestDto = requestDeserializer(restPath.RequestType, httpReq.InputStream);
				}
			}

			return restPath.CreateRequest(httpReq.PathInfo, requestParams, requestDto);
		}

		/// <summary>
		/// Used in Unit tests
		/// </summary>
		/// <returns></returns>
		public override object CreateRequest(IHttpRequest httpReq, string operationName)
		{
			if (this.RestPath == null)
				throw new ArgumentNullException("No RestPath found");

			return GetRequest(httpReq, this.RestPath);
		}
	}

}
