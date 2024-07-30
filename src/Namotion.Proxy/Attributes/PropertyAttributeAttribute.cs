namespace Namotion.Proxy.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class PropertyAttributeAttribute : Attribute
{
    public PropertyAttributeAttribute(string propertyName, string attributeName)
    {
        PropertyName = propertyName;
        AttributeName = attributeName;
    }

    public string PropertyName { get; }

    public string AttributeName { get; }
}
