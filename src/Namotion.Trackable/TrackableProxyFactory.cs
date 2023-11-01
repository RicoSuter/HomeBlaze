using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Castle.DynamicProxy;

namespace Namotion.Trackable;

public class TrackableProxyFactory : ITrackableFactory
{
    private readonly IEnumerable<ITrackableInterceptor> _interceptors;
    private readonly IServiceProvider _serviceProvider;

    public TrackableProxyFactory(IEnumerable<ITrackableInterceptor> interceptors, IServiceProvider serviceProvider)
    {
        _interceptors = interceptors;
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

        var proxy = new ProxyGenerator()
            .CreateClassProxy(
                proxyType,
                new Type[] { typeof(ITrackable) },
                new ProxyGenerationOptions(),
                constructorArguments,
                new TrackableInterceptor(_interceptors));

        return proxy;
    }
}
