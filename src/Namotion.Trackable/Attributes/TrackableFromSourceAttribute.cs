using System;

namespace Namotion.Trackable.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TrackableFromSourceAttribute : TrackableAttribute
{
    public string? RelativePath { get; set; }

    public string? AbsolutePath { get; set; }

    public int Length { get; set; }
}
