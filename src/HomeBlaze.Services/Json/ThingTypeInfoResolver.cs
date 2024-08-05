using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace HomeBlaze.Services.Json
{
    public class ThingTypeInfoResolver : DefaultJsonTypeInfoResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly bool _serializeIdentifier;

        public ThingTypeInfoResolver(IServiceProvider serviceProvider, bool serializeIdentifier)
        {
            _serviceProvider = serviceProvider;
            _serializeIdentifier = serializeIdentifier;
        }

        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var typeInfo = base.GetTypeInfo(type, options);
            if (type.IsAssignableTo(typeof(IThing)) &&
                type != typeof(IThing))
            {
                var baseProperties = base.GetTypeInfo(type.BaseType!, options).Properties;
                var properties = typeInfo
                   .Properties
                   .Select(p => new
                   {
                        Property = p,
                        Attribute = p
                            .AttributeProvider?
                            .GetCustomAttributes(true)
                            .OfType<ConfigurationAttribute>()
                            .FirstOrDefault() ??

                            baseProperties
                                .FirstOrDefault(x => x.Name == p.Name)?
                                .AttributeProvider?
                                .GetCustomAttributes(true)
                                .OfType<ConfigurationAttribute>()
                                .FirstOrDefault()
                   })
                   .ToArray();

                foreach (var pair in properties)
                {
                    if (pair.Attribute == null)
                    {
                        typeInfo.Properties.Remove(pair.Property);
                    }
                    else if (!_serializeIdentifier && (pair.Property.Name == nameof(IThing.Id) || pair.Attribute.IsIdentifier))
                    {
                        typeInfo.Properties.Remove(pair.Property);
                    }
                    else if (pair.Attribute.Name != null)
                    {
                        pair.Property.Name = pair.Attribute.Name;
                    }
                }

                typeInfo.CreateObject = () =>
                {
                    return ActivatorUtilities.CreateInstance(_serviceProvider, type);
                };
            }

            return typeInfo;
        }
    }
}