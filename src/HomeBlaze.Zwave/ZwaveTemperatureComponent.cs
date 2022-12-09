using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;

namespace HomeBlaze.Zwave
{
    public class ZwaveTemperatureComponent : ZwaveClassComponent, IIconProvider, ITemperatureSensor
    {
        protected override string Class => "Sensor:Temperature";

        public override string Title => $"Temperature (Node {ParentDevice?.NodeId.ToString() ?? "n/a"})";

        public string IconName => "fas fa-temperature-high";

        [State]
        public decimal? Temperature { get; internal set; }

        [State]
        public byte? Scale { get; internal set; }

        [State]
        public string? OriginalUnit { get; internal set; }

        public ZwaveTemperatureComponent(ZwaveDevice parent)
            : base(parent)
        {
        }
    }
}
