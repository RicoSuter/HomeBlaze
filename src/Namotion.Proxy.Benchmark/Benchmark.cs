using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Namotion.Proxy.Benchmark;

[MemoryDiagnoser]
public class Benchmark
{
#pragma warning disable CS8618

    private Car _object;
    private ProxyContext? _context;

#pragma warning restore CS8618

    [Params(
        "regular",
        "proxy"
    )]
    public string? Type;

    [GlobalSetup]
    public void Setup()
    {
        if (Type == "regular")
        {
            _object = new Car();
        }
        else if (Type == "proxy")
        {
            _context = ProxyContext
                .CreateBuilder()
                .WithFullPropertyTracking()
                .WithRegistry()
                .Build();

            _object = new Car(_context);
            AddLotsOfPreviousCars();
        }
    }

    //[Benchmark]
    public void AddLotsOfPreviousCars()
    {
        _object.PreviousCars = Enumerable.Range(0, 10000)
            .Select(i => new Car())
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
            new Tire(),
            new Tire(),
            new Tire(),
            new Tire()
        };

        _object.Tires = newTires;
    }
}