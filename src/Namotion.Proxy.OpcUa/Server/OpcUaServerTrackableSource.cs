using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Reactive.Linq;

using Namotion.Proxy.Sources.Abstractions;
using Opc.Ua;
using Opc.Ua.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace Namotion.Proxy.OpcUa.Server;

internal class OpcUaServerTrackableSource<TProxy> : BackgroundService, IProxySource, IDisposable
    where TProxy : IProxy
{
    internal const string OpcVariableKey = "OpcVariable";

    private readonly IProxyContext _context;
    private readonly TProxy _proxy;
    private readonly ILogger _logger;
    private readonly string? _rootName;

    private ProxyOpcUaServer<TProxy>? _server;
    private Action<ProxyPropertyPathReference>? _propertyUpdateAction;

    internal ISourcePathProvider SourcePathProvider { get; }

    public OpcUaServerTrackableSource(
        TProxy proxy,
        ISourcePathProvider sourcePathProvider,
        ILogger<OpcUaServerTrackableSource<TProxy>> logger,
        string? rootName)
    {
        _context = proxy.Context ??
            throw new InvalidOperationException($"Context is not set on {nameof(TProxy)}.");

        _proxy = proxy;
        _logger = logger;
        _rootName = rootName;

        SourcePathProvider = sourcePathProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var stream = typeof(OpcUaProxyExtensions).Assembly
                .GetManifestResourceStream("Namotion.Proxy.OpcUa.MyOpcUaServer.Config.xml");

            var application = new ApplicationInstance
            {
                ApplicationName = "MyOpcUaServer",
                ApplicationType = ApplicationType.Server,
                ApplicationConfiguration = await ApplicationConfiguration.Load(
                    stream, ApplicationType.Server, typeof(ApplicationConfiguration), false)
            };

            try
            {
                _server = new ProxyOpcUaServer<TProxy>(_proxy, this, _rootName);

                await application.CheckApplicationInstanceCertificate(true, CertificateFactory.DefaultKeySize);
                await application.Start(_server);

                await Task.Delay(-1, stoppingToken);
            }
            catch (Exception ex)
            {
                application.Stop();

                if (ex is not TaskCanceledException)
                {
                    _logger.LogError(ex, "Failed to start OPC UA server.");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
        }
    }

    internal void UpdateProperty(ProxyPropertyReference property, string sourcePath, object? value)
    {
        var convertedValue = Convert.ChangeType(value, property.Metadata.Type); // TODO: improve conversion here
        _propertyUpdateAction?.Invoke(new ProxyPropertyPathReference(property, sourcePath, convertedValue));
    }

    public Task<IDisposable?> InitializeAsync(IEnumerable<ProxyPropertyPathReference> properties, Action<ProxyPropertyPathReference> propertyUpdateAction, CancellationToken cancellationToken)
    {
        _propertyUpdateAction = propertyUpdateAction;
        return Task.FromResult<IDisposable?>(null);
    }

    public Task<IEnumerable<ProxyPropertyPathReference>> ReadAsync(IEnumerable<ProxyPropertyPathReference> properties, CancellationToken cancellationToken)
    {
        return Task.FromResult<IEnumerable<ProxyPropertyPathReference>>(properties
            .Where(p => p.Property.TryGetPropertyData(OpcUaServerTrackableSource<TProxy>.OpcVariableKey, out var _))
            .Select(property => (property, node: property.Property.GetPropertyData(OpcUaServerTrackableSource<TProxy>.OpcVariableKey) as BaseDataVariableState))
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
            var node = property.Property.GetPropertyData(OpcUaServerTrackableSource<TProxy>.OpcVariableKey) as BaseDataVariableState;
            if (node is not null)
            {
                var actualValue = property.Value;
                if (actualValue is decimal)
                {
                    actualValue = Convert.ToDouble(actualValue);
                }

                node.Value = actualValue;
                node.ClearChangeMasks(_server?.CurrentInstance.DefaultSystemContext, false);
            }
        }

        return Task.CompletedTask;
    }

    public string? TryGetSourcePath(ProxyPropertyReference property)
    {
        return SourcePathProvider.TryGetSourcePath(property);
    }
}
