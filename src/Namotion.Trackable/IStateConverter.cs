using Namotion.Trackable.Model;
using System;

namespace Namotion.Trackable;

public interface IStateConverter
{
    object? ConvertFromSource(object? value, Type targetType, TrackableProperty variable);

    object? ConvertToSource(object? value, TrackableProperty variable);
}
