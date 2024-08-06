namespace HomeBlaze.Services
{
    public interface IThingEditPage
    {
        void RegisterComponent(IThingSetupComponent component);

        Task RefreshAsync();
    }
}