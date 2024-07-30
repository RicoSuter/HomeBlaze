using HotChocolate.Execution.Configuration;

using Microsoft.Extensions.Hosting;

using Namotion.Proxy;
using Namotion.Proxy.GraphQL;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class TrackableGraphQLExtensions
{
    public static void AddTrackedGraphQL<TProxy>(this IRequestExecutorBuilder builder)
        where TProxy : IProxy
    {
        builder
            .Services
            .AddSingleton<IHostedService, GraphQLSubscriptionSender<TProxy>>();

        builder
            .AddQueryType<Query<TProxy>>()
            .AddSubscriptionType<Subscription<TProxy>>();
    }
}