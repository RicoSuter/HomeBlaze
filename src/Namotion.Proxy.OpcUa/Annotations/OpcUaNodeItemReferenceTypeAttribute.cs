namespace Namotion.Proxy.OpcUa.Annotations;

public class OpcUaNodeItemReferenceTypeAttribute : Attribute
{
    public OpcUaNodeItemReferenceTypeAttribute(string type)
    {
        Type = type;
    }

    public string Type { get; }
}
