using HomeBlaze.Abstractions.Attributes;
using Microsoft.Extensions.Logging;
using Namotion.Reflection;
using System.Collections.Concurrent;
using System.Reflection;

namespace HomeBlaze.Services
{
    public static class ReflectionUtilities
    {
        private static readonly ConcurrentDictionary<Type, ContextualPropertyInfo[]> _propertyCache =
            new ConcurrentDictionary<Type, ContextualPropertyInfo[]>();

        public static object? TryGetPropertyValue(object? obj, ContextualPropertyInfo property, ILogger logger)
        {
            try
            {
                return obj != null ? property.GetValue(obj) : null;
            }
            catch (Exception e)
            {
                logger.LogWarning($"Getter of property threw an exception.", e);
                return null;
            }
        }

        public static ContextualPropertyInfo[] GetStateProperties(this Type type)
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
                    .ToArray();
            }

            return _propertyCache[type];
        }
    }
}