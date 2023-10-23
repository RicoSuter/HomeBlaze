using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Namotion.Trackable.GraphQL
{
    public static class TrackableGraphQLExtensions
    {
        public static void AddTrackedGraphQL<TTrackable>(this IRequestExecutorBuilder builder)
            where TTrackable : class
        {
            builder
                .Services
                .AddHostedService<GraphQLSubscriptionSender<TTrackable>>();

            builder
                .AddQueryType<Query<TTrackable>>()
                .AddSubscriptionType<Subscription<TTrackable>>();
        }
    }
}