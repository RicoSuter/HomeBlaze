using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Inputs
{
    public interface IActivityDevice : IThing
    {
        [State]
        DeviceActivity Activity { get; }

        [State]
        DeviceActivity[] Activities { get; }

        [Operation]
        Task ChangeActivityAsync(string activityId, CancellationToken cancellationToken);

        public struct DeviceActivity
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