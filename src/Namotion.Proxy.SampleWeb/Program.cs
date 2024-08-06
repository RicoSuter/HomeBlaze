using Microsoft.AspNetCore.Mvc;
using Namotion.Proxy;
using Namotion.Proxy.AspNetCore.Controllers;
using Namotion.Proxy.Attributes;
using Namotion.Proxy.Registry.Abstractions;
using Namotion.Proxy.Sources.Attributes;
using NSwag.Annotations;

namespace Namotion.Trackable.SampleWeb
{
    [GenerateProxy]
    public abstract class CarBase
    {
        public CarBase()
        {
            Tires = Enumerable
                .Range(1, 4)
                .Select(_ => new Tire())
                .ToArray();
        }

        [ProxySource("mqtt", "name")]
        [ProxySource("opc", "Name")]
        public virtual string Name { get; set; } = "My Car";

        [ProxySourcePath("mqtt", "tires")]
        [ProxySourcePath("opc", "Tires")]
        public virtual Tire[] Tires { get; set; }

        [ProxySource("mqtt", "averagePressure")]
        [ProxySource("opc", "AveragePressure")]
        public virtual decimal AveragePressure => Tires.Average(t => t.Pressure);
    }

    [GenerateProxy]
    public abstract class TireBase
    {
        [ProxySource("mqtt", "pressure")]
        [ProxySource("opc", "Pressure")]
        [Unit("bar")]
        public virtual decimal Pressure { get; set; }

        [Unit("bar")]
        [PropertyAttribute(nameof(Pressure), "Minimum")]
        public virtual decimal Pressure_Minimum { get; set; } = 0.0m;

        [PropertyAttribute(nameof(Pressure), "Maximum")]
        public virtual decimal Pressure_Maximum => 4 * Pressure;
    }

    public class UnitAttribute : Attribute, IProxyPropertyInitializer
    {
        private readonly string _unit;

        public UnitAttribute(string unit)
        {
            _unit = unit;
        }

        public void InitializeProperty(RegisteredProxyProperty property, object? index, IProxyContext context)
        {
            property.AddAttribute("Unit", typeof(string), () => _unit, null);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var context = ProxyContext
                .CreateBuilder()
                .WithRegistry()
                .WithFullPropertyTracking()
                .WithProxyLifecycle()
                .WithDataAnnotationValidation()
                .Build();

            var car = new Car(context);

            // trackable
            builder.Services.AddSingleton(car);

            // trackable api controllers
            builder.Services.AddProxyControllers<Car, TrackablesController<Car>>();

            // trackable UPC UA
            builder.Services.AddOpcUaServerProxySource<Car>("opc", rootName: "Root");

            // trackable mqtt
            builder.Services.AddMqttServerProxySource<Car>("mqtt");

            // trackable GraphQL
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

        [OpenApiTag("Car")]
        [Route("/api/car")]
        public class TrackablesController<TProxy> : ProxyControllerBase<TProxy> where TProxy : IProxy
        {
            public TrackablesController(TProxy proxy) : base(proxy)
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

                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}