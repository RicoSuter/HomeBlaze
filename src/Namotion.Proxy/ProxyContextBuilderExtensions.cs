using Namotion.Proxy.Abstractions;
using Namotion.Proxy.ChangeTracking;
using Namotion.Proxy.Lifecycle;
using Namotion.Proxy.Registry;
using Namotion.Proxy.Validation;

namespace Namotion.Proxy;

public static class ProxyContextBuilderExtensions
{
    public static IProxyContextBuilder WithFullPropertyTracking(this IProxyContextBuilder builder)
    {
        return builder
            .WithEqualityCheck()
            .WithAutomaticContextAssignment()
            .WithDerivedPropertyChangeDetection();
    }

    public static IProxyContextBuilder WithEqualityCheck(this IProxyContextBuilder builder)
    {
        return builder
            .TryAddSingleHandler(new PropertyValueEqualityCheckHandler());
    }

    public static IProxyContextBuilder WithDerivedPropertyChangeDetection(this IProxyContextBuilder builder)
    {
        return builder
            .TryAddSingleHandler(new InitiallyLoadDerivedPropertiesHandler())
            .TryAddSingleHandler(new DerivedPropertyChangeDetectionHandler(builder.GetLazyHandlers<IProxyWriteHandler>()))
            .WithPropertyChangedObservable();
    }

    public static IProxyContextBuilder WithReadPropertyRecorder(this IProxyContextBuilder builder)
    {
        return builder
            .TryAddSingleHandler(new ReadPropertyRecorder());
    }

    /// <summary>
    /// Registers support for <see cref="IProxyPropertyValidator"/> handlers.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The builder.</returns>
    public static IProxyContextBuilder WithPropertyValidation(this IProxyContextBuilder builder)
    {
        return builder
            .TryAddSingleHandler(new ProxyValidationHandler(builder.GetLazyHandlers<IProxyPropertyValidator>()));
    }

    /// <summary>
    /// Adds support for data annotations on the proxy properties and <see cref="WithPropertyValidation"/>.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The builder.</returns>
    public static IProxyContextBuilder WithDataAnnotationValidation(this IProxyContextBuilder builder)
    {
        return builder
            .TryAddSingleHandler(new DataAnnotationsValidator())
            .WithPropertyValidation();
    }

    /// <summary>
    /// Registers the property changed observable which can be retrieved using proxy.GetPropertyChangedObservable().
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The builder.</returns>
    public static IProxyContextBuilder WithPropertyChangedObservable(this IProxyContextBuilder builder)
    {
        return builder
            .TryAddSingleHandler(new PropertyChangedObservable());
    }

    /// <summary>
    /// Adds automatic context assignment and <see cref="WithProxyLifecycle"/>.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The builder.</returns>
    public static IProxyContextBuilder WithAutomaticContextAssignment(this IProxyContextBuilder builder)
    {
        return builder
            .TryAddSingleHandler(new AutomaticContextAssignmentHandler())
            .WithProxyLifecycle();
    }

    /// <summary>
    /// Adds support for <see cref="IProxyLifecycleHandler"/> handlers.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The builder.</returns>
    public static IProxyContextBuilder WithProxyLifecycle(this IProxyContextBuilder builder)
    {
        return builder
            .TryAddSingleHandler(new ProxyLifecycleHandler(builder.GetLazyHandlers<IProxyLifecycleHandler>()));
    }

    /// <summary>
    /// Adds support for <see cref="IProxyLifecycleHandler"/> handlers.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The builder.</returns>
    public static IProxyContextBuilder WithRegistry(this IProxyContextBuilder builder)
    {
        return builder
            .TryAddSingleHandler(new ProxyRegistry())
            .WithAutomaticContextAssignment();
    }

    /// <summary>
    /// Automatically assigns the parents to the proxy data.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The builder.</returns>
    public static IProxyContextBuilder WithParents(this IProxyContextBuilder builder)
    {
        return builder
            .TryAddSingleHandler(new ParentsHandler())
            .WithProxyLifecycle();
    }
}