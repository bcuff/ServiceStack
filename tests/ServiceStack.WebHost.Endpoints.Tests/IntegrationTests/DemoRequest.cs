using System;
using System.Linq;
using System.Runtime.Serialization;
using ServiceStack.ServiceHost;

namespace ServiceStack.WebHost.Endpoints.Tests.IntegrationTests
{
    [DataContract(Namespace = ExampleConfig.DefaultNamespace)]
    [RestService("/demo/{Id}")]
    public class DemoRequest
    {
        [DataMember(EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Demo Demo { get; set; }
    }
}
