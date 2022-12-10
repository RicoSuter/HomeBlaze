using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Things
{
    public class Dashboard : IThing
    {

        [Configuration(IsIdentifier = true)]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        public string? Title => Name;

        [Configuration]
        public string? Name { get; set; }

        [Configuration]
        public string? Icon { get; set; }

        [Configuration, State]
        public IList<Widget> Widgets { get; set; } = new List<Widget>();
    }
}