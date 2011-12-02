using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;
using ServiceStack.ServiceHost;

namespace ServiceStack.WebHost.Endpoints.Tests.IntegrationTests
{
    [AsyncService]
    public class DemoAsyncService
        : IService<DemoRequest>
        , IRestGetService<DemoRequest>
        , IRestPutService<DemoRequest>
        , IRestDeleteService<DemoRequest>
    {
        static readonly Dictionary<string, Demo> _store = new Dictionary<string, Demo>();

        public object Get(DemoRequest request)
        {
            Trace.WriteLine("Starting demo get @\r\n" + Environment.StackTrace);
            return Task.Factory.StartNew(() =>
            {
                Trace.WriteLine("Running demo get @\r\n" + Environment.StackTrace);
                lock (_store)
                {
                    Demo result;
                    _store.TryGetValue(request.Id, out result);
                    return new DemoResponse
                    {
                        Demo = result,
                    };
                }
            });
        }

        public object Put(DemoRequest request)
        {
            Trace.WriteLine("Starting demo put @\r\n" + Environment.StackTrace);
            return Task.Factory.StartNew(() =>
            {
                Trace.WriteLine("Running demo put @\r\n" + Environment.StackTrace);
                lock (_store)
                {
                    _store[request.Id] = request.Demo;
                    return new DemoResponse();
                }
            });
        }

        public object Delete(DemoRequest request)
        {
            Trace.WriteLine("Starting demo delete @\r\n" + Environment.StackTrace);
            return Task.Factory.StartNew(() =>
            {
                Trace.WriteLine("Running demo delete @\r\n" + Environment.StackTrace);
                lock (_store)
                {
                    _store.Remove(request.Id);
                    return new DemoResponse();
                }
            });
        }

        public object Execute(DemoRequest request)
        {
            return Get(request);
        }
    }
}
