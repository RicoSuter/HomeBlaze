using Namotion.Proxy;

namespace Namotion.Proxy.Benchmark
{
    [GenerateProxy]
    public abstract class TireBase
    {
        public virtual decimal Pressure { get; set; }

        public virtual decimal Pressure_Minimum { get; set; } = 0.0m;

        public virtual decimal Pressure_Maximum { get; set; } = 4.0m;
    }
}