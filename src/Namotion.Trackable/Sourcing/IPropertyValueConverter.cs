using Namotion.Trackable.Model;
using System;

namespace Namotion.Trackable.Sourcing;

public interface IPropertyValueConverter
{
    object? ConvertFromSource(object? value, Type targetType, TrackedProperty variable);

    object? ConvertToSource(object? value, TrackedProperty variable);
}
