using Microsoft.AspNetCore.Components;

using HomeBlaze.Abstractions;
using System.Threading.Tasks;
using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using HomeBlaze.Services.Abstractions;

namespace HomeBlaze.Components
{
    public abstract class ThingSetupComponent<TThing> : ThingSetupComponentBase<TThing>
        where TThing : IThing
    {
#pragma warning disable CS8618 // class ensures it is never null after OnInitialized

        public TThing Thing { get; private set; }

#pragma warning restore CS8618

        public override bool IsDirty => 
            EditedThing != null &&
            ThingSerializer?.SerializeThing(Thing) != ThingSerializer?.SerializeThing(EditedThing);

        [Inject]
        public IThingSerializer? ThingSerializer { get; set; }

        [Inject]
        public IServiceProvider? ServiceProvider { get; set; }

        protected override void OnInitialized()
        {
            if (IsEditing)
            {
                Thing = ThingSerializer!.CloneThing(EditedThing!);
            }
            else
            {
                Thing = (TThing)ActivatorUtilities.CreateInstance(ServiceProvider!, typeof(TThing));

                if (ExtendedThing != null && Thing is ExtensionThing extensionThing)
                {
                    extensionThing.ExtendedThingId = ExtendedThing.Id;
                }
            }
        }

        public override Task<TThing?> CreateThingAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<TThing?>(Thing);
        }

        public override Task<bool> UpdateEditedThingAsync(CancellationToken cancellationToken)
        {
            ThingSerializer!.PopulateThing(Thing!, EditedThing!);
            return Task.FromResult(true);
        }
    }
}