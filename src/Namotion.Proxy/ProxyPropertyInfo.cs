namespace Namotion.Proxy;

public record struct ProxyPropertyInfo(
    string Name, // TODO: Remove as already defined as key in the dictionary
    Type Type,
    object[] Attributes,
    Func<object?, object?>? GetValue,
    Action<object?, object?>? SetValue)
{
    public readonly bool IsDerived => Attributes.Any(a => a is DerivedAttribute);
}
