using System;
using System.ServiceModel.Channels;
using System.Threading;
using System.Web;
using System.Xml;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.WebHost.Endpoints.Utils;
using ServiceStack.WebHost.Endpoints.Metadata;
using ServiceStack.WebHost.Endpoints.Support;

namespace ServiceStack.WebHost.Endpoints
{
	public class Soap11SyncReplyHandler : SoapHandler
	{
		public Soap11SyncReplyHandler() : base(EndpointAttributes.Soap11) { }
	}

	public class Soap11AsyncOneWayHandler : SoapHandler
	{
		public Soap11AsyncOneWayHandler() : base(EndpointAttributes.Soap11) { }

		public override void ProcessRequest(HttpContext context)
		{
			if (context.Request.HttpMethod == HttpMethods.Get)
			{
				var wsdl = new Soap11WsdlMetadataHandler();
				wsdl.Execute(context);
				return;
			}

			var requestMessage = GetSoap11RequestMessage(context);
			SendOneWay(requestMessage);
		}
	}

	public class Soap11MessageSyncReplyHttpHandler : SoapHandler, IHttpAsyncHandler
	{
		public Soap11MessageSyncReplyHttpHandler() : base(EndpointAttributes.Soap11) {}

        public new IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            return HttpUtils.BeginSynchronousHttpHandler(context, cb, extraData, ProcessRequest);
        }

        public new void EndProcessRequest(IAsyncResult result)
        {
            HttpUtils.EndSynchronousHttpHandler(result);
        }

		public new void ProcessRequest(HttpContext context)
		{
			if (context.Request.HttpMethod == HttpMethods.Get)
			{
				var wsdl = new Soap11WsdlMetadataHandler();
				wsdl.Execute(context);
				return;
			}

			var requestMessage = GetSoap11RequestMessage(context);
			var responseMessage = Send(requestMessage);

			context.Response.ContentType = GetSoapContentType(context);
			using (var writer = XmlWriter.Create(context.Response.OutputStream))
			{
				responseMessage.WriteMessage(writer);
			}
		}
	}

}