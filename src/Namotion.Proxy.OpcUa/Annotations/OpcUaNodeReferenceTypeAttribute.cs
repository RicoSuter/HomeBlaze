namespace Namotion.Proxy.OpcUa.Annotations;

public class OpcUaNodeReferenceTypeAttribute : Attribute
{
    public OpcUaNodeReferenceTypeAttribute(string type)
    {
        Type = type;
    }

    public string Type { get; }
}
