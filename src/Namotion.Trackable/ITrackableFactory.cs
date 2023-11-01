using System;

namespace Namotion.Trackable;

public interface ITrackableFactory
{
    TProxy CreateRootProxy<TProxy>(ITrackableContext trackableContext);

    TProxy CreateProxy<TProxy>();

    object CreateProxy(Type thingType);
}
