using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Security
{
    public interface IAuthenticatedThing : IThing
    {
        [State]
        bool IsAuthenticated { get; }
    }
}
