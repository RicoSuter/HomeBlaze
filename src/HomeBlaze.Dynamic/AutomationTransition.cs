using Blazor.Diagrams.Core.Models;
using System.Text.Json.Serialization;

namespace HomeBlaze.Dynamic
{
    public class AutomationTransition
    {
        public string? Name { get; set; }


        public PortAlignment? ToPort { get; set; }

        public PortAlignment? FromPort { get; set; }


        public string? FromState { get; set; }

        public string? ToState { get; set; }


        public AutomationCondition Condition { get; set; } = new AutomationCondition();

        public Operation? Operation { get; set; }


        [JsonIgnore]
        public string Title => (Name ?? "n/a") + (Operation != null ? " (Operation)" : "");
    }
}
