using System;

namespace Namotion.Trackable;

public interface ITrackableFactory
{
    TProxy CreateProxy<TProxy>();

    object CreateProxy(Type thingType);
}
