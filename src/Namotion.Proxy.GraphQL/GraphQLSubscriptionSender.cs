using HotChocolate.Subscriptions;
using Microsoft.Extensions.Hosting;
using Namotion.Proxy.Abstractions;
using System.Reactive.Linq;

namespace Namotion.Proxy.GraphQL
{
    public class GraphQLSubscriptionSender<TProxy> : BackgroundService
        where TProxy : IProxy
    {
        private readonly TProxy _proxy;
        private readonly IProxyContext _context;
        private readonly ITopicEventSender _sender;

        public GraphQLSubscriptionSender(TProxy proxy, ITopicEventSender sender)
        {
            _context = proxy.Context  ??
                throw new InvalidOperationException($"Context is not set on {nameof(TProxy)}.");

            _proxy = proxy;
            _sender = sender;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _context
                .GetPropertyChangedObservable()
                .ForEachAsync(async (change) =>
                {
                    await _sender.SendAsync(nameof(Subscription<TProxy>.Root), _proxy, stoppingToken);
                }, stoppingToken);
        }
    }
}