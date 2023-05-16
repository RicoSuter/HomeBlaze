using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Components.Dialogs;
using HomeBlaze.Host;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Things
{
    public abstract class GroupBase
    {
        private readonly IThingManager _thingManager;

        [Configuration, State(Order = -1)]
        public IEnumerable<IThing> Things { get; set; } = new List<IThing>();

        public GroupBase(IThingManager thingManager)
        {
            _thingManager = thingManager;
        }

        [Operation(Title = "Add Thing")]
        public void AddThing(NavigationManager navigationManager)
        {
            navigationManager.NavigateToCreateThing(this as IThing);
        }

        [Operation(Title = "Add Group")]
        public async Task AddGroupAsync(IDialogService dialogService, CancellationToken cancellationToken)
        {
            var thing = await ThingSetupDialog.AddThingAsync(dialogService, t => t == typeof(Group));
            if (thing != null)
            {
                AddThing(thing);

                await _thingManager.WriteConfigurationAsync(cancellationToken);
                _thingManager.DetectChanges((IThing)this);
            }
        }

        public void AddThing(IThing thing)
        {
            lock (this)
            {
                Things = Things
                    .Concat(new IThing[] { thing })
                    .ToArray();
            }
        }

        public void RemoveThing(IThing thing)
        {
            lock (this)
            {
                Things = Things
                    .Where(t => t != thing)
                    .ToArray();
            }
        }

        public void MoveThingUp(IThing thing)
        {
            lock (this)
            {
                var things = Things.ToList();

                var index = things.IndexOf(thing);
                things.Insert(index - 1, thing);
                things.RemoveAt(index + 1);

                Things = things;
            }
        }

        public void MoveThingDown(IThing thing)
        {
            lock (this)
            {
                var things = Things.ToList();

                var index = things.IndexOf(thing);
                things.Insert(index + 2, thing);
                things.RemoveAt(index);

                Things = things;
            }
        }
    }
}