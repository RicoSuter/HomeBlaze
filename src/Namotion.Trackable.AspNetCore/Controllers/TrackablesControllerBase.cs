﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using Microsoft.AspNetCore.Mvc;

using Namotion.Trackable.Attributes;
using Namotion.Trackable.Validation;

namespace Namotion.Trackable.AspNetCore.Controllers;

public abstract class TrackablesControllerBase<TTrackable> : ControllerBase
    where TTrackable : class
{
    private readonly static JsonSerializerOptions _options;

    private readonly TrackableContext<TTrackable> _trackableContext;

    protected TrackablesControllerBase(TrackableContext<TTrackable> trackableContext)
    {
        _trackableContext = trackableContext;
    }

    static TrackablesControllerBase()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        void RenameAttributeProperties(JsonTypeInfo typeInfo)
        {
            foreach (JsonPropertyInfo propertyInfo in typeInfo.Properties)
            {
                var variableAttribute = propertyInfo.AttributeProvider?
                    .GetCustomAttributes(true)
                    .OfType<AttributeOfTrackableAttribute>()
                    .FirstOrDefault();

                if (variableAttribute != null)
                {
                    propertyInfo.Name =
                        options.PropertyNamingPolicy.ConvertName(variableAttribute.PropertyName) + "@" +
                        options.PropertyNamingPolicy.ConvertName(variableAttribute.AttributeName);
                }
            }
        }

        options.Converters.Add(new JsonStringEnumConverter());
        options.TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { RenameAttributeProperties }
        };

        _options = options;
    }

    [HttpGet]
    public ActionResult<TTrackable> GetVariables()
    {
        var json = JsonSerializer.SerializeToElement(_trackableContext.Object, _options);
        return Ok(json);
    }

    [HttpPost]
    public ActionResult UpdateVariables(
        [FromBody] Dictionary<string, JsonElement> updates,
        [FromServices] IEnumerable<ITrackablePropertyValidator> propertyValidators)
    {
        try
        {
            var resolvedUpdates = updates
                .Select(t =>
                {
                    var variable = _trackableContext
                        .AllProperties
                        .SingleOrDefault(v => v.Path.ToLowerInvariant() == t.Key.ToLowerInvariant());

                    return new
                    {
                        t.Key,
                        Variable = variable,
                        Value = variable != null ?
                            t.Value.Deserialize(variable.PropertyType) :
                            null
                    };
                })
                .ToArray();

            // check only known variables
            if (resolvedUpdates.Any(u => u.Variable == null))
            {
                return BadRequest(new ProblemDetails
                {
                    Detail = "Unknown variable paths."
                });
            }

            // check not read-only
            if (resolvedUpdates.Any(u => !u.Variable!.IsWriteable))
            {
                return BadRequest(new ProblemDetails
                {
                    Detail = "Attempted to change read only variable."
                });
            }

            // run validators
            var errors = new Dictionary<string, ValidationResult[]>();
            foreach (var update in resolvedUpdates)
            {
                var updateErrors = propertyValidators
                    .SelectMany(v => v.Validate(update.Variable!, update.Value, _trackableContext))
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
                    Detail = "Variable updates not valid.",
                    Extensions =
                    {
                        { "errors", errors.ToDictionary(e => e.Key, e => e.Value.Select(v => v.ErrorMessage)) }
                    }
                });
            }

            // write updates
            foreach (var update in resolvedUpdates)
            {
                update.Variable!.SetValue(update.Value);
            }

            return Ok();
        }
        catch (JsonException)
        {
            return BadRequest(new ProblemDetails
            {
                Detail = "Invalid variable value."
            });
        }
    }

    /// <summary>
    /// Gets all leaf properties.
    /// </summary>
    /// <returns></returns>
    [HttpGet("properties")]
    public ActionResult GetProperties()
    {
        var allTrackers = _trackableContext.AllTrackers;
        return Ok(_trackableContext
            .AllProperties
            .Where(p => !p.IsAttribute && allTrackers.Any(t => t.ParentProperty == p) == false));
    }
}
