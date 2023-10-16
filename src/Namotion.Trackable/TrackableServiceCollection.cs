using Castle.DynamicProxy;

using Namotion.Trackable;
using Namotion.Trackable.Validation;
using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class TrackableServiceCollection
{
    public static IServiceCollection AddTrackedThing<TThing>(this IServiceCollection services)
        where TThing : class
    {
        services
            .AddSingleton(sp =>
            {
                var stateChangeValidators = sp.GetServices<IPropertyChangeValidator>();
                return new TrackableContext<TThing>(stateChangeValidators, sp);
            })
            .AddSingleton(sp => sp.GetRequiredService<TrackableContext<TThing>>().Object);

        return services;
    }

    internal static object CreateProxy(this IServiceProvider serviceProvider, Type proxyType, ITrackableContext thingContext, IInterceptor interceptor)
    {
        var constructorArguments = proxyType
            .GetConstructors()
            .First()
            .GetParameters()
            .Select(p => p.ParameterType.IsAssignableTo(typeof(ITrackableFactory)) ? thingContext :
                         serviceProvider.GetService(p.ParameterType))
            .ToArray();

        var thing = new ProxyGenerator()
            .CreateClassProxy(
                proxyType,
                new Type[] { typeof(ITrackable) },
                new ProxyGenerationOptions(),
                constructorArguments,
                interceptor);

        return thing;
    }
}

