using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceStack.DataAnnotations
{
    /// <summary>
    /// Hints to the framework that methods belonging to a the target service class are asynchronous/non-blocking and should be executed in such a way that is more optimal for non-blocking code.
    /// If this attribute is applied to a service class or a type from which the service class inherits then all methods within the class are assumed to be asynchronous.
    /// Methods belonging to classes with this attribute should return <see cref="IAsyncResult"/> objects.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AsyncServiceAttribute : Attribute
    {
        static readonly Dictionary<Type, bool> _reflectionCache = new Dictionary<Type, bool>();

        internal static bool IsServiceAsynchronous(Type serviceType)
        {
            lock (_reflectionCache)
            {
                bool result;
                if (_reflectionCache.TryGetValue(serviceType, out result)) return result;
                _reflectionCache[serviceType] = result = Attribute.IsDefined(serviceType, typeof(AsyncServiceAttribute), true);
                return result;
            }
        }
    }
}
