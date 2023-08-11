using HomeBlaze.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Host.Controllers
{
    [Route("/api/things")]
    public class ThingsController : Controller
    {
        private readonly IThingManager _thingManager;
        private readonly IThingSerializer _thingSerializer;
        private readonly ILogger<ThingsController> _logger;

        public ThingsController(IThingManager thingManager, IThingSerializer thingSerializer, ILogger<ThingsController> logger)
        {
            _thingManager = thingManager;
            _thingSerializer = thingSerializer;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAllThings()
        {
            return Json(new
            {
                items = _thingManager.AllThings.Select(t => new
                {
                    t.Id,
                    t.Title,
                    Type = t.GetType().FullName
                })
            });
        }

        [HttpGet("{*thingId}")]
        public async Task<IActionResult> GetThing(string thingId, CancellationToken cancellationToken)
        {
            if (thingId.EndsWith("/operations"))
            {
                var actualThingId = thingId[..thingId.LastIndexOf("/operations")];
                return GetThingOperationsAsync(actualThingId);
            }
            else if (thingId.Contains("/operations/"))
            {
                var actualThingId = thingId[..thingId.LastIndexOf("/operations/")];
                var operationName = thingId[(thingId.LastIndexOf("/operations/") + "/operations/".Length)..thingId.LastIndexOf("/execute")];
                return await ExecuteThingOperationAsync(actualThingId, operationName, cancellationToken);
            }

            var thing = _thingManager.TryGetById(thingId);
            if (thing != null)
            {
                return new ContentResult
                {
                    StatusCode = 200,
                    Content = _thingSerializer.SerializeThing(thing),
                    ContentType = MediaTypeNames.Application.Json
                };
            }
            else
            {
                return NotFound();
            }
        }

        private IActionResult GetThingOperationsAsync(string thingId)
        {
            var thing = _thingManager.TryGetById(thingId);
            if (thing != null)
            {
                var operations = _thingManager.GetOperations(thingId, true);
                return Json(new
                {
                    items = operations.Select(o => new
                    {
                        o.Name
                    })
                });
            }
            else
            {
                return NotFound(new { });
            }
        }

        private async Task<IActionResult> ExecuteThingOperationAsync(string thingId, string operationName, CancellationToken cancellationToken)
        {
            var thing = _thingManager.TryGetById(thingId);
            if (thing != null)
            {
                var operations = _thingManager.GetOperations(thingId, true);
                var operation = operations.SingleOrDefault(o => o.Name == operationName);
                if (operation != null)
                {
                    await operation.ExecuteAsync(new Dictionary<string, object?>(), _logger, cancellationToken);
                    return Ok(new { });
                }
            }

            return NotFound(new { });
        }
    }
}
