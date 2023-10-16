using System;

namespace Namotion.Trackable;

public interface ITrackableFactory
{
    TChildTing Create<TChildTing>();

    object Create(Type thingType);
}
