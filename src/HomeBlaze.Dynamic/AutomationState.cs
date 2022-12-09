using System.Threading.Tasks;

namespace HomeBlaze.Dynamic
{
    public class AutomationState
    {
        public int X { get; set; }

        public int Y { get; set; }

        public string? Name { get; set; }

        public Operation? Operation { get; set; }

        public override string ToString()
        {
            return (Name ?? "n/a") +
                (Operation != null ? " (Operation)" : "");
        }
    }
}
