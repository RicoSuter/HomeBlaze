using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions
{
    public interface IGroupThing : IThing
    {
        [State]
        IEnumerable<IThing> Things { get; }

        void AddThing(IThing thing);

        void RemoveThing(IThing thing);

        void MoveThingUp(IThing thing);

        void MoveThingDown(IThing thing);
    }
}