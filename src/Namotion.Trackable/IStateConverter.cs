using Namotion.Trackable.Model;
using System;

namespace Namotion.Trackable;

public interface IStateConverter
{
    object? ConvertFromSource(object? value, Type targetType, TrackedProperty variable);

    object? ConvertToSource(object? value, TrackedProperty variable);
}
