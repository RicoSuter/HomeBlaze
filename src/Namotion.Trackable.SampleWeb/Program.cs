using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Mvc;
using Namotion.Trackable.AspNetCore.Controllers;
using Namotion.Trackable.Attributes;
using Namotion.Trackable.Sourcing;
using System.Reactive.Linq;

namespace Namotion.Trackable.SampleWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAuthorization();

            builder.Services.AddTrackable<Car>();
            builder.Services.AddTrackableControllers<Car, TrackablesController<Car>>();
            builder.Services.AddMqttServerTrackableSource<Car>("mqtt");

            builder.Services.AddHostedService<Simulator>();

            builder.Services.AddOpenApiDocument();

            builder.Services
                .AddHostedService<GraphQLSubscriptionSender>() // graphql

                .AddGraphQLServer()
                .AddInMemorySubscriptions()

                // graphql
                .AddQueryType<Query>()
                .AddSubscriptionType<Subscription>();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapGraphQL();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.Run();
        }

        public class GraphQLSubscriptionSender : BackgroundService
        {
            private readonly TrackableContext<Car> _trackableContext;
            private readonly ITopicEventSender _sender;

            public GraphQLSubscriptionSender(TrackableContext<Car> trackableContext, ITopicEventSender sender)
            {
                _trackableContext = trackableContext;
                _sender = sender;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                await _trackableContext.ForEachAsync(async (change) =>
                {
                    await _sender.SendAsync(nameof(Subscription.Car), _trackableContext.Object);
                }, stoppingToken);
            }
        }

        public class Subscription
        {
            [Subscribe]
            public Car Car([EventMessage] Car car) => car;
        }

        public class Query
        {
            private readonly Car _car;

            public Query(Car car)
            {
                _car = car;
            }

            public Car GetCar() => _car;
        }

        public class Car
        {
            public Car(ITrackableFactory factory)
            {
                Tires = new Tire[]
                {
                    factory.CreateProxy<Tire>(),
                    factory.CreateProxy<Tire>(),
                    factory.CreateProxy<Tire>(),
                    factory.CreateProxy<Tire>()
                };
            }

            [Trackable]
            [TrackableSource("mqtt", "name")]
            public virtual string Name { get; set; } = "My Car";

            [Trackable]
            [TrackableSource("mqtt", "tires")]
            public virtual Tire[] Tires { get; set; }
        }

        public class Tire
        {
            [Trackable]
            [TrackableSource("mqtt", "pressure")]
            public virtual decimal Pressure { get; set; }
        }

        [Route("api/car")]
        public class TrackablesController<TTrackable> : TrackablesControllerBase<TTrackable>
            where TTrackable : class
        {
            public TrackablesController(TTrackable trackable, TrackableContext<TTrackable> context)
                : base(trackable, context)
            {
            }
        }

        public class Simulator : BackgroundService
        {
            private readonly Car _car;

            public Simulator(Car car)
            {
                _car = car;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _car.Tires[0].Pressure++;
                    _car.Tires[1].Pressure++;
                    _car.Tires[2].Pressure++;
                    _car.Tires[3].Pressure++;

                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}