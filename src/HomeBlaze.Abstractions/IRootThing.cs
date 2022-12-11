namespace HomeBlaze.Abstractions
{
    public interface IRootThing : IThing
    {
        ICollection<IThing> Things { get; }
    }
}