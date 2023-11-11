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

    private readonly IEnumerable<ITrackableInterceptor> _trackableInterceptors;
    private readonly IInterceptor[] _castleInterceptors;
    private readonly IServiceProvider _serviceProvider;

    public TrackableFactory(
        IServiceProvider serviceProvider,
        IEnumerable<ITrackableInterceptor>? trackableInterceptors = null,
        IEnumerable<IInterceptor>? interceptors = null)
    {
        _trackableInterceptors = trackableInterceptors?.ToArray() ?? Array.Empty<ITrackableInterceptor>();
        _castleInterceptors = interceptors?.ToArray() ?? Array.Empty<IInterceptor>();

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
                _castleInterceptors.Concat(new[] { new TrackableInterceptor(_trackableInterceptors) }).ToArray());

        return proxy;
    }
}
