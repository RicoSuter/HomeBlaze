using HomeBlaze.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Components
{
    public interface IThingSetupComponent
    {
        bool IsDirty { get; }

        bool IsValid { get; }

        IThingEditPage? Page { get; set; }

        Task<IThing?> CreateThingAsync(CancellationToken cancellationToken);

        Task<bool> UpdateEditedThingAsync(CancellationToken cancellationToken);
    }
}