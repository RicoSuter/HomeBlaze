using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Security
{
    public interface IAuthenticatedThing
    {
        [State]
        bool IsAuthenticated { get; }
    }
}
