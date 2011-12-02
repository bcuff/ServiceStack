using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ServiceStack.Common.Web;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;

namespace ServiceStack.WebHost.Endpoints.Tests.IntegrationTests
{
    [TestFixture]
    public class DemoRestTests : IntegrationTestBase
    {
        [Test]
        public void TestDemo()
        {
            foreach (var client in new ServiceClientBase [] { new XmlServiceClient(BaseUrl), new JsonServiceClient(BaseUrl) })
            {
                using (client)
                {
                    SendToEndpoint<DemoResponse>(client, new DemoRequest
                    {
                        Id = "1",
                        Demo = new Demo
                        {
                            Text = "Hello, world!"
                        },
                    }, HttpMethods.Put, response => Assert.That(response.ResponseStatus.ErrorCode, Is.Null));
                }
            }

            foreach (var client in new ServiceClientBase[] { new XmlServiceClient(BaseUrl), new JsonServiceClient(BaseUrl) })
            {
                using (client)
                {
                    SendToEndpoint<DemoResponse>(client, new DemoRequest
                    {
                        Id = "1"
                    }, HttpMethods.Get, response => Assert.That(response.Demo.Text, Is.EqualTo("Hello, world!")));
                }
            }
        }
    }
}
