using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface ICameraSensor
    {
        [State]
        public Image? Image { get; set; }
    }
}
