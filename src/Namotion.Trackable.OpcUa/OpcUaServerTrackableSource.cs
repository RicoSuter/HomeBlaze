using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Namotion.Trackable;
using System.Reactive.Linq;
using Namotion.Trackable.Model;
using System.Collections.Concurrent;
using Namotion.Trackable.Sources;
using Opc.UaFx;
using Opc.UaFx.Server;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

namespace Namotion.Trackable.OpcUa
{
    public class OpcUaServerTrackableSource<TTrackable> : BackgroundService, ITrackableSource, IDisposable
        where TTrackable : class
    {
        internal const string OpcVariableKey = "OpcVariable";

        private readonly TrackableContext<TTrackable> _trackableContext;
        internal readonly ISourcePathProvider _sourcePathProvider;
        private readonly ILogger _logger;

        private Action<string, object?>? _propertyUpdateAction;
        private ConcurrentDictionary<string, object?> _state = new();

        private readonly OpcProviderBasedNodeManager<TTrackable> _nodeManager;
        private readonly OpcNodeSet[] _nodeSets = Array.Empty<OpcNodeSet>();

        private OpcServer? _opcServer;

        internal Dictionary<string, TrackedProperty> _variables = new Dictionary<string, TrackedProperty>();

        public OpcUaServerTrackableSource(
            TrackableContext<TTrackable> trackableContext,
            ISourcePathProvider sourcePathProvider,
            ILogger<OpcUaServerTrackableSource<TTrackable>> logger)
        {
            _trackableContext = trackableContext;
            _sourcePathProvider = sourcePathProvider;
            _nodeManager = new OpcProviderBasedNodeManager<TTrackable>(_trackableContext, this);
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

        public Task<IDisposable?> InitializeAsync(IEnumerable<string> sourcePaths, Action<string, object?> propertyUpdateAction, CancellationToken cancellationToken)
        {
            _propertyUpdateAction = propertyUpdateAction;
            return Task.FromResult<IDisposable?>(null);
        }

        public Task<IReadOnlyDictionary<string, object?>> ReadAsync(IEnumerable<string> sourcePaths, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyDictionary<string, object?>>(_state
                .Where(s => sourcePaths.Contains(s.Key.Replace(".", "/")))
                .ToDictionary(s => s.Key, s => s.Value));
        }

        public async Task WriteAsync(IReadOnlyDictionary<string, object?> propertyChanges, CancellationToken cancellationToken)
        {
            foreach ((var sourcePath, var value) in propertyChanges)
            {
                var actualValue = value;
                if (actualValue is decimal)
                {
                    actualValue = Convert.ToDouble(actualValue);
                }

                var property = _variables[sourcePath];
                var node = property.Data[OpcUaServerTrackableSource<TTrackable>.OpcVariableKey] as OpcDataVariableNode;
                node!.Value = actualValue;
                node.UpdateChanges(_opcServer!.SystemContext, OpcNodeChanges.Value);
            }
        }

        public string? TryGetSourcePath(TrackedProperty property)
        {
            return _sourcePathProvider.TryGetSourcePath(property);
        }
    }

    public class OpcProviderBasedNodeManager<TTrackable> : OpcNodeManager
        where TTrackable : class
    {
        private readonly TrackableContext<TTrackable> _trackableContext;

        private readonly IEnumerable<IOpcUaNodeProvider> _nodeProviders = Enumerable.Empty<IOpcUaNodeProvider>();

        private TrackableContext<TTrackable> trackableContext;
        private OpcUaServerTrackableSource<TTrackable> _source;

        public OpcProviderBasedNodeManager(TrackableContext<TTrackable> trackableContext, OpcUaServerTrackableSource<TTrackable> source)
            : base("https://foobar/")
        {
            _trackableContext = trackableContext;
            _source = source;
        }

        public OpcServer CreateServer()
        {
            var companionSpecsManager = OpcNodeSetManager.Create(
                NodeSetFactory.LoadNodeSetFromEmbeddedResource<OpcProviderBasedNodeManager<TTrackable>>("NodeSets.Opc.Ua.Di.NodeSet2.xml"),
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

            var root = _trackableContext.TryGetTracker(_trackableContext.Object);
            if (root is not null)
            {
                var node = new OpcFolderNode(new OpcName("Root", DefaultNamespaceIndex));
                CreateObjectNode(context, root, node);
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

        private void CreateObjectNode(OpcUaNodeProviderContext context, ProxyTracker parent, OpcInstanceNode parentNode)
        {
            foreach (var property in parent.Properties)
            {
                var propertyName = _source._sourcePathProvider.TryGetSourceProperty(property.Value);
                if (property.Value.Children.Any())
                {
                    var propertyNode = new OpcFolderNode(new OpcName(propertyName, DefaultNamespaceIndex));
                    foreach (var child in property.Value.Children)
                    {
                        var objectNode = new OpcObjectNode(child.Path);
                        CreateObjectNode(context, child, objectNode);
                       
                        propertyNode.AddChild(context.Context, objectNode);
                    }
                    parentNode.AddChild(context.Context, propertyNode);
                }
                else
                {
                    var sourcePath = _source.TryGetSourcePath(property.Value);
                    if (sourcePath is not null)
                    {
                        var value = property.Value.GetValue();
                        var type = property.Value.PropertyType;

                        if (type == typeof(decimal))
                        {
                            type = typeof(double);
                            value = Convert.ToDouble(value);
                        }

                        var opcName = new OpcName(propertyName, DefaultNamespaceIndex);
                        var opcType = typeof(OpcDataVariableNode<>).MakeGenericType(type);
                        var variable = (OpcDataVariableNode)Activator.CreateInstance(opcType, opcName)!;

                        variable.Value = value;

                        property.Value.Data = property.Value.Data.SetItem(OpcUaServerTrackableSource<TTrackable>.OpcVariableKey, variable);

                        _source._variables[sourcePath] = property.Value;

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