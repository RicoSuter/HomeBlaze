//namespace Namotion.Proxy.Sources;

//public class CompositeTrackableContextSource : ITrackableSource
//{
//    private readonly IReadOnlyDictionary<string, ITrackableSource> _sources;

//    public string Separator { get; }

//    public ISourcePathProvider SourcePathProvider => throw new NotImplementedException();

//    public CompositeTrackableContextSource(IReadOnlyDictionary<string, ITrackableSource> sources, string separator = ".")
//    {
//        _sources = sources
//            .OrderByDescending(s => s.Key)
//            .ToDictionary(s => s.Key, s => s.Value);

//        Separator = separator;
//    }

//    public async Task<IDisposable?> InitializeAsync(IEnumerable<PropertyInfo> properties, Action<PropertyInfo> propertyUpdateAction, CancellationToken cancellationToken)
//    {
//        var disposables = new List<IDisposable>();

//        var groups = properties.GroupBy(p => _sources.First(s => p.Path.StartsWith(s.Key + Separator)));
//        foreach (var group in groups)
//        {
//            (var path, var source) = group.Key;
//            if (path is not null && source is not null)
//            {
//                var innerSourcePaths = group.Select(p => p.Path.Substring(path.Length + Separator.Length));

//                var disposable = await source.InitializeAsync(
//                    group,
//                    (property) => propertyUpdateAction(
//                        new PropertyInfo(property.Property, property.Path.Substring(path.Length + 1), property.Value)),
//                    cancellationToken);

//                if (disposable != null)
//                {
//                    disposables.Add(disposable);
//                }
//            }
//        }

//        return new CompositeDisposable(disposables);
//    }

//    public async Task<IEnumerable<PropertyInfo>> ReadAsync(IEnumerable<PropertyInfo> properties, CancellationToken cancellationToken)
//    {
//        var result = new List<PropertyInfo>();

//        var groups = properties.GroupBy(p => _sources.First(s => p.Path.StartsWith(s.Key + Separator)));
//        foreach (var group in groups)
//        {
//            (var path, var source) = group.Key;
//            if (path is not null && source is not null)
//            {
//                var sourceResult = await source.ReadAsync(group, cancellationToken);
//                result.AddRange(sourceResult.Select(p => new PropertyInfo(p.Property, path + Separator + p.Path, p.Value)));
//            }
//        }

//        return result;
//    }

//    public async Task WriteAsync(IEnumerable<PropertyInfo> propertyChanges, CancellationToken cancellationToken)
//    {
//        var result = new List<KeyValuePair<TrackedProperty, object?>>();

//        var groups = propertyChanges.GroupBy(p => _sources.First(s => p.Path.StartsWith(s.Key + Separator)));
//        foreach (var group in groups)
//        {
//            (var path, var source) = group.Key;
//            if (path is not null && source is not null)
//            {
//                var properties = group
//                    .Select(p => new PropertyInfo(p.Property, p.Path.Substring(path.Length + 1), p.Value))
//                    .ToList();

//                await source.WriteAsync(properties, cancellationToken);
//            }
//        }
//    }

//    public string? TryGetSourcePath(ProxyPropertyReference property)
//    {
//        var source = _sources.First(s => property.Path.StartsWith(s.Key + Separator));
//        return source.Value.TryGetSourcePath(property);
//    }

//    private class CompositeDisposable : IDisposable
//    {
//        private readonly IEnumerable<IDisposable> _disposables;

//        public CompositeDisposable(IEnumerable<IDisposable> disposables)
//        {
//            _disposables = disposables;
//        }

//        public void Dispose()
//        {
//            foreach (var disposable in _disposables)
//            {
//                disposable.Dispose();
//            }
//        }
//    }
//}
