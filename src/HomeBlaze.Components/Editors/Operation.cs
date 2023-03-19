using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using HomeBlaze.Abstractions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace HomeBlaze.Components.Editors
{
    public class Operation
    {
        public OperationType Type { get; set; } = OperationType.ThingOperation;

        public string? Code { get; set; }

        public List<ThingVariable> Variables { get; set; } = new List<ThingVariable>();

        public string? ThingId { get; set; }

        public string? Name { get; set; }

        public Dictionary<string, object?> Parameters { get; set; } = new Dictionary<string, object?>();

        public void Initialize(string name, IThingManager thingManager)
        {
            Name = name;
            Parameters = GetOperation(thingManager)?
                .CreateParameters() ?? new Dictionary<string, object?>();
        }

        public async Task<bool> ExecuteAsync(IThingManager thingManager, ILogger logger, CancellationToken cancellationToken)
        {
            if (Type == OperationType.ThingOperation)
            {
                var operation = GetOperation(thingManager);
                if (operation != null)
                {
                    try
                    {
                        return await operation.ExecuteAsync(Parameters, logger, cancellationToken);
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                {
                    logger.LogWarning("The operation {OperationName} of Thing {ThingId} could not be resolved.", Name, ThingId);
                }

                return false;
            }
            else if (Type == OperationType.CSharp)
            {
                var options = ScriptOptions.Default
                    .AddImports("System", "System.Threading", "System.Threading.Tasks")
                    .AddReferences(
                        GetType().Assembly,
                        typeof(string).Assembly,
                        typeof(IThing).Assembly);

                var initializerCode = "";
                var things = new Dictionary<string, object>();
                foreach (var variable in Variables.Where(v => !string.IsNullOrEmpty(v.Name)))
                {
                    var thing = thingManager.TryGetById(variable.ThingId);
                    if (thing != null)
                    {
                        var thingType = thing.GetType();
                      
                        options = options.AddReferences(thingType.Assembly);
                        things.Add(variable.Name!, thing);

                        initializerCode += $"var {variable.Name} = ({thingType.FullName})Things[\"{variable.Name}\"];\n";
                    }
                }

                var state = await CSharpScript.RunAsync(initializerCode + Code, options, new OperationModel
                { 
                    Things = things,
                    logger = logger,
                }, cancellationToken: cancellationToken);
                return state.Exception != null;
            }

            return false;
        }

        public class OperationModel
        {
            public Dictionary<string, object>? Things { get; set; }

            public ILogger? logger { get; set; }
        }

        private ThingOperation? GetOperation(IThingManager thingManager)
        {
            var operations = thingManager.GetOperations(ThingId, true);
            return operations.SingleOrDefault(o => o.Name == Name);
        }
    }
}
