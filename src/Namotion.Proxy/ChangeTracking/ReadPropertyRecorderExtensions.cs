namespace Namotion.Proxy.ChangeTracking;

public static class ReadPropertyRecorderExtensions
{
    public static ReadPropertyRecorderScope BeginReadPropertyRecording(this IProxyContext context)
    {
        ReadPropertyRecorder.Scopes.Value =
            ReadPropertyRecorder.Scopes.Value ??
            new Dictionary<IProxyContext, List<HashSet<ProxyPropertyReference>>>();

        var scope = new HashSet<ProxyPropertyReference>();
        ReadPropertyRecorder.Scopes.Value.TryAdd(context, new List<HashSet<ProxyPropertyReference>>());
        ReadPropertyRecorder.Scopes.Value[context].Add(scope);

        return new ReadPropertyRecorderScope(context, scope);
    }
}
