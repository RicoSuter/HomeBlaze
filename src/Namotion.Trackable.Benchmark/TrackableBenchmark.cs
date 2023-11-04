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
        public string? Type;

        [GlobalSetup]
        public void Setup()
        {
            if (Type == "trackable")
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
        public void IncrementDerivedAverage()
        {
            _object.Tires[0].Pressure += 5;
            _object.Tires[1].Pressure += 6;
            _object.Tires[2].Pressure += 7;
            _object.Tires[3].Pressure += 8;

            var average = _object.AveragePressure;
        }

        [Benchmark]
        public void Write()
        {
            _object.Tires[0].Pressure = 5;
            _object.Tires[1].Pressure = 6;
            _object.Tires[2].Pressure = 7;
            _object.Tires[3].Pressure = 8;
        }

        [Benchmark]
        public decimal Read()
        {
            return 
                _object.Tires[0].Pressure +
                _object.Tires[1].Pressure +
                _object.Tires[2].Pressure +
                _object.Tires[3].Pressure;
        }

        [Benchmark]
        public void DerivedAverage()
        {
            var average = _object.AveragePressure;
        }
    }
}