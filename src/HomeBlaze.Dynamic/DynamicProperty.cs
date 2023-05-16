using DynamicExpresso;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Components.Editors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeBlaze.Dynamic
{
    public class DynamicProperty
    {
        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public string? Name { get; set; }

        public string? Expression { get; set; }

        public object? Evaluate(IEnumerable<PropertyVariable> variables, Type returnType, IThingManager thingManager, ILogger logger)
        {
            if (Expression == null)
            {
                return null;
            }

            var interpreter = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LateBindObject);

            foreach (var variable in variables)
            {
                var value = variable.TryGetValue(thingManager);
                interpreter.SetVariable(variable.ActualName, value!);
            }

            try
            {
                var objectResult = interpreter
                    .Eval(Expression);

                var json = JsonSerializer.Serialize(objectResult, _serializerOptions);
                var result = JsonSerializer.Deserialize(json, returnType, _serializerOptions);
                return result;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to evaluate dynamic property.");
                return null;
            }
        }
    }
}