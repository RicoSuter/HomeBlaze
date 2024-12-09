using System.Linq;

namespace Namotion.Proxy.Benchmark
{
    [GenerateProxy]
    public partial class Car
    {
        public Car()
        {
            Tires = new Tire[]
            {
                new Tire(),
                new Tire(),
                new Tire(),
                new Tire()
            };

            Name = "My Car";
        }

        public partial string Name { get; set; }

        public partial Tire[] Tires { get; set; }

        public partial Car[]? PreviousCars { get; set; }

        public decimal AveragePressure => Tires.Average(t => t.Pressure);
    }
}