namespace HomeBlaze.Abstractions
{
    public interface IExtensionThing : IThing
    {
        IThing? ExtendedThing { get; }
    }
}