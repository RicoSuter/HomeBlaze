using System.Linq;
using Namotion.Trackable.Attributes;
using Namotion.Trackable.Sources;

namespace Namotion.Trackable.Benchmark
{
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
        public virtual decimal AveragePressure => Tires.Average(t => t.Pressure);
    }
}