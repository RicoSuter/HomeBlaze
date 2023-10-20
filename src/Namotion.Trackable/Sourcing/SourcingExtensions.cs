﻿using Namotion.Trackable.Model;

namespace Namotion.Trackable.Sourcing;

public static class SourcingExtensions
{
    internal const string SourcePathKey = "SourcePath";
    internal const string IsChangingFromSourceKey = "IsChangingFromSource";

    public static string? TryGetSourcePath(this TrackableProperty property)
    {
        return property.Data.TryGetValue(SourcePathKey, out var value) ? value as string : null;
    }

    public static bool IsChangingFromSource(this TrackablePropertyChange change)
    {
        return change.PropertyDataSnapshot.TryGetValue(IsChangingFromSourceKey, out var isChangingFromSource) && 
            isChangingFromSource is bool isChangingFromSourceBool ? isChangingFromSourceBool : false;
    }

    public static object? GetSourceValue(this TrackableProperty property)
    {
        var value = property.GetValue();
        return ConvertToSource(property, value);
    }

    public static void SetValueFromSource(this TrackableProperty property, object? valueFromSource)
    {
        property.Data[IsChangingFromSourceKey] = true;
        try
        {
            var currentValue = property.GetValue();
            var newValue = ConvertFromSource(property, valueFromSource);
            if (!Equals(currentValue, newValue))
            {
                property.SetValue(newValue);
            }
        }
        finally
        {
            property.Data[IsChangingFromSourceKey] = false;
        }
    }

    private static object? ConvertFromSource(TrackableProperty property, object? value)
    {
        foreach (var attribute in property.GetCustomAttributes<IStateConverter>(true))
        {
            value = attribute.ConvertFromSource(value, property.PropertyType, property);
        }

        return value;
    }

    private static object? ConvertToSource(TrackableProperty property, object? value)
    {
        foreach (var attribute in property.GetCustomAttributes<IStateConverter>(true))
        {
            value = attribute.ConvertToSource(value, property);
        }

        return value;
    }
}