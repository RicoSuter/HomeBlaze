using Namotion.Trackable.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Namotion.Trackable.Model;

public class PropertyReflectionMetadata
{
    private readonly PropertyInfo _propertyInfo;
    private readonly ITrackablePropertyInitializer[] _propertyInitializers;

    public PropertyReflectionMetadata(PropertyInfo propertyInfo, ITrackablePropertyInitializer[] propertyInitializers)
    {
        _propertyInfo = propertyInfo;
        _propertyInitializers = propertyInitializers;

        PropertyType = propertyInfo.PropertyType;

        Attributes = propertyInfo.GetCustomAttributes(true);
        TrackableAttribute = Attributes.OfType<TrackableAttribute>().FirstOrDefault();

        var processors = Attributes.Concat(_propertyInitializers)
            .OfType<IPropertyProcessor>()
            .Where(p => p.CanProcess(propertyInfo.PropertyType))
            .ToArray();

        PropertyInitializers = processors.OfType<ITrackablePropertyInitializer>().ToArray();

        Name = propertyInfo.Name;

        IsTrackable = TrackableAttribute != null;

        IsRequired = Attributes.Any(a => a is RequiredAttribute || a.GetType().FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute") &&
            propertyInfo.PropertyType.IsClass &&
            propertyInfo.PropertyType.FullName?.StartsWith("System.") == false;

        IsVirtual =
            propertyInfo.GetMethod?.IsVirtual == true ||
            propertyInfo.SetMethod?.IsVirtual == true;

        IsReadable = propertyInfo.GetMethod != null;
        IsWriteable = propertyInfo.SetMethod != null;
        IsDerived = propertyInfo.SetMethod == null;
    }

    public string Name { get; }

    public Type PropertyType { get; }

    public bool IsTrackable { get; }

    public bool IsRequired { get; }

    public bool IsVirtual { get; }

    public object[] Attributes { get; }

    public TrackableAttribute? TrackableAttribute { get; }

    public ITrackablePropertyInitializer[] PropertyInitializers { get; }

    public bool IsReadable { get; }

    public bool IsWriteable { get; }

    public bool IsDerived { get; }

    internal TrackedProperty CreateProperty(ProxyTracker parent, IObserver<TrackedPropertyChange> observer)
    {
        if (!IsVirtual)
        {
            throw new InvalidOperationException($"Trackable property {_propertyInfo.DeclaringType?.Name}.{_propertyInfo.Name} must be virtual.");
        }

        return TrackableAttribute?.CreateProperty(this, parent, observer) ??
            throw new InvalidOperationException($"{nameof(TrackableAttribute)} is null.");
    }

    public void SetValue(ITrackable trackable, object? value)
    {
        _propertyInfo.SetValue(trackable, value);
    }

    public object? GetValue(ITrackable trackable)
    {
        return _propertyInfo.GetValue(trackable);
    }
}
