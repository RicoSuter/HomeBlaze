using HotChocolate.Subscriptions;
using Microsoft.Extensions.Hosting;
using System.Reactive.Linq;

namespace Namotion.Trackable.GraphQL
{
    public class GraphQLSubscriptionSender<TTrackable> : BackgroundService
        where TTrackable : class
    {
        private readonly TrackableContext<TTrackable> _trackableContext;
        private readonly ITopicEventSender _sender;

        public GraphQLSubscriptionSender(TrackableContext<TTrackable> trackableContext, ITopicEventSender sender)
        {
            _trackableContext = trackableContext;
            _sender = sender;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _trackableContext.ForEachAsync(async (change) =>
            {
                await _sender.SendAsync(nameof(Subscription<TTrackable>.Root), _trackableContext.Object, stoppingToken);
            }, stoppingToken);
        }
    }
}