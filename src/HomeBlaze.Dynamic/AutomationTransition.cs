using Blazor.Diagrams.Core.Models;
using HomeBlaze.Components.Editors;
using System.Collections.Generic;
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

        public IList<Operation> Operations { get; set; } = new List<Operation>();


        [JsonIgnore]
        public string Title => (Name ?? "n/a") + (
            Operations.Count == 1 ? ", with Operation" :
            Operations.Count > 1 ? ", with Operations" : 
            "");
    }
}
