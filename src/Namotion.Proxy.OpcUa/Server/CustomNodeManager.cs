using System.Reactive.Linq;
using System.Reflection;
using Namotion.Proxy.Registry.Abstractions;
using Namotion.Proxy.OpcUa.Annotations;

using Opc.Ua.Server;
using Opc.Ua;
using Opc.Ua.Export;

namespace Namotion.Proxy.OpcUa.Server;

internal class CustomNodeManager<TProxy> : CustomNodeManager2
    where TProxy : IProxy
{
    private const string PathDelimiter = ".";

    private readonly TProxy _proxy;
    private readonly IProxyRegistry _registry;
    private readonly OpcUaServerTrackableSource<TProxy> _source;
    private readonly string? _rootName;

    private Dictionary<RegisteredProxy, FolderState> _proxies = new();

    public CustomNodeManager(
        TProxy proxy,
        OpcUaServerTrackableSource<TProxy> source,
        IServerInternal server,
        ApplicationConfiguration configuration,
        string? rootName) :
        base(server, configuration, new string[] 
        {
            "https://foobar/",
            "http://opcfoundation.org/UA/",
            "http://opcfoundation.org/UA/DI/",
            "http://opcfoundation.org/UA/PADIM",
            "http://opcfoundation.org/UA/Machinery/",
            "http://opcfoundation.org/UA/Machinery/ProcessValues"
        })
    {
        _proxy = proxy;
        _registry = proxy.Context?.GetHandler<IProxyRegistry>() ?? throw new ArgumentException($"Registry could not be found.");
        _source = source;
        _rootName = rootName;
    }

    protected override NodeStateCollection LoadPredefinedNodes(ISystemContext context)
    {
        var collection = base.LoadPredefinedNodes(context);

        LoadNodeSetFromEmbeddedResource<OpcUaNodeAttribute>("NodeSets.Opc.Ua.NodeSet2.xml", context, collection);
        LoadNodeSetFromEmbeddedResource<OpcUaNodeAttribute>("NodeSets.Opc.Ua.Di.NodeSet2.xml", context, collection);
        LoadNodeSetFromEmbeddedResource<OpcUaNodeAttribute>("NodeSets.Opc.Ua.PADIM.NodeSet2.xml", context, collection);
        LoadNodeSetFromEmbeddedResource<OpcUaNodeAttribute>("NodeSets.Opc.Ua.Machinery.NodeSet2.xml", context, collection);
        LoadNodeSetFromEmbeddedResource<OpcUaNodeAttribute>("NodeSets.Opc.Ua.Machinery.ProcessValues.NodeSet2.xml", context, collection);

        return collection;
    }

    public static void LoadNodeSetFromEmbeddedResource<TAssemblyType>(string name, ISystemContext context, NodeStateCollection nodes)
    {
        var assembly = typeof(TAssemblyType).Assembly;
        using var stream = assembly.GetManifestResourceStream($"{assembly.FullName!.Split(',')[0]}.{name}");
     
        var nodeSet = UANodeSet.Read(stream ?? throw new ArgumentException("Embedded resource could not be found.", nameof(name)));
        nodeSet.Import(context, nodes);
    }

    public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
    {
        base.CreateAddressSpace(externalReferences);

        var metadata = _registry.KnownProxies[_proxy];
        if (metadata is not null)
        {
            if (_rootName is not null)
            {
                var node = CreateFolder(ObjectIds.ObjectsFolder, new NodeId(_rootName, NamespaceIndex), _rootName, null, null);
                CreateObjectNode(node.NodeId, metadata, _rootName + PathDelimiter);
            }
            else
            {
                CreateObjectNode(ObjectIds.ObjectsFolder, metadata, string.Empty);
            }
        }
    }

    private void CreateObjectNode(NodeId parentNodeId, RegisteredProxy proxy, string prefix)
    {
        foreach (var property in proxy.Properties)
        {
            var propertyName = _source.SourcePathProvider.TryGetSourcePropertyName(property.Value.Property)!;

            var children = property.Value.Children;
            if (children.Count >= 1)
            {
                if (propertyName is not null)
                {
                    if (children.Count == 1 && children.All(c => c.Index is null))
                    {
                        CreateReferenceObjectNode(propertyName, property.Value, children.Single(), parentNodeId, prefix);
                    }
                    else if (children.All(c => c.Index is int))
                    {
                        CreateArrayObjectNode(propertyName, property.Value, property.Value.Children, parentNodeId, prefix);
                    }
                    else if (children.All(c => c.Index is not null))
                    {
                        CreateDictionaryObjectNode(propertyName, property.Value, property.Value.Children, parentNodeId, prefix);
                    }
                }
            }
            else
            {
                CreateVariableNode(propertyName, property, parentNodeId, prefix);
            }
        }
    }

    private void CreateReferenceObjectNode(string propertyName, RegisteredProxyProperty property, ProxyPropertyChild child, NodeId parentNodeId, string parentPath)
    {
        var path = parentPath + propertyName;
        var browseName = GetBrowseName(propertyName, property, child.Index);
        var referenceTypeId = GetReferenceTypeId(property);

        CreateChildObject(browseName, child.Proxy, path, parentNodeId, referenceTypeId);
    }

    private void CreateArrayObjectNode(string propertyName, RegisteredProxyProperty property, ICollection<ProxyPropertyChild> children, NodeId parentNodeId, string parentPath)
    {
        var nodeId = new NodeId(parentPath + propertyName, NamespaceIndex);
        var browseName = GetBrowseName(propertyName, property, null);

        var typeDefinitionId = GetTypeDefinitionId(property);
        var referenceTypeId = GetReferenceTypeId(property);

        var propertyNode = CreateFolder(parentNodeId, nodeId, browseName, typeDefinitionId, referenceTypeId);

        var childPrefix = parentPath + propertyName + PathDelimiter;
        var childReferenceTypeId = GetChildReferenceTypeId(property);
        foreach (var child in children)
        {
            var childBrowseName = new QualifiedName($"{propertyName}[{child.Index}]", NamespaceIndex);
            var path = $"{childPrefix}{propertyName}[{child.Index}]";

            CreateChildObject(childBrowseName, child.Proxy, path, propertyNode.NodeId, childReferenceTypeId);
        }
    }

    private void CreateDictionaryObjectNode(string propertyName, RegisteredProxyProperty property, ICollection<ProxyPropertyChild> children, NodeId parentNodeId, string parentPath)
    {
        var nodeId = new NodeId(parentPath + propertyName, NamespaceIndex);
        var browseName = GetBrowseName(propertyName, property, null);

        var typeDefinitionId = GetTypeDefinitionId(property);
        var referenceTypeId = GetReferenceTypeId(property);

        var propertyNode = CreateFolder(parentNodeId, nodeId, browseName, typeDefinitionId, referenceTypeId);
        var childReferenceTypeId = GetChildReferenceTypeId(property);
        foreach (var child in children)
        {
            var childBrowseName = new QualifiedName(child.Index?.ToString(), NamespaceIndex);
            var childPath = parentPath + propertyName + PathDelimiter + child.Index;

            CreateChildObject(childBrowseName, child.Proxy, childPath, propertyNode.NodeId, childReferenceTypeId);
        }
    }

    private void CreateVariableNode(string propertyName, KeyValuePair<string, RegisteredProxyProperty> property, NodeId parentNodeId, string parentPath)
    {
        var sourcePath = _source.TryGetSourcePath(property.Value.Property);
        if (sourcePath is not null)
        {
            var value = property.Value.GetValue();
            var type = property.Value.Type;

            if (type == typeof(decimal))
            {
                type = typeof(double);
                value = Convert.ToDouble(value);
            }

            var nodeId = new NodeId(parentPath + propertyName, NamespaceIndex);
            var browseName = GetBrowseName(propertyName, property.Value, null);
            var dataType = Opc.Ua.TypeInfo.Construct(type);
            var referenceTypeId = GetReferenceTypeId(property.Value);

            // TODO: Add support for arrays (valueRank >= 0)
            var variable = CreateVariableNode(parentNodeId, nodeId, browseName, dataType, -1, referenceTypeId);
            variable.Value = value;
            variable.StateChanged += (context, node, changes) =>
            {
                if (changes.HasFlag(NodeStateChangeMasks.Value))
                {
                    _source.UpdateProperty(property.Value.Property, sourcePath, variable.Value);
                }
            };

            property.Value.Property.SetPropertyData(OpcUaServerTrackableSource<TProxy>.OpcVariableKey, variable);
        }
    }

    private void CreateChildObject(
        QualifiedName browseName,
        IProxy proxy, 
        string path, 
        NodeId parentNodeId, 
        NodeId? referenceTypeId)
    {
        var registeredProxy = _registry.KnownProxies[proxy];
        if (_proxies.TryGetValue(registeredProxy, out var objectNode))
        {
            var parentNode = FindNodeInAddressSpace(parentNodeId);
            parentNode.AddReference(referenceTypeId ?? ReferenceTypeIds.HasComponent, false, objectNode.NodeId);
        }
        else
        {
            var nodeId = new NodeId(path, NamespaceIndex);
            var typeDefinitionId = GetTypeDefinitionId(proxy);

            var node = CreateFolder(parentNodeId, nodeId, browseName, typeDefinitionId, referenceTypeId);
            CreateObjectNode(node.NodeId, registeredProxy, path + PathDelimiter);

            _proxies[registeredProxy] = node;
        }
    }

    private static NodeId? GetReferenceTypeId(RegisteredProxyProperty property)
    {
        var referenceTypeAttribute = property.Attributes
            .OfType<OpcUaNodeReferenceTypeAttribute>()
            .FirstOrDefault();

        return referenceTypeAttribute is not null ?
            typeof(ReferenceTypeIds).GetField(referenceTypeAttribute.Type)?.GetValue(null) as NodeId : null;
    }

    private static NodeId? GetChildReferenceTypeId(RegisteredProxyProperty property)
    {
        var referenceTypeAttribute = property.Attributes
            .OfType<OpcUaNodeItemReferenceTypeAttribute>()
            .FirstOrDefault();

        return referenceTypeAttribute is not null ?
            typeof(ReferenceTypeIds).GetField(referenceTypeAttribute.Type)?.GetValue(null) as NodeId : null;
    }

    private NodeId? GetTypeDefinitionId(RegisteredProxyProperty property)
    {
        var typeDefinitionAttribute = property.Attributes
            .OfType<OpcUaTypeDefinitionAttribute>()
            .FirstOrDefault();

        return GetTypeDefinitionId(typeDefinitionAttribute);
    }

    private NodeId? GetTypeDefinitionId(IProxy proxy)
    {
        var typeDefinitionAttribute = proxy.GetType().GetCustomAttribute<OpcUaTypeDefinitionAttribute>();
        return GetTypeDefinitionId(typeDefinitionAttribute);
    }

    private NodeId? GetTypeDefinitionId(OpcUaTypeDefinitionAttribute? typeDefinitionAttribute)
    {
        if (typeDefinitionAttribute is null)
        {
            return null;
        }

        if (typeDefinitionAttribute.Namespace is not null)
        {
            var typeDefinition = NodeId.Create(
                typeDefinitionAttribute.Type,
                typeDefinitionAttribute.Namespace,
                SystemContext.NamespaceUris);

            return PredefinedNodes.Values.SingleOrDefault(n =>
                    n.BrowseName.Name == typeDefinition.Identifier.ToString() &&
                    n.BrowseName.NamespaceIndex == typeDefinition.NamespaceIndex)?
                .NodeId;
        }

        return typeof(ObjectTypeIds).GetField(typeDefinitionAttribute.Type)?.GetValue(null) as NodeId;
    }

    private QualifiedName GetBrowseName(string propertyName, RegisteredProxyProperty property, object? index)
    {
        var browseNameProvider = property.Attributes.OfType<IOpcUaBrowseNameProvider>().SingleOrDefault();
        if (browseNameProvider is null)
        {
            return new QualifiedName(propertyName + (index is not null ? $"[{index}]" : string.Empty), NamespaceIndex);
        }

        if (browseNameProvider.BrowseNamespace is not null)
        {
            return new QualifiedName(browseNameProvider.BrowseName, (ushort)SystemContext.NamespaceUris.GetIndex(browseNameProvider.BrowseNamespace));
        }

        return new QualifiedName(browseNameProvider.BrowseName, NamespaceIndex);
    }

    private FolderState CreateFolder(
        NodeId parentNodeId,
        NodeId nodeId,
        QualifiedName browseName, 
        NodeId? typeDefinition, 
        NodeId? referenceType)
    {
        var parentNode = FindNodeInAddressSpace(parentNodeId);

        var folder = new FolderState(parentNode)
        {
            NodeId = nodeId,
            BrowseName = browseName,
            DisplayName = new Opc.Ua.LocalizedText(browseName.Name),
            TypeDefinitionId = typeDefinition ?? ObjectTypeIds.FolderType,
            WriteMask = AttributeWriteMask.None,
            UserWriteMask = AttributeWriteMask.None,
            ReferenceTypeId = referenceType ?? ReferenceTypeIds.HasComponent
        };

        if (parentNode != null)
        {
            parentNode.AddChild(folder);
        }

        AddPredefinedNode(SystemContext, folder);
        return folder;
    }

    private BaseDataVariableState CreateVariableNode(
        NodeId parentNodeId, NodeId nodeId, QualifiedName browseName, 
        Opc.Ua.TypeInfo dataType, int valueRank, NodeId? referenceType)
    {
        var parentNode = FindNodeInAddressSpace(parentNodeId);

        var variable = new BaseDataVariableState(parentNode)
        {
            NodeId = nodeId,

            SymbolicName = browseName.Name,
            BrowseName = browseName,
            DisplayName = new Opc.Ua.LocalizedText(browseName.Name),

            TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
            DataType = Opc.Ua.TypeInfo.GetDataTypeId(dataType),
            ValueRank = valueRank,
            AccessLevel = AccessLevels.CurrentReadOrWrite,
            UserAccessLevel = AccessLevels.CurrentReadOrWrite,
            StatusCode = StatusCodes.Good,
            Timestamp = DateTime.UtcNow,

            ReferenceTypeId = referenceType ?? ReferenceTypeIds.HasProperty
        };

        if (parentNode != null)
        {
            parentNode.AddChild(variable);
        }

        AddPredefinedNode(SystemContext, variable);
        return variable;
    }
}
