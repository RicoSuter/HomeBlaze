using HomeBlaze.Components.Editors;
using System.Collections.Generic;
using System.Linq;

namespace HomeBlaze.Dynamic
{
    public class AutomationState
    {
        public int X { get; set; }

        public int Y { get; set; }

        public string? Name { get; set; }

        public IList<Operation> Operations { get; set; } = new List<Operation>();
    }
}
