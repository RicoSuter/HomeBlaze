using HomeBlaze.Abstractions;

namespace HomeBlaze.Services.Extensions
{
    public interface IUpdateThing<T>
    {
        void Update(T data);
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<TThing> CreateOrUpdate<TThing, TInput>(
            this IEnumerable<TInput> newThings,
            IEnumerable<IThing> existingThings,
            Func<TInput, TThing, bool> compare,
            Func<TInput, TThing> create)
            where TThing : IThing, IUpdateThing<TInput>
        {
            return newThings
                .Select(d =>
                {
                    var existingThing = existingThings
                        .OfType<TThing>()
                        .SingleOrDefault(t => compare(d, t));

                    if (existingThing != null)
                    {
                        existingThing.Update(d);
                        return existingThing;
                    }
                    else
                    {
                        return create(d);
                    }
                });
        }
    }
}
