using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

using Namotion.Proxy.Sources;
using Namotion.Proxy;
using Namotion.Proxy.ChangeTracking;
using Namotion.Proxy.Sources.Abstractions;
using Namotion.Proxy.Registry.Abstractions;

using System.Reactive.Linq;

namespace Namotion.Trackable.Sources;

public class ProxySourceBackgroundService<TProxy> : BackgroundService
    where TProxy : IProxy
{
    private readonly IProxyContext _context;
    private readonly IProxySource _source;
    private readonly ILogger _logger;
    private readonly TimeSpan _bufferTime;
    private readonly TimeSpan _retryTime;

    private HashSet<string>? _initializedProperties;

    public ProxySourceBackgroundService(
        IProxySource source,
        IProxyContext context,
        ILogger logger,
        TimeSpan? bufferTime = null,
        TimeSpan? retryTime = null)
    {
        _source = source;
        _context = context;
        _logger = logger;
        _bufferTime = bufferTime ?? TimeSpan.FromMilliseconds(8);
        _retryTime = retryTime ?? TimeSpan.FromSeconds(10);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // TODO: Currently newly added properties/trackable are not automatically tracked/subscribed to

                var propertiesWithSetter = _context
                    .GetHandler<IProxyRegistry>()
                    .KnownProxies
                    .SelectMany(v => v.Value.Properties
                        .Where(p => p.Value.HasSetter)
                        .Select(p =>
                        {
                            var reference = new ProxyPropertyReference(v.Key, p.Key);
                            return new ProxyPropertyPathReference(reference,
                                _source.TryGetSourcePath(reference) ?? string.Empty,
                                p.Value.HasGetter ? p.Value.GetValue() : null);
                        }))
                    .Where(p => p.Path != string.Empty)
                    .ToList();

                lock (this)
                {
                    _initializedProperties = new HashSet<string>();
                }

                // subscribe first and mark all properties as initialized which are updated before the read has completed 
                using var disposable = await _source.InitializeAsync(propertiesWithSetter!, UpdatePropertyValueFromSource, stoppingToken);

                // read all properties (subscription during read will later be ignored)
                var initialValues = await _source.ReadAsync(propertiesWithSetter!, stoppingToken);
                lock (this)
                {
                    // ignore properties which have been updated via subscription
                    foreach (var value in initialValues
                        .Where(v => !_initializedProperties.Contains(v.Path)))
                    {
                        UpdatePropertyValueFromSource(value);
                    }

                    _initializedProperties = null;
                }

                await _context
                    .GetPropertyChangedObservable()
                    .Where(change => !change.IsChangingFromSource(_source) && _source.TryGetSourcePath(change.Property) != null)
                    .BufferChanges(_bufferTime)
                    .Where(changes => changes.Any())
                    .ForEachAsync(async changes =>
                    {
                        var values = changes
                            .Select(c => new ProxyPropertyPathReference(
                                c.Property, _source.TryGetSourcePath(c.Property)!, c.NewValue));

                        await _source.WriteAsync(values, stoppingToken);
                    }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to listen for changes.");
                await Task.Delay(_retryTime, stoppingToken);
            }
        }
    }

    protected void UpdatePropertyValueFromSource(ProxyPropertyPathReference property)
    {
        try
        {
            MarkPropertyAsInitialized(property.Path);
            property.Property.SetValueFromSource(_source, property.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set value of property {PropertyName} of type {Type}.",
                property.Property.Proxy.GetType().FullName, property.Property.Name);
        }
    }

    private void MarkPropertyAsInitialized(string sourcePath)
    {
        if (_initializedProperties is not null)
        {
            lock (this)
            {
                if (_initializedProperties != null)
                {
                    _initializedProperties.Add(sourcePath);
                }
            }
        }
    }
}
