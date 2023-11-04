using Namotion.Trackable.Attributes;
using Namotion.Trackable.Sources;

namespace Namotion.Trackable.Benchmark
{
    public class Tire
    {
        [Trackable]
        [TrackableSource("mqtt", "pressure")]
        public virtual decimal Pressure { get; set; }

        [AttributeOfTrackable(nameof(Pressure), "minimum")]
        public virtual decimal Pressure_Minimum { get; set; } = 0.0m;

        [AttributeOfTrackable(nameof(Pressure), "maximum")]
        public virtual decimal Pressure_Maximum { get; set; } = 4.0m;
    }
}