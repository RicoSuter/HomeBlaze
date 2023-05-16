using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Logging;
using HomeBlaze.Abstractions.Presentation;
using System.ComponentModel;
using HomeBlaze.Components.Editors;
using System.Threading.Tasks;
using System.Threading;
using HomeBlaze.Abstractions;
using System;

namespace HomeBlaze.Dynamic
{
    [DisplayName("Operation Thing")]
    [ThingSetup(typeof(OperationThingSetup), CanEdit = true, CanClone = true)]
    public class OperationThing : IThing, IIconProvider
    {
        private readonly IThingManager _thingManager;
        private readonly ILogger<OperationThing> _logger;

        public string IconName => "fa-solid fa-scroll";

        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Configuration]
        public string? Title { get; set; }

        [Configuration]
        public Operation Operation { get; set; } = new Operation
        {
            Type = OperationType.CSharp
        };

        public OperationThing(
            IThingManager thingManager,
            ILogger<OperationThing> logger)
        {
            _thingManager = thingManager;
            _logger = logger;
        }

        [Operation]
        public async Task<bool> ExecuteOperationAsync(CancellationToken cancellationToken)
        {
            var operation = Operation;
            if (operation != null)
            { 
                var result = await operation.ExecuteAsync(_thingManager, _logger, cancellationToken);
                if (result == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}