using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace HomeBlaze.Json
{
    public static class JsonUtilities
    {
        public static T Clone<T>(T obj, JsonSerializerOptions? options = null)
        {
            var json = JsonSerializer.Serialize(obj, options);
            return JsonSerializer.Deserialize<T>(json, options)!;
        }

        // Dynamically attach a JsonSerializerOptions copy that is configured using PopulateTypeInfoResolver
        private readonly static ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> _populateMap = new();

        public static void PopulateObject(string json, object destination, JsonSerializerOptions? options = null)
        {
            options = GetOptionsWithPopulateResolver(options);
            PopulateTypeInfoResolver._populateObject = destination;
            try
            {
                object? result = JsonSerializer.Deserialize(json, destination.GetType(), options);
                Debug.Assert(ReferenceEquals(result, destination));
            }
            finally
            {
                PopulateTypeInfoResolver._populateObject = null;
            }
        }

        private static JsonSerializerOptions GetOptionsWithPopulateResolver(JsonSerializerOptions? options)
        {
            options ??= JsonSerializerOptions.Default;

            if (!_populateMap.TryGetValue(options, out JsonSerializerOptions? populateResolverOptions))
            {
                // Force a serialization to mark options as read-only
                JsonSerializer.Serialize(value: 0, options);

                populateResolverOptions = new JsonSerializerOptions(options)
                {
                    TypeInfoResolver = new PopulateTypeInfoResolver(options.TypeInfoResolver!)
                };

                _populateMap.TryAdd(options, populateResolverOptions);
            }

            return populateResolverOptions;
        }

        private class PopulateTypeInfoResolver : IJsonTypeInfoResolver
        {
            private readonly IJsonTypeInfoResolver _jsonTypeInfoResolver;
            [ThreadStatic]
            internal static object? _populateObject;

            public PopulateTypeInfoResolver(IJsonTypeInfoResolver jsonTypeInfoResolver)
            {
                _jsonTypeInfoResolver = jsonTypeInfoResolver;
            }

            public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
            {
                var typeInfo = _jsonTypeInfoResolver.GetTypeInfo(type, options);
                if (typeInfo != null && typeInfo.Kind != JsonTypeInfoKind.None)
                {
                    Func<object>? defaultCreateObjectDelegate = typeInfo.CreateObject;
                    typeInfo.CreateObject = () =>
                    {
                        object? result = _populateObject;
                        if (result != null)
                        {
                            // clean up to prevent reuse in recursive scenaria
                            _populateObject = null;
                        }
                        else
                        {
                            // fall back to the default delegate
                            result = defaultCreateObjectDelegate?.Invoke();
                        }

                        return result!;
                    };
                }

                return typeInfo;
            }
        }
    }
}