using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;

namespace ServiceStack.WebHost.Endpoints.Tests.IntegrationTests
{
    [DataContract(Namespace = ExampleConfig.DefaultNamespace)]
    public class DemoResponse
    {
        public DemoResponse()
        {
            ResponseStatus = new ResponseStatus();
        }

        [DataMember(EmitDefaultValue = false)]
        public ResponseStatus ResponseStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Demo Demo { get; set; }
    }
}
