using Microsoft.AspNetCore.Mvc;

using Namotion.Trackable.AspNetCore.Controllers;
using Namotion.Trackable.Attributes;
using Namotion.Trackable.GraphQL;
using Namotion.Trackable.Model;
using Namotion.Trackable.Sources;
using NSwag.Annotations;

namespace Namotion.Trackable.SampleWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // trackable
            builder.Services.AddTrackable<Car>();

            // trackable api controllers
            builder.Services.AddTrackableControllers<Car, TrackablesController<Car>>();

            // trackable UPC UA
            builder.Services.AddOpcUaServerTrackableSource<Car>("mqtt");

            // trackable mqtt
            builder.Services.AddMqttServerTrackableSource<Car>("mqtt");

            // trackable graphql
            builder.Services
                .AddGraphQLServer()
                .AddInMemorySubscriptions()
                .AddTrackedGraphQL<Car>();

            // other asp services
            builder.Services.AddHostedService<Simulator>();
            builder.Services.AddOpenApiDocument();
            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapGraphQL();

            app.UseOpenApi();
            app.UseSwaggerUi();

            app.MapControllers();
            app.Run();
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
            [TrackableSourcePath("mqtt", "tires")]
            public virtual Tire[] Tires { get; set; }

            [Trackable]
            [TrackableSource("mqtt", "averagePressure")]
            public virtual decimal AveragePressure => Tires.Average(t => t.Pressure);
        }

        public class Tire
        {
            [Trackable]
            [TrackableSource("mqtt", "pressure")]
            [Unit("bar")]
            public virtual decimal Pressure { get; set; }

            [Unit("bar")]
            [AttributeOfTrackable(nameof(Pressure), "Minimum")]
            public virtual decimal Pressure_Minimum { get; set; } = 0.0m;

            [AttributeOfTrackable(nameof(Pressure), "Maximum")]
            public virtual decimal Pressure_Maximum { get; set; } = 4.0m;
        }

        public class UnitAttribute : Attribute, ITrackablePropertyInitializer
        {
            private readonly string _unit;

            public UnitAttribute(string unit)
            {
                _unit = unit;
            }

            public void InitializeProperty(TrackedProperty property, object? parentCollectionKey, ITrackableContext context)
            {
                property.Parent.AddProperty(
                    TrackedProperty<string>.CreateAttribute(property, "Unit", _unit, context));
            }
        }

        [OpenApiTag("Car")]
        [Route("/api/car")]
        public class TrackablesController<TTrackable> : TrackablesControllerBase<TTrackable>
            where TTrackable : class
        {
            public TrackablesController(TrackableContext<TTrackable> trackableContext)
                : base(trackableContext)
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
                    _car.Tires[0].Pressure = Random.Shared.Next(0, 40) / 10m;
                    _car.Tires[1].Pressure = Random.Shared.Next(0, 40) / 10m;
                    _car.Tires[2].Pressure = Random.Shared.Next(0, 40) / 10m;
                    _car.Tires[3].Pressure = Random.Shared.Next(0, 40) / 10m;

                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}