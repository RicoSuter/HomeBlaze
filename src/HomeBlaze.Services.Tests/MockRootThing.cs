using HomeBlaze.Abstractions;
using HomeBlaze.Things;

namespace HomeBlaze.Services.Tests
{
    public class MockRootThing : GroupBase, IGroupThing
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Title => "Mock Root Thing Title";

        public MockRootThing()
            : base(null!)
        {
        }
    }
}
