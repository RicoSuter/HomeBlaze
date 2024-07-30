namespace Namotion.Proxy.OpcUa.Annotations;

public class OpcUaTypeDefinitionAttribute : Attribute
{
    public OpcUaTypeDefinitionAttribute(string type, string? ns = null)
    {
        Type = type;
        Namespace = ns;
    }

    public string Type { get; }

    public string? Namespace { get; }
}
