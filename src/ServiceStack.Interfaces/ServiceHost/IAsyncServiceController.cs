using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceStack.ServiceHost
{
    public interface IAsyncServiceController : IServiceController
    {
        /// <summary>
        /// Executes the DTO request under the supplied requestContext.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestContext"></param>
        /// <param name="callback">The callback that executes when the request is compelte. May execute synchronously.</param>
        /// <returns></returns>
        void ExecuteAsync(object request, IRequestContext requestContext, Action<object, Exception> callback);
    }
}
