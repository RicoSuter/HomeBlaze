using System;

namespace Namotion.Trackable;

public interface ITrackableFactory
{
    TChildTing CreateProxy<TChildTing>();

    object CreateProxy(Type thingType);
}
