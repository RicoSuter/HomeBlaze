using Castle.DynamicProxy;

using Namotion.Trackable;
using Namotion.Trackable.Validation;
using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class TrackableServiceCollection
{
    public static IServiceCollection AddTrackableContext<TTrackable>(this IServiceCollection services)
        where TTrackable : class
    {
        services
            .AddSingleton(sp =>
            {
                var propertyValidators = sp.GetServices<ITrackablePropertyValidator>();
                var interceptors = sp.GetServices<ITrackableInterceptor>();
                return new TrackableContext<TTrackable>(propertyValidators, interceptors, sp);
            })
            .AddSingleton(sp => sp.GetRequiredService<TrackableContext<TTrackable>>().Object);

        return services;
    }

    internal static object CreateProxy(this IServiceProvider serviceProvider, Type proxyType, ITrackableContext thingContext, IInterceptor[] interceptors)
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
                interceptors);

        return thing;
    }
}

