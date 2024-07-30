using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Opc.UaFx;
using Opc.UaFx.Server;

using System.Reactive.Linq;
using System.Collections.ObjectModel;

using Namotion.Proxy.Sources.Abstractions;
using Namotion.Proxy.Registry.Abstractions;

namespace Namotion.Proxy.OpcUa.Traeger.Server
{
    internal class OpcUaServerTrackableSource<TProxy> : BackgroundService, IProxySource, IDisposable
        where TProxy : IProxy
    {
        internal const string OpcVariableKey = "OpcVariable";

        private readonly IProxyContext _context;
        private readonly ILogger _logger;

        private readonly OpcProviderBasedNodeManager<TProxy> _nodeManager;
        private readonly OpcNodeSet[] _nodeSets = Array.Empty<OpcNodeSet>();

        private OpcServer? _opcServer;

        internal ISourcePathProvider SourcePathProvider { get; }

        public OpcUaServerTrackableSource(
            TProxy proxy,
            ISourcePathProvider sourcePathProvider,
            ILogger<OpcUaServerTrackableSource<TProxy>> logger)
        {
            _context = proxy.Context ??
                throw new InvalidOperationException($"Context is not set on {nameof(TProxy)}.");

            _nodeManager = new OpcProviderBasedNodeManager<TProxy>(proxy, this);
            SourcePathProvider = sourcePathProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _opcServer = _nodeManager.CreateServer();
                    _opcServer.Start();

                    OpcUaNamespace.GetNamespace(_opcServer.SystemContext).Resolve(_opcServer);

                    foreach (var ns in _nodeSets.SelectMany(ns => ns.Namespaces))
                    {
                        ns.Resolve(_opcServer);
                    }

                    await Task.Delay(-1, stoppingToken);
                }
                catch (Exception ex)
                {
                    if (ex is not TaskCanceledException)
                    {
                        _logger.LogError(ex, "Failed to start OPC UA server.");
                        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    }
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public Task<IDisposable?> InitializeAsync(IEnumerable<ProxyPropertyPathReference> properties, Action<ProxyPropertyPathReference> propertyUpdateAction, CancellationToken cancellationToken)
        {
            return Task.FromResult<IDisposable?>(null);
        }

        public Task<IEnumerable<ProxyPropertyPathReference>> ReadAsync(IEnumerable<ProxyPropertyPathReference> properties, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<ProxyPropertyPathReference>>(properties
                .Where(p => p.Property.TryGetPropertyData(OpcUaServerTrackableSource<TProxy>.OpcVariableKey, out var _))
                .Select(property => (property, node: property.Property.GetPropertyData(OpcUaServerTrackableSource<TProxy>.OpcVariableKey) as OpcDataVariableNode))
                .Where(p => p.node is not null)
                .Select(p => new ProxyPropertyPathReference(p.property.Property, p.property.Path,
                    p.property.Property.Metadata.Type == typeof(decimal) ? Convert.ToDecimal(p.node!.Value) : p.node!.Value))
                .ToList());
        }

        public Task WriteAsync(IEnumerable<ProxyPropertyPathReference> propertyChanges, CancellationToken cancellationToken)
        {
            foreach (var property in propertyChanges
                .Where(p => p.Property.TryGetPropertyData(OpcUaServerTrackableSource<TProxy>.OpcVariableKey, out var _)))
            {
                var actualValue = property.Value;
                if (actualValue is decimal)
                {
                    actualValue = Convert.ToDouble(actualValue);
                }

                var node = property.Property.GetPropertyData(OpcUaServerTrackableSource<TProxy>.OpcVariableKey) as OpcDataVariableNode;
                node!.Value = actualValue;
                node.UpdateChanges(_opcServer!.SystemContext, OpcNodeChanges.Value);
            }

            return Task.CompletedTask;
        }

        public string? TryGetSourcePath(ProxyPropertyReference property)
        {
            return SourcePathProvider.TryGetSourcePath(property);
        }
    }

    internal class OpcProviderBasedNodeManager<TProxy> : OpcNodeManager
        where TProxy : IProxy
    {
        private readonly TProxy _proxy;
        private readonly IProxyRegistry _registry;
        private readonly IEnumerable<IOpcUaNodeProvider> _nodeProviders = Enumerable.Empty<IOpcUaNodeProvider>();

        private OpcUaServerTrackableSource<TProxy> _source;

        public OpcProviderBasedNodeManager(TProxy proxy, OpcUaServerTrackableSource<TProxy> source)
            : base("https://foobar/")
        {
            _proxy = proxy;
            _registry = proxy.Context?.GetHandler<IProxyRegistry>() ?? throw new ArgumentException($"Registry could not be found.");
            _source = source;
        }

        public OpcServer CreateServer()
        {
            var companionSpecsManager = OpcNodeSetManager.Create(
                NodeSetFactory.LoadNodeSetFromEmbeddedResource<OpcProviderBasedNodeManager<TProxy>>("NodeSets.Opc.Ua.Di.NodeSet2.xml"),
                _nodeProviders
                    .SelectMany(p => p.CreateNodeSets())
                    .DistinctBy(n => n.Name)
                    .ToArray()
            );

            return new OpcServer(/*"opc.tcp://localhost:4840/",*/ companionSpecsManager, this);
        }

        protected override IEnumerable<OpcNodeSet> ImportNodes()
        {
            return _nodeProviders
                .SelectMany(p => p.CreateNodeSets())
                .DistinctBy(n => n.Name)
                .ToArray();
        }

        protected override IEnumerable<IOpcNode> CreateNodes(OpcNodeReferenceCollection references)
        {
            // OPC UA Modelling overview: https://reference.opcfoundation.org/Machinery/v102/docs/4

            var context = new OpcUaNodeProviderContext
            {
                Context = SystemContext,
                Manager = this,
                References = references
            };

            var metadata = _registry.KnownProxies[_proxy];
            if (metadata is not null)
            {
                var node = new OpcFolderNode(new OpcName("Root", DefaultNamespaceIndex));
                CreateObjectNode(context, metadata, node, string.Empty);
                context.Nodes.Add(node);
                context.References.Add(node, OpcObjectTypes.ObjectsFolder);
            }

            foreach (var provider in _nodeProviders)
            {
                provider.CreateNodes(context);
            }

            foreach (var node in context.Nodes)
            {
                yield return node;
            }
        }

        private void CreateObjectNode(OpcUaNodeProviderContext context, RegisteredProxy metadata, OpcInstanceNode parentNode, string prefix)
        {
            foreach (var property in metadata.Properties)
            {
                var propertyName = _source.SourcePathProvider.TryGetSourcePropertyName(property.Value.Property);
                if (property.Value.Children.Any())
                {
                    var propertyNode = new OpcFolderNode(new OpcName(propertyName, DefaultNamespaceIndex));
                    foreach (var child in property.Value.Children)
                    {
                        var index = child.Index is not null ? $"[{child.Index}]" : string.Empty;
                        var path = prefix + propertyName + index;

                        var objectNode = new OpcObjectNode(path);
                        CreateObjectNode(context, _registry.KnownProxies[child.Proxy], objectNode, path + ".");

                        propertyNode.AddChild(context.Context, objectNode);
                    }
                    parentNode.AddChild(context.Context, propertyNode);
                }
                else
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

                        var opcName = new OpcName(propertyName, DefaultNamespaceIndex);
                        var opcType = typeof(OpcDataVariableNode<>).MakeGenericType(type);
                        var variable = (OpcDataVariableNode)Activator.CreateInstance(opcType, opcName)!;

                        variable.Value = value;
                        property.Value.Property.SetPropertyData(OpcUaServerTrackableSource<TProxy>.OpcVariableKey, variable);

                        parentNode.AddChild(context.Context, variable);
                    }
                }
            }
        }
    }

    public static class OpcUaMachinery
    {
        private const int MachineIdentificationType_NodeValue = 1012;
        private const int MachineryItemState_StateMachineType_NodeValue = 1002;

        public static OpcNamespace GetNamespace(OpcContext context)
        {
            return context.Namespaces.Single(ns => ns.Uri.OriginalString == "http://opcfoundation.org/UA/Machinery/");
        }

        public static OpcNodeId GetMachineIdentificationType(OpcContext context)
        {
            return new OpcNodeId(MachineIdentificationType_NodeValue, GetNamespace(context));
        }

        public static OpcNodeId GetMachineryItemStateStateMachineType(OpcContext context)
        {
            // MachineryItemState_StateMachineType
            return new OpcNodeId(MachineryItemState_StateMachineType_NodeValue, GetNamespace(context));
        }
    }


    public interface IOpcUaNodeProvider
    {
        IEnumerable<OpcNodeSet> CreateNodeSets();

        void CreateNodes(OpcUaNodeProviderContext context);
    }

    public record OpcUaNodeProviderContext
    {
        public required OpcContext Context { get; init; }

        public required OpcNodeManager Manager { get; init; }

        public required OpcNodeReferenceCollection References { get; init; }

        public Collection<IOpcNode> Nodes { get; } = new Collection<IOpcNode>();
    }

    public static class NodeSetFactory
    {
        public static OpcNodeSet LoadNodeSetFromEmbeddedResource<TAssemblyType>(string name)
        {
            var assembly = typeof(TAssemblyType).Assembly;
            using var stream = assembly.GetManifestResourceStream($"{assembly.FullName!.Split(',')[0]}.{name}");
            return OpcNodeSet.Load(stream ?? throw new ArgumentException("Embedded resource could not be found.", nameof(name)));
        }
    }

    public static class OpcUaNamespace
    {
        private const int StateMachineType_NodeValue = 2755;
        private const int HasAddIn_NodeValue = 17604;

        public static OpcNamespace GetNamespace(OpcContext context)
        {
            return context.Namespaces.Single(ns => ns.Uri.OriginalString == "http://opcfoundation.org/UA/");
        }

        public static OpcNodeId GetStateMachineType(OpcContext context)
        {
            return new OpcNodeId(StateMachineType_NodeValue, GetNamespace(context));
        }

        public static OpcNodeId HasAddIn { get; } = new OpcNodeId(HasAddIn_NodeValue);
    }
}