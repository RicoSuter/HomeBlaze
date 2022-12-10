using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions;

namespace HomeBlaze.Things
{
    public class Vehicle : IThing
    {
        public string? Id => Title.GetHashCode().ToString();

        public string Title => "Tesla Model 3";

        [Configuration]
        public string? Baz { get; set; }

        [State]
        public string? Foo { get; private set; }

        [State]
        public string? Bar { get; private set; }

        [ScanForState]
        public VehicleBattery? Battery { get; private set; } = new VehicleBattery();

        [State]
        public Panel? Panel { get; private set; } = new Panel();

        [State]
        public IList<IThing> Tires { get; } = new List<IThing>
        {
            new Tire("Tire (Front Left)"),
            new Tire("Tire (Front Right)"),
            new Tire("Tire (Rear Left)") ,
            new Tire("Tire (Rear Right)")
        };
    }

    public class VehicleBattery
    {
        [State]
        public decimal? Charge { get; private set; } = 0.79m;

        [State("Battery/Voltage")]
        public decimal? Voltage { get; private set; } = 0.12m;
    }


    public class Tire : IThing
    {
        public string? Id => Title.GetHashCode().ToString();

        public string Title { get; private set; }

        public Tire(string name)
        {
            Title = name;
        }

        [State]
        public PressureSensor? PressureSensor { get; internal set; } = new PressureSensor();
    }

    public class Panel : IThing
    {
        public string? Id => Title.GetHashCode().ToString();

        public string Title => "Panel";

        [State]
        public bool? IsOn { get; private set; } = true;
    }

    public class PressureSensor : IThing
    {
        public string? Id => Title.GetHashCode().ToString();

        public string Title => "Pressure Sensor";

        [State]
        public decimal? Pressure { get; private set; } = 12m;
    }
}
