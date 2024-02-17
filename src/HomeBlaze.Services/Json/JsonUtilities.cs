using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace HomeBlaze.Services.Json
{
    public static class JsonUtilities
    {
        public static T Clone<T>(T obj, JsonSerializerOptions? options = null)
        {
            var json = JsonSerializer.Serialize(obj, options);
            return JsonSerializer.Deserialize<T>(json, options)!;
        }

        public static void Populate(object root, string json, JsonSerializerOptions? options = null)
        {
            PopulateTypeInfoResolver._root = root;
            JsonSerializer.Deserialize(json, root.GetType(), GetOptionsWithPopulateResolver(options));
        }

        public static T? PopulateOrDeserialize<T>(T? root, string json, JsonSerializerOptions? options = null)
            where T : class, new()
        {
            root = root ?? new T();
            PopulateTypeInfoResolver._root = root;
            return JsonSerializer.Deserialize<T>(json, GetOptionsWithPopulateResolver(options));
        }

        private class PopulateTypeInfoResolver : IJsonTypeInfoResolver
        {
            [ThreadStatic]
            internal static object? _root;

            private readonly IJsonTypeInfoResolver _jsonTypeInfoResolver;

            public PopulateTypeInfoResolver(IJsonTypeInfoResolver jsonTypeInfoResolver)
            {
                _jsonTypeInfoResolver = jsonTypeInfoResolver;
            }

            public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
            {
                var typeInfo = _jsonTypeInfoResolver.GetTypeInfo(type, options);
                if (typeInfo is not null &&
                    type == _root?.GetType())
                {
                    var originalCreateObject = typeInfo.CreateObject;
                    typeInfo.CreateObject = () =>
                    {
                        if (_root is not null)
                        {
                            var root = _root;
                            _root = null;
                            return root;
                        }

                        return originalCreateObject!.Invoke();
                    };
                }

                return typeInfo;
            }
        }

        // Dynamically attach a JsonSerializerOptions copy that is configured using PopulateTypeInfoResolver
        private readonly static ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> _populateMap = new();

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
    }
}