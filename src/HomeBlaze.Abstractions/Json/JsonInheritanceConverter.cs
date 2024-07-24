using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable disable

// TODO: Take from NJS when released

namespace HomeBlaze.Abstractions.Json
{
    /// <summary>Defines the class as inheritance base class and adds a discriminator property to the serialized object.</summary>
    internal class JsonInheritanceConverter<TBase> : JsonConverter<TBase>
    {
        private readonly string _discriminatorName;

        /// <summary>Gets the list of additional known types.</summary>
        public static IDictionary<string, Type> AdditionalKnownTypes { get; } = new Dictionary<string, Type>();

        /// <summary>Initializes a new instance of the <see cref="JsonInheritanceConverter{TBase}"/> class.</summary>
        public JsonInheritanceConverter()
        {
            var attribute = typeof(TBase).GetCustomAttribute<JsonInheritanceConverterAttribute>();
            _discriminatorName = attribute?.DiscriminatorName ?? "discriminator";
        }

        /// <summary>Initializes a new instance of the <see cref="JsonInheritanceConverter{TBase}"/> class.</summary>
        /// <param name="discriminatorName">The discriminator name.</param>
        public JsonInheritanceConverter(string discriminatorName)
        {
            _discriminatorName = discriminatorName;
        }

        /// <summary>Gets the discriminator property name.</summary>
        public virtual string DiscriminatorName => _discriminatorName;

        /// <inheritdoc />
        public override TBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var document = JsonDocument.ParseValue(ref reader);
            var hasDiscriminator = document.RootElement.TryGetProperty(_discriminatorName, out var discriminator);
            var subtype = GetDiscriminatorType(document.RootElement, typeToConvert, hasDiscriminator ? discriminator.GetString() : null);

            var bufferWriter = new MemoryStream();
            using (var writer = new Utf8JsonWriter(bufferWriter))
            {
                document.RootElement.WriteTo(writer);
            }

            return (TBase)JsonSerializer.Deserialize(bufferWriter.ToArray(), subtype, options);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, TBase value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(_discriminatorName, GetDiscriminatorValue(value.GetType()));

            var bytes = JsonSerializer.SerializeToUtf8Bytes((object)value, options);
            var document = JsonDocument.Parse(bytes);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                property.WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        /// <summary>Gets the discriminator value for the given type.</summary>
        /// <param name="type">The object type.</param>
        /// <returns>The discriminator value.</returns>
        public virtual string GetDiscriminatorValue(Type type)
        {
            var knownType = AdditionalKnownTypes.SingleOrDefault(p => p.Value == type);
            if (knownType.Key != null)
            {
                return knownType.Key;
            }

            return type.Name;
        }

        /// <summary>Gets the type for the given discriminator value.</summary>
        /// <param name="jObject">The JSON object.</param>
        /// <param name="objectType">The object (base) type.</param>
        /// <param name="discriminatorValue">The discriminator value.</param>
        /// <returns></returns>
        protected virtual Type GetDiscriminatorType(JsonElement jObject, Type objectType, string discriminatorValue)
        {
            if (AdditionalKnownTypes.ContainsKey(discriminatorValue))
            {
                return AdditionalKnownTypes[discriminatorValue];
            }

            throw new InvalidOperationException("Could not find subtype of '" + objectType.Name + "' with discriminator '" + discriminatorValue + "'.");
        }

        private static Type GetSubtypeFromKnownTypeAttributes(Type objectType, string discriminatorValue)
        {
            var type = objectType;
            do
            {
                var knownTypeAttributes = type
                    .GetTypeInfo()
                    .GetCustomAttributes(false)
                    .Where(a => a.GetType().Name == "KnownTypeAttribute");

                foreach (dynamic attribute in knownTypeAttributes)
                {
                    if (attribute.Type != null && attribute.Type.Name == discriminatorValue)
                    {
                        return attribute.Type;
                    }
                    else if (attribute.MethodName != null)
                    {
                        var method = type.GetRuntimeMethod((string)attribute.MethodName, new Type[0]);
                        if (method != null)
                        {
                            var types = (IEnumerable<Type>)method.Invoke(null, new object[0]);
                            foreach (var knownType in types)
                            {
                                if (knownType.Name == discriminatorValue)
                                {
                                    return knownType;
                                }
                            }
                            return null;
                        }
                    }
                }

                type = type.GetTypeInfo().BaseType;
            } while (type != null);

            return null;
        }
    }
}