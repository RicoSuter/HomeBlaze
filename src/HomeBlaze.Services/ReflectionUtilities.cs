using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using Microsoft.Extensions.Logging;
using Namotion.Reflection;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HomeBlaze.Services
{
    public static class ReflectionUtilities
    {
        private static readonly ConcurrentDictionary<Type, StatePropertyInfo[]> _propertyCache =
            new ConcurrentDictionary<Type, StatePropertyInfo[]>();

        public class StatePropertyInfo
        {
            public required ContextualPropertyInfo Property { get; init; }

            public StateAttribute? StateAttribute { get; init; }

            public ScanForStateAttribute? ScanForStateAttribute { get; init; }

            public ParentThingAttribute? ParentThingAttribute { get; init; }

            public bool IsThingProperty { get; init; }

            public bool IsThingArrayProperty { get; init; }

            public object? TryGetValue(object? obj, ILogger logger)
            {
                try
                {
                    return obj != null ? Property.GetValue(obj) : null;
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, $"Getter of property threw an exception.");
                    return null;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetValue(object? obj, object? value)
            {
                Property.SetValue(obj, value);
            }
        }

        public static StatePropertyInfo[] GetStateProperties(this Type type)
        {
            // TODO: Use namotion.reflection types
            if (!_propertyCache.TryGetValue(type, out var pair))
            {
                var interfaceProperties = type
                    .GetInterfaces()
                    .SelectMany(i => i.GetProperties()
                        .Where(property => property.GetCustomAttribute<StateAttribute>(true) != null))
                    .ToArray();

                var properties = type
                    .GetRuntimeProperties()
                    .Where(property => property.GetCustomAttribute<StateAttribute>(true) != null ||
                                       property.GetCustomAttribute<ScanForStateAttribute>(true) != null ||
                                       property.GetCustomAttribute<ParentThingAttribute>(true) != null)
                    .ToArray();

                _propertyCache[type] = interfaceProperties
                    .Concat(properties)
                    .DistinctBy(p => p.Name)
                    .Select(p => p.ToContextualProperty())
                    .Select(p => new StatePropertyInfo
                    {
                        Property = p,
                        StateAttribute = p.ContextAttributes.OfType<StateAttribute>().FirstOrDefault(),
                        ScanForStateAttribute = p.ContextAttributes.OfType<ScanForStateAttribute>().FirstOrDefault(),
                        ParentThingAttribute = p.ContextAttributes.OfType<ParentThingAttribute>().FirstOrDefault(),
                        IsThingProperty = p.PropertyType.Type.IsAssignableTo(typeof(IThing)),
                        IsThingArrayProperty = p.PropertyType.Type.IsAssignableTo(typeof(IEnumerable<IThing>))
                    })
                    .ToArray();
            }

            return _propertyCache[type];
        }
    }
}