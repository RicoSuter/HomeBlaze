using HomeBlaze.Abstractions;

namespace HomeBlaze.Abstractions.Services
{
    public interface IThingSerializer
    {
        T CloneThing<T>(T thing) where T : IThing;

        string SerializeThing<T>(T source) where T : IThing;

        void PopulateThing<T>(T target, string sourceJson) where T : IThing;

        void PopulateThing<T>(T source, T target) where T : IThing;
    }
}