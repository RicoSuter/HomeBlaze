namespace Namotion.Proxy.ChangeTracking;

public class ReadPropertyRecorderScope : IDisposable
{
    private readonly IProxyContext _context;
    private readonly HashSet<ProxyPropertyReference> _properties;

    public ReadPropertyRecorderScope(IProxyContext context, HashSet<ProxyPropertyReference> properties)
    {
        _context = context;
        _properties = properties;
    }

    public ProxyPropertyReference[] Properties
    {
        get
        {
            lock (typeof(ReadPropertyRecorder))
            {
                return _properties.ToArray();
            }
        }
    }

    public ProxyPropertyReference[] GetPropertiesAndReset()
    {
        lock (typeof(ReadPropertyRecorder))
        {
            var properties = _properties.ToArray();
            _properties.Clear();
            return properties;
        }
    }

    public ProxyPropertyReference[] GetPropertiesAndDispose()
    {
        lock (typeof(ReadPropertyRecorder))
        {
            var properties = _properties.ToArray();
            Dispose();
            return properties;
        }
    }

    public void Dispose()
    {
        lock (typeof(ReadPropertyRecorder))
        {
            ReadPropertyRecorder.Scopes.Value?[_context]?.Remove(_properties);
        }
    }
}
