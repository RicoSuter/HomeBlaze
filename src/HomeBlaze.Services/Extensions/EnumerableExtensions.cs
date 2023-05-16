using HomeBlaze.Abstractions;

namespace HomeBlaze.Services.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TThing> CreateOrUpdate<TThing, TInput>(
            this IEnumerable<TInput> newThings,
            IEnumerable<IThing> existingThings,
            Func<TInput, TThing, bool> compare,
            Action<TThing, TInput> update,
            Func<TInput, TThing> create)
            where TThing : IThing
        {
            return newThings
                .Select(newThing =>
                {
                    var existingThing = existingThings
                        .OfType<TThing>()
                        .SingleOrDefault(existingThing => compare(newThing, existingThing));

                    if (existingThing != null)
                    {
                        update(existingThing, newThing);
                        return existingThing;
                    }
                    else
                    {
                        return create(newThing);
                    }
                });
        }
    }
}
