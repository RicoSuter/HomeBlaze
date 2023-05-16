using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Services.Tests
{
    public class MockThing : IThing
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Title => "Mock Thing Title";

        [State]
        public int? Value { get; set; }
    }
}
