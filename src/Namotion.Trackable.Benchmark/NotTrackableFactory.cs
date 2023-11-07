using System;
using System.Linq;

namespace Namotion.Trackable.Benchmark
{
    public class NotTrackableFactory : ITrackableFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public NotTrackableFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TProxy CreateProxy<TProxy>()
        {
            return (TProxy)CreateProxy(typeof(TProxy));
        }

        public object CreateProxy(Type proxyType)
        {
            var constructorArguments = proxyType
                .GetConstructors()
                .First()
                .GetParameters()
                .Select(p => p.ParameterType.IsAssignableTo(typeof(ITrackableFactory)) ? this :
                             _serviceProvider.GetService(p.ParameterType))
                .ToArray();

            return Activator.CreateInstance(proxyType, constructorArguments) ?? throw new InvalidOperationException("Could not create instance.");
        }
    }
}