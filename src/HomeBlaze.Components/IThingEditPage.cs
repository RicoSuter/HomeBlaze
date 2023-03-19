using System.Threading.Tasks;

namespace HomeBlaze.Components
{
    public interface IThingEditPage
    {
        void RegisterComponent(IThingSetupComponent component);

        Task RefreshAsync();
    }
}