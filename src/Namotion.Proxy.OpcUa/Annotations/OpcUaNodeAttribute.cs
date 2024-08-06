using Namotion.Proxy.Sources.Abstractions;
using Namotion.Proxy.Sources.Attributes;
using System.IO;

namespace Namotion.Proxy.OpcUa.Annotations;

public class OpcUaNodeAttribute : ProxySourcePathAttribute, IOpcUaBrowseNameProvider
{
    public OpcUaNodeAttribute(string browseName, string browseNamespace, string? sourceName = null, string? path = null)
        : base(sourceName ?? "opc", path ?? browseName)
    {
        BrowseName = browseName;
        BrowseNamespace = browseNamespace;
    }

    public string BrowseName { get; }

    public string BrowseNamespace { get; }
}


public class OpcUaVariableAttribute : ProxySourceAttribute, IOpcUaBrowseNameProvider
{
    public OpcUaVariableAttribute(string browseName, string browseNamespace, string? sourceName = null, string? path = null)
        : base(sourceName ?? "opc", path ?? browseName)
    {
        BrowseName = browseName;
        BrowseNamespace = browseNamespace;
    }

    public string BrowseName { get; }

    public string BrowseNamespace { get; }
}

public interface IOpcUaBrowseNameProvider
{
    string BrowseName { get; }

    string BrowseNamespace { get; }
}
