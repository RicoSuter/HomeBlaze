namespace Namotion.Proxy.Lifecycle;

// TODO: Is this needed? Remove?

public static class ParentsHandlerExtensions
{
    private const string ParentsKey = "Namotion.Parents";

    public static void AddParent(this IProxy proxy, ProxyPropertyReference parent, object? index)
    {
        var parents = proxy.GetParents();
        parents.Add(new ProxyParent(parent, index));
    }

    public static void RemoveParent(this IProxy proxy, ProxyPropertyReference parent, object? index)
    {
        var parents = proxy.GetParents();
        parents.Remove(new ProxyParent(parent, index));
    }

    public static HashSet<ProxyParent> GetParents(this IProxy proxy)
    {
        return (HashSet<ProxyParent>)proxy.Data.GetOrAdd(ParentsKey, (_) => new HashSet<ProxyParent>())!;
    }
}

public record struct ProxyParent(
    ProxyPropertyReference Property,
    object? Index)
{
}
