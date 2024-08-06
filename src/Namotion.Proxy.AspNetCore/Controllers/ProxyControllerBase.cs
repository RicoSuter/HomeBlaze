using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Mvc;

using Namotion.Proxy.Attributes;
using Namotion.Proxy.Registry.Abstractions;
using Namotion.Proxy.Validation;

namespace Namotion.Proxy.AspNetCore.Controllers;

public abstract class ProxyControllerBase<TProxy> : ControllerBase
    where TProxy : IProxy
{
    private readonly TProxy _proxy;
    private readonly IProxyContext _context;

    protected ProxyControllerBase(TProxy proxy)
    {
        _proxy = proxy;
        _context = proxy.Context ?? throw new ArgumentException($"The proxy context is null.");
    }

    [HttpGet]
    public ActionResult<TProxy> GetVariables()
    {
        // TODO: correctly generate OpenAPI schema
        return Ok(_proxy.ToJsonObject());
    }

    [HttpPost]
    public ActionResult UpdateVariables(
        [FromBody] Dictionary<string, JsonElement> updates,
        [FromServices] IEnumerable<IProxyPropertyValidator> propertyValidators)
    {
        try
        {
            var resolvedUpdates = updates
                .Select(t =>
                {
                    (var proxy, var property) = _proxy.FindPropertyFromJsonPath(t.Key);
                    return new
                    {
                        t.Key,
                        t.Value,
                        Proxy = proxy,
                        Property = property
                    };
                })
                .ToArray();

            // check only known variables
            if (resolvedUpdates.Any(u => u.Proxy == null))
            {
                return BadRequest(new ProblemDetails
                {
                    Detail = "Unknown property paths."
                });
            }

            // check not read-only
            if (resolvedUpdates.Any(u => u.Property.SetValue is null))
            {
                return BadRequest(new ProblemDetails
                {
                    Detail = "Attempted to change read only property."
                });
            }

            // run validators
            var errors = new Dictionary<string, ValidationResult[]>();
            foreach (var update in resolvedUpdates)
            {
                var updateErrors = propertyValidators
                    .SelectMany(v => v.Validate(
                        new ProxyPropertyReference(update.Proxy!, update.Property.Name), update.Value, _context))
                    .ToArray();

                if (updateErrors.Any())
                {
                    errors.Add(update.Key, updateErrors);
                }
            }

            if (errors.Any())
            {
                return BadRequest(new ProblemDetails
                {
                    Detail = "Property updates not valid.",
                    Extensions =
                    {
                        { "errors", errors.ToDictionary(e => e.Key, e => e.Value.Select(v => v.ErrorMessage)) }
                    }
                });
            }

            // write updates
            foreach (var update in resolvedUpdates)
            {
                update.Property.SetValue?.Invoke(update.Proxy, update.Value.Deserialize(update.Property.Type));
            }

            return Ok();
        }
        catch (JsonException)
        {
            return BadRequest(new ProblemDetails
            {
                Detail = "Invalid property value."
            });
        }
    }

    /// <summary>
    /// Gets all leaf properties.
    /// </summary>
    /// <returns></returns>
    [HttpGet("properties")]
    public ActionResult<ProxyDescription> GetProperties()
    {
        return Ok(CreateProxyDescription(_proxy, _context.GetHandler<IProxyRegistry>()));
    }

    private static ProxyDescription CreateProxyDescription(IProxy proxy, IProxyRegistry register)
    {
        var description = new ProxyDescription
        {
            Type = proxy.GetType().Name
        };

        if (register.KnownProxies.TryGetValue(proxy, out var metadata))
        {
            foreach (var property in metadata.Properties
                .Where(p => p.Value.HasGetter &&
                            p.Value.Attributes.OfType<PropertyAttributeAttribute>().Any() == false))
            {
                var propertyName = property.GetJsonPropertyName();
                var value = property.Value.GetValue();

                description.Properties[propertyName] = CreateDescription(register, metadata, property.Key, property.Value, value);
            }
        }

        return description;
    }

    public class ProxyDescription
    {
        public required string Type { get; init; }

        public Dictionary<string, ProxyPropertyDescription> Properties { get; } = new Dictionary<string, ProxyPropertyDescription>();
    }

    public class ProxyPropertyDescription
    {
        public required string Type { get; init; }

        public object? Value { get; internal set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public IReadOnlyDictionary<string, ProxyPropertyDescription>? Attributes { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public ProxyDescription? Proxy { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<ProxyDescription>? Proxies { get; set; }
    }

    private static ProxyPropertyDescription CreateDescription(IProxyRegistry registry, RegisteredProxy parent, 
        string propertyName, RegisteredProxyProperty property, object? value)
    {
        var attributes = parent.Properties
            .Where(p => p.Value.HasGetter &&
                        p.Value.Attributes.OfType<PropertyAttributeAttribute>().Any(a => a.PropertyName == propertyName))
            .ToDictionary(
                p => p.Value.Attributes.OfType<PropertyAttributeAttribute>().Single().AttributeName,
                p => CreateDescription(registry, parent, p.Key, p.Value, p.Value.GetValue()));

        var description = new ProxyPropertyDescription
        {
            Type = property.Type.Name,
            Attributes = attributes.Any() ? attributes : null
        };

        if (value is IProxy childProxy)
        {
            description.Proxy = CreateProxyDescription(childProxy, registry);
        }
        else if (value is ICollection collection && collection.OfType<IProxy>().Any())
        {
            var children = new List<ProxyDescription>();
            foreach (var arrayProxyItem in collection.OfType<IProxy>())
            {
                children.Add(CreateProxyDescription(arrayProxyItem, registry));
            }
            description.Proxies = children;
        }
        else
        {
            description.Value = value;
        }

        return description;
    }
}
