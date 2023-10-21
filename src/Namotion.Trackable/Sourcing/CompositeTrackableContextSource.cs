using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Namotion.Trackable.Sourcing;

public class CompositeTrackableContextSource : ITrackableSource
{
    private readonly IReadOnlyDictionary<string, ITrackableSource> _sources;

    public string Separator { get; }

    public CompositeTrackableContextSource(IReadOnlyDictionary<string, ITrackableSource> sources, string separator = ".")
    {
        _sources = sources
            .OrderByDescending(s => s.Key)
            .ToDictionary(s => s.Key, s => s.Value);

        Separator = separator;
    }

    public async Task<IDisposable?> InitializeAsync(string sourceName, IEnumerable<string> sourcePaths, Action<string, object?> propertyUpdateAction, CancellationToken cancellationToken)
    {
        var disposables = new List<IDisposable>();

        var groups = sourcePaths.GroupBy(p => _sources.First(s => p.StartsWith(s.Key + Separator)));
        foreach (var group in groups)
        {
            (var path, var source) = group.Key;
            if (path is not null && source is not null)
            {
                var innerSourcePaths = group.Select(p => p.Substring(path.Length + Separator.Length));
                var disposable = await source.InitializeAsync(
                    sourceName,
                    innerSourcePaths,
                    (innerPath, value) => propertyUpdateAction(path + Separator + innerPath, value),
                    cancellationToken);

                if (disposable != null)
                {
                    disposables.Add(disposable);
                }
            }
        }

        return new CompositeDisposable(disposables);
    }

    public async Task<IReadOnlyDictionary<string, object?>> ReadAsync(IEnumerable<string> sourcePaths, CancellationToken cancellationToken)
    {
        var result = new List<KeyValuePair<string, object?>>();

        var groups = sourcePaths.GroupBy(p => _sources.First(s => p.StartsWith(s.Key + Separator)));
        foreach (var group in groups)
        {
            (var path, var source) = group.Key;
            if (path is not null && source is not null)
            {
                var innerSourcePaths = group.Select(p => p.Substring(path.Length + Separator.Length)); ;
                var sourceResult = await source.ReadAsync(innerSourcePaths, cancellationToken);
                result.AddRange(sourceResult.Select(p => new KeyValuePair<string, object?>(path + Separator + p.Key, p.Value)));
            }
        }

        return result.ToDictionary(p => p.Key, p => p.Value);
    }

    public async Task WriteAsync(IReadOnlyDictionary<string, object?> propertyChanges, CancellationToken cancellationToken)
    {
        var result = new List<KeyValuePair<string, object?>>();

        var groups = propertyChanges.GroupBy(p => _sources.First(s => p.Key.StartsWith(s.Key + Separator)));
        foreach (var group in groups)
        {
            (var path, var source) = group.Key;
            if (path is not null && source is not null)
            {
                var innerSourcePaths = group.ToDictionary(p => p.Key.Substring(path.Length + Separator.Length), p => p.Value);
                await source.WriteAsync(innerSourcePaths, cancellationToken);
            }
        }
    }

    private class CompositeDisposable : IDisposable
    {
        private readonly IEnumerable<IDisposable> _disposables;

        public CompositeDisposable(IEnumerable<IDisposable> disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
