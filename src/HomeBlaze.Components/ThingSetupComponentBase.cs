using Microsoft.AspNetCore.Components;

using HomeBlaze.Abstractions;
using System.Threading.Tasks;
using System.Threading;

namespace HomeBlaze.Components
{
    public abstract class ThingSetupComponentBase<TThing> : ComponentBase, IThingSetupComponent
        where TThing : IThing
    {
        private IThingEditPage? _page;

        public virtual bool IsDirty => false;

        /// <summary>
        /// Gets a value indicating whether the current editor's values are valid and the thing can be created or updated.
        /// </summary>
        public virtual bool IsValid => true;

        /// <summary>
        /// Gets a value indicating whether we are updating an existing thing.
        /// </summary>
        public bool IsEditing => EditedThing != null;

        /// <summary>
        /// Gets the original thing to be updated (null when creating a new thing).
        /// </summary>
        [Parameter]
        public TThing? EditedThing { get; set; }

        /// <summary>
        /// Gets the thing to be extended (only applies when creating a thing extension).
        /// </summary>
        [Parameter]
        public IThing? ExtendedThing { get; set; }

#pragma warning disable BL0007 // need to modify setter, behavior for Blazor is the same

        [Parameter]
        public IThingEditPage? Page
        {
            get => _page;
            set
            {
                _page = value;
                _page?.RegisterComponent(this);
            }
        }

#pragma warning restore BL0007

        public virtual Task<TThing?> CreateThingAsync(CancellationToken cancellationToken)
        {
            // intentionally left empty
            return Task.FromResult<TThing?>(default);
        }

        public virtual Task<bool> UpdateEditedThingAsync(CancellationToken cancellationToken)
        {
            // intentionally left empty
            return Task.FromResult(true);
        }

        async Task<IThing?> IThingSetupComponent.CreateThingAsync(CancellationToken cancellationToken)
        {
            return await CreateThingAsync(cancellationToken);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_page != null)
            {
                await _page.RefreshAsync();
            }
        }
    }
}