using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Castle.DynamicProxy;

namespace Namotion.Trackable;

public class TrackableProxyFactory : ITrackableFactory
{
    private readonly IEnumerable<ITrackablePropertyValidator> _propertyValidators;
    private readonly IServiceProvider _serviceProvider;

    public TrackableProxyFactory(IEnumerable<ITrackablePropertyValidator> propertyValidators, IServiceProvider serviceProvider)
    {
        _propertyValidators = propertyValidators;
        _serviceProvider = serviceProvider;
    }

    public TChild CreateRootProxy<TChild>(ITrackableContext trackableContext)
    {
        return (TChild)CreateProxy(_serviceProvider, typeof(TChild), trackableContext);
    }

    public TChild CreateProxy<TChild>()
    {
        return (TChild)CreateProxy(_serviceProvider, typeof(TChild), null);
    }

    public object CreateProxy(Type trackableType)
    {
        return CreateProxy(_serviceProvider, trackableType, null);
    }

    private object CreateProxy(IServiceProvider serviceProvider, Type proxyType, ITrackableContext? trackableContext)
    {
        var constructorArguments = proxyType
            .GetConstructors()
            .First()
            .GetParameters()
            .Select(p => p.ParameterType.IsAssignableTo(typeof(ITrackableFactory)) ? this :
                         serviceProvider.GetService(p.ParameterType))
            .ToArray();

        var proxy = new ProxyGenerator()
            .CreateClassProxy(
                proxyType,
                new Type[] { typeof(ITrackable) },
                new ProxyGenerationOptions(),
                constructorArguments,
                new TrackableInterceptor(_propertyValidators, trackableContext));

        return proxy;
    }
}
