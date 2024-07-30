using System.Linq;

namespace Namotion.Proxy.Benchmark
{
    [GenerateProxy]
    public abstract class CarBase
    {
        public CarBase()
        {
            Tires = new Tire[]
            {
                new Tire(),
                new Tire(),
                new Tire(),
                new Tire()
            };
        }

        public virtual string Name { get; set; } = "My Car";

        public virtual Tire[] Tires { get; set; }

        public virtual Car[]? PreviousCars { get; set; }

        public virtual decimal AveragePressure => Tires.Average(t => t.Pressure);
    }
}