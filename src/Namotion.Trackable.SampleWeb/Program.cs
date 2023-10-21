using Microsoft.AspNetCore.Mvc;
using Namotion.Trackable.AspNetCore.Controllers;
using Namotion.Trackable.Attributes;
using Namotion.Trackable.Sourcing;

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

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.UseOpenApi();
            app.UseSwaggerUi3();

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
            [TrackableSource("mqtt", RelativePath = "name")]
            public virtual string Name { get; set; } = "My Car";

            [Trackable]
            [TrackableSource("mqtt", RelativePath = "tires")]
            public virtual Tire[] Tires { get; set; }
        }

        public class Tire
        {
            [Trackable]
            [TrackableSource("mqtt", RelativePath = "pressure")]
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