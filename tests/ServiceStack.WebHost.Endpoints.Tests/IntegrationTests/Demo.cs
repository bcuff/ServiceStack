using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ServiceStack.ServiceHost;

namespace ServiceStack.WebHost.Endpoints.Tests.IntegrationTests
{
    [DataContract(Namespace = ExampleConfig.DefaultNamespace)]
    public class Demo
    {
        [DataMember(EmitDefaultValue = false)]
        public string Text { get; set; }
    }
}
