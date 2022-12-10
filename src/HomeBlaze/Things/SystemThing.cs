using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Components.Dialogs;
using MudBlazor;
using System.Reflection;

namespace HomeBlaze.Things
{
    public class SystemThing : IThing, IIconProvider
    {
        private readonly IThingManager _thingManager;

        public string? Id => "system." + InternalId;

        public string Title => "System";

        [Configuration(IsIdentifier = true)]
        public string InternalId { get; set; } = Guid.NewGuid().ToString();

        [Configuration, State]
        public List<IThing> Things { get; set; } = new List<IThing>();

        [State]
        public SystemDiagnostics SystemDiagnostics { get; }

        [State]
        public PluginManager PluginManager { get; }

        [Configuration, State]
        public List<Dashboard> Dashboards { get; set; } = new List<Dashboard> { new Dashboard { Name = "home", Icon = "home" } };

        public string IconName => "fab fa-hubspot";

        public SystemThing(IThingManager thingManager, ITypeManager typeManager, IEventManager eventManager, ILogger<PollingThing> logger)
        {
            _thingManager = thingManager;

            SystemDiagnostics = new SystemDiagnostics(this, thingManager, eventManager, logger);
            PluginManager = new PluginManager(this, typeManager);
        }


        [Operation(Title = "Add Thing")]
        public async Task AddThingAsync(IDialogService dialogService, CancellationToken cancellationToken)
        {
            var thing = await ThingSetupDialog.AddThingAsync(dialogService, t => t.GetCustomAttribute<ThingWidgetAttribute>() == null);
            if (thing != null)
            {
                Things.Add(thing);
                await _thingManager.WriteConfigurationAsync(cancellationToken);

                _thingManager.DetectChanges(this);
            }
        }

        public async Task EditThingAsync(IThing thing, IDialogService dialogService, CancellationToken cancellationToken)
        {
            var hasChanged = await ThingSetupDialog.EditThingAsync(dialogService, thing);
            if (hasChanged)
            {
                await _thingManager.WriteConfigurationAsync(cancellationToken);
                _thingManager.DetectChanges(thing);
            }
        }

        public async Task DeleteThingAsync(IThing thing, IDialogService dialogService, CancellationToken cancellationToken)
        {
            var delete = await dialogService.ShowMessageBox("Delete Thing", "Do you really want to delete this Thing?", "Delete", "No");
            if (delete == true)
            {
                Things.Remove(thing);
                await _thingManager.WriteConfigurationAsync(cancellationToken);
            }
        }
    }
}