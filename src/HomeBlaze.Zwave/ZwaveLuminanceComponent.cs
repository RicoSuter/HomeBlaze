using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Sensors;

namespace HomeBlaze.Zwave
{
    public class ZwaveLuminanceComponent : ZwaveClassComponent, ILightSensor
    {
        protected override string Class => "Sensor:Luminance";

        public override string Title => $"Luminance (Node {ParentDevice?.NodeId.ToString() ?? "n/a"})";

        [State]
        public decimal? LightLevel { get; internal set; }

        [State]
        public byte? Scale { get; internal set; }

        [State]
        public string? OriginalUnit { get; internal set; }

        public ZwaveLuminanceComponent(ZwaveDevice parent)
            : base(parent)
        {
        }
    }
}
