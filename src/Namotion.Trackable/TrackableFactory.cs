using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Castle.DynamicProxy;

namespace Namotion.Trackable;

public class TrackableFactory : ITrackableFactory
{
    private readonly ProxyGenerator _proxyGenerator = new();
    private readonly Type[] _additionalInterfacesToProxy = new Type[] { typeof(ITrackable) };
    private readonly ProxyGenerationOptions _proxyGenerationOptions = new();

    private readonly IEnumerable<ITrackableInterceptor> _interceptors;
    private readonly IInterceptor[] _castleInterceptors;
    private readonly IServiceProvider _serviceProvider;

    public TrackableFactory(IEnumerable<ITrackableInterceptor> interceptors, IServiceProvider serviceProvider)
    {
        _interceptors = interceptors;
        // TODO: use own array with castle interceptors
        _castleInterceptors = _interceptors.OfType<IInterceptor>().ToArray();
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

        var proxy = _proxyGenerator
            .CreateClassProxy(
                proxyType,
                _additionalInterfacesToProxy,
                _proxyGenerationOptions,
                constructorArguments,
                _castleInterceptors.Concat(new[] { new TrackableInterceptor(_interceptors) }).ToArray());

        return proxy;
    }
}
