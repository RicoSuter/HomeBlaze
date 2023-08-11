using Microsoft.Extensions.Logging;
using System.Reflection;
using HomeBlaze.Abstractions.Attributes;
using System.Text.Json;

namespace HomeBlaze.Abstractions
{
    public class ThingOperation
    {
        private readonly OperationAttribute _attribute;

        public ThingOperation(IThing sourceThing, string name, string description, MethodInfo method, OperationAttribute attribute)
        {
            SourceThing = sourceThing;
            Name = name;
            Description = description;
            Method = method;

            _attribute = attribute;
        }

        public string Id => Name + ":" + SourceThing.Id;

        public IThing SourceThing { get; }

        public string Name { get; }

        public string? Title => _attribute.Title;

        public string Description { get; }

        public MethodInfo Method { get; } // TODO: Hide this and expose meta model

        public Dictionary<string, object?> CreateParameters()
        {
            return Method
                .GetParameters()
                .Where(p => p.Name != null &&
                            p.ParameterType.IsInterface == false &&
                            p.ParameterType != typeof(CancellationToken))
                .ToDictionary(p => p.Name!, p => (object?)null);
        }

        public async Task<bool> ExecuteAsync(
            IReadOnlyDictionary<string, object?> parameters,
            ILogger logger, CancellationToken cancellationToken)
        {
            try
            {
                var index = 0;
                var parameterList = new List<object?>();
                foreach (var parameter in Method
                    .GetParameters()
                    .Where(p => p.Name != null))
                {
                    if (parameter.ParameterType == typeof(CancellationToken))
                    {
                        parameterList.Add(cancellationToken);
                    }
                    else
                    {
                        if (!parameters.TryGetValue(parameter.Name!, out var parameterValue))
                        {
                            parameterValue = parameter.DefaultValue;
                        }

                        if (parameterValue == null)
                        {
                            parameterList.Add(null);
                        }
                        else if (parameterValue.GetType().IsAssignableTo(parameter.ParameterType))
                        {
                            parameterList.Add(parameterValue);
                        }
                        else if (parameterValue is JsonElement jsonStringElement && 
                                 jsonStringElement.ValueKind == JsonValueKind.String)
                        {
                            parameterValue = jsonStringElement.Deserialize<string>();
                            parameterList.Add(Convert.ChangeType(parameterValue, parameter.ParameterType));
                        }
                        else if (parameterValue is JsonElement jsonElement)
                        {
                            parameterList.Add(jsonElement.Deserialize(parameter.ParameterType));
                        }
                        else
                        {
                            parameterList.Add(Convert.ChangeType(parameterValue, parameter.ParameterType));
                        }
                    }
                    index++;
                }

                var task = Method?.Invoke(SourceThing, parameterList.ToArray()) as Task;
                if (task != null)
                {
                    await task;
                }

                logger.LogInformation("Operation {OperationName} of {ThingId} successfully executed.", Name, SourceThing?.Id);
                return true;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to execute operation {OperationName} of {ThingId}.", Name, SourceThing?.Id);
                throw;
            }
        }
    }
}