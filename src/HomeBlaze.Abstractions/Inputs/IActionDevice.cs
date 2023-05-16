using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Inputs
{
    public interface IActionDevice : IThing
    {
        [State]
        DeviceAction[] Actions { get; }

        [Operation]
        Task ExecuteActionAsync(string actionId, CancellationToken cancellationToken = default);

        public struct DeviceAction
        {
            public string Id { get; set; }

            public string Title { get; set; }

            public override string ToString()
            {
                return Title + " (" + Id + ")";
            }
        }
    }
}