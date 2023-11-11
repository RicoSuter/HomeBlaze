using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Namotion.Trackable.Benchmark
{
    [MemoryDiagnoser]
    public class TrackableBenchmark
    {
#pragma warning disable CS8618

        private Car _object;
        private ITrackableFactory _factory;

#pragma warning restore CS8618

        [Params(
            //"regular_small",
            //"regular_huge",
            //"trackable_small", 
            "trackable_huge"
        )]
        public string? Type;

        [GlobalSetup]
        public void Setup()
        {
            if (Type == "regular_small")
            {
                _factory = new NotTrackableFactory(
                    new ServiceCollection().BuildServiceProvider());

                _object = new Car(_factory);
            }
            else if (Type == "regular_huge")
            {
                _factory = new NotTrackableFactory(
                    new ServiceCollection().BuildServiceProvider());

                _object = new Car(_factory);
                AddLotsOfPreviousCars();
            }
            else if (Type == "trackable_small")
            {
                _factory = new TrackableFactory(new ServiceCollection().BuildServiceProvider());
                var context = new TrackableContext<Car>(_factory);
                _object = context.Object;
            }
            else
            {
                _factory = new TrackableFactory(new ServiceCollection().BuildServiceProvider());
                var context = new TrackableContext<Car>(_factory);
                _object = context.Object;
                AddLotsOfPreviousCars();
            }
        }

        [Benchmark]
        public void AddLotsOfPreviousCars()
        {
            _object.PreviousCars = Enumerable.Range(0, 10000)
                .Select(i => _factory.CreateProxy<Car>())
                .ToArray();
        }

        [Benchmark]
        public void IncrementDerivedAverage()
        {
            _object.Tires[0].Pressure += 5;
            _object.Tires[1].Pressure += 6;
            _object.Tires[2].Pressure += 7;
            _object.Tires[3].Pressure += 8;

            var average = _object.AveragePressure;

            _object.PreviousCars = null;
        }

        //[Benchmark]
        //public void Write()
        //{
        //    _object.Tires[0].Pressure = 5;
        //    _object.Tires[1].Pressure = 6;
        //    _object.Tires[2].Pressure = 7;
        //    _object.Tires[3].Pressure = 8;
        //}

        //[Benchmark]
        //public decimal Read()
        //{
        //    return 
        //        _object.Tires[0].Pressure +
        //        _object.Tires[1].Pressure +
        //        _object.Tires[2].Pressure +
        //        _object.Tires[3].Pressure;
        //}

        //[Benchmark]
        //public void DerivedAverage()
        //{
        //    var average = _object.AveragePressure;
        //}

        [Benchmark]
        public void ChangeAllTires()
        {
            var newTires = new Tire[]
            {
                _factory?.CreateProxy<Tire>() ?? new Tire(),
                _factory?.CreateProxy<Tire>() ?? new Tire(),
                _factory?.CreateProxy<Tire>() ?? new Tire(),
                _factory?.CreateProxy<Tire>() ?? new Tire()
            };

            _object.Tires = newTires;
        }
    }
}