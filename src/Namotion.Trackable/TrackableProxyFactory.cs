using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Castle.DynamicProxy;

namespace Namotion.Trackable;

public class TrackableProxyFactory : ITrackableFactory
{
    private readonly ITrackableContext _trackableContext;
    private readonly IEnumerable<ITrackablePropertyValidator> _propertyValidators;
    private readonly IServiceProvider _serviceProvider;

    public TrackableProxyFactory(ITrackableContext trackableContext, IEnumerable<ITrackablePropertyValidator> propertyValidators, IServiceProvider serviceProvider)
    {
        _trackableContext = trackableContext;
        _propertyValidators = propertyValidators;
        _serviceProvider = serviceProvider;
    }

    public TChild CreateProxy<TChild>()
    {
        return (TChild)CreateProxy(_serviceProvider, typeof(TChild));
    }

    public object CreateProxy(Type trackableType)
    {
        return CreateProxy(_serviceProvider, trackableType);
    }

    private object CreateProxy(IServiceProvider serviceProvider, Type proxyType)
    {
        var constructorArguments = proxyType
            .GetConstructors()
            .First()
            .GetParameters()
            .Select(p => p.ParameterType.IsAssignableTo(typeof(ITrackableFactory)) ? this :
                         p.ParameterType.IsAssignableTo(typeof(ITrackableContext)) ? _trackableContext :
                         serviceProvider.GetService(p.ParameterType))
            .ToArray();

        var proxy = new ProxyGenerator()
            .CreateClassProxy(
                proxyType,
                new Type[] { typeof(ITrackable) },
                new ProxyGenerationOptions(),
                constructorArguments,
                new TrackableInterceptor(_trackableContext, _propertyValidators));

        return proxy;
    }
}
