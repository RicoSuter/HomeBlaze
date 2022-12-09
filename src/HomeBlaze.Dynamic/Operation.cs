using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using HomeBlaze.Abstractions;

namespace HomeBlaze.Dynamic
{
    public class Operation
    {
        public string? ThingId { get; set; }

        public string? OperationName { get; set; }

        public Dictionary<string, object?> Parameters { get; set; } = new Dictionary<string, object?>();

        public ThingOperation? GetOperation(IThingManager thingManager)
        {
            var thing = thingManager.TryGetById(ThingId);
            if (thing != null)
            {
                var operations = thingManager.GetOperations(thing, true);
                return operations.SingleOrDefault(o => o.Name == OperationName);
            }

            return null;
        }

        public async Task<bool> ExecuteAsync(IThingManager thingManager, ILogger logger, CancellationToken cancellationToken)
        {
            var operation = GetOperation(thingManager);
            if (operation != null)
            {
                return await operation.ExecuteAsync(Parameters, logger, cancellationToken);
            }
            else
            {
                logger.LogWarning("Operation {OperationName} of Thing {ThingId} could not be resolved in operation.", OperationName, ThingId);
            }

            return false;
        }
    }
}
