using Namotion.Trackable.Model;

namespace Namotion.Trackable.Attributes;

public interface ITrackableInitializer
{
    void Initialize(Tracker tracker, ITrackableContext context);
}