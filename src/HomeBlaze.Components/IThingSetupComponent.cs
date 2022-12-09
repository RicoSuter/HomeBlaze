using HomeBlaze.Abstractions;
using HomeBlaze.Components.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Components
{
    public interface IThingSetupComponent
    {
        bool IsValid { get; }

        ThingSetupDialog? Dialog { get; set; }

        Task<IThing?> CreateThingAsync(CancellationToken cancellationToken);

        Task<bool> UpdateEditedThingAsync(CancellationToken cancellationToken);
    }
}