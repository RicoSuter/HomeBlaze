using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace HomeBlaze.Json
{
    public class ServiceProviderTypeInfoResolver : IJsonTypeInfoResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IJsonTypeInfoResolver _jsonTypeInfoResolver;

        public ServiceProviderTypeInfoResolver(IServiceProvider serviceProvider, IJsonTypeInfoResolver jsonTypeInfoResolver)
        {
            _serviceProvider = serviceProvider;
            _jsonTypeInfoResolver = jsonTypeInfoResolver;
        }

        public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var typeInfo = _jsonTypeInfoResolver.GetTypeInfo(type, options);

            if (typeInfo != null &&
                !type.IsArray &&
                type.IsClass &&
                type.FullName != "System.String" &&
                type.GetConstructors().All(c => c.GetParameters().Length > 0))
            {
                typeInfo.CreateObject = () => ActivatorUtilities.CreateInstance(_serviceProvider, type);
            }

            return typeInfo;
        }
    }
}