using System;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Namotion.Trackable.Benchmark
{
    [MemoryDiagnoser]
    public class TrackableBenchmark
    {
#pragma warning disable CS8618

        protected Car _object;

#pragma warning restore CS8618

        [Params("regular", "trackable")]
        public string? type;

        [GlobalSetup]
        public void Setup()
        {
            if (type == "trackable")
            {
                var context = new TrackableContext<Car>(new TrackableFactory(
                    Array.Empty<ITrackableInterceptor>(),
                    new ServiceCollection().BuildServiceProvider()));

                _object = context.Object;
            }
            else
            {
                var factory = new TrackableFactory(
                    Array.Empty<ITrackableInterceptor>(),
                    new ServiceCollection().BuildServiceProvider());

                _object = new Car(factory);
            }
        }

        [Benchmark]
        public void WriteAndRead()
        {
            _object.Tires[0].Pressure += 5;
            _object.Tires[0].Pressure += 6;
            _object.Tires[0].Pressure += 7;
            _object.Tires[0].Pressure += 8;

            var average = _object.AveragePressure;
        }
    }
}