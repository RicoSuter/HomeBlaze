using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using MudBlazor;

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;

using Namotion.Reflection;

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

        public override bool IsValid
        {
            get
            {
                Form?.Validate();
                return Form?.IsValid == true;
            }
        }

        protected MudForm? Form { get; set; }

        protected override void OnInitialized()
        {
            if (IsEditing)
            {
                Thing = ThingSerializer!.CloneThing(EditedThing!);
            }
            else
            {
                Thing = CreateThing();

                if (ExtendedThing != null &&
                    Thing is IExtensionThing extensionThing &&
                    extensionThing.HasProperty("ExtendedThingId"))
                {
                    // TODO: Add an interface for that
                    ((dynamic)extensionThing).ExtendedThingId = ExtendedThing.Id;
                }
            }
        }

        protected virtual TThing CreateThing()
        {
            return (TThing)ActivatorUtilities.CreateInstance(ServiceProvider!, typeof(TThing));
        }

        public override Task<TThing?> CreateThingAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<TThing?>(Thing);
        }

        public override Task<bool> UpdateEditedThingAsync(CancellationToken cancellationToken)
        {
            ThingSerializer!.PopulateThing(Thing!, EditedThing!);
           
            if (EditedThing is PollingThing pollingThing) // TODO: Use interface with Reset method
            {
                pollingThing.Reset();
            }

            return Task.FromResult(true);
        }
    }
}