using HomeBlaze.Abstractions;

namespace HomeBlaze.Services
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