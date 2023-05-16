using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Namotion.NuGetPlugins
{
    public class NuGetPackageRepository : INuGetPackageRepository
    {
        private readonly PackageSource _packageSource;
        private readonly Func<NuGetPackage, bool> _filter;

        public NuGetPackageRepository(PackageSource packageSource)
            : this(packageSource, plugin => true)
        {
        }

        public NuGetPackageRepository(PackageSource packageSource, Func<NuGetPackage, bool> filter)
        {
            _packageSource = packageSource ?? throw new ArgumentNullException(nameof(packageSource));
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        public static NuGetPackageRepository CreateForNuGetOrg()
        {
            return CreateForNuGetOrg(plugin => true);
        }

        public static NuGetPackageRepository CreateForNuGetOrg(Func<NuGetPackage, bool> filter)
        {
            return new NuGetPackageRepository(new PackageSource("https://api.nuget.org/v3/index.json"), filter);
        }

        public async Task<IEnumerable<NuGetPackage>> SearchPackagesAsync(string searchTerm, int skip, int take, CancellationToken cancellationToken)
        {
            var providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());

            var sourceRepository = new SourceRepository(_packageSource, providers);
            var resource = await sourceRepository.GetResourceAsync<PackageSearchResource>(cancellationToken);

            var count = 0;
            var packages = new List<NuGetPackage>();
            var tasks = new List<Task>();
            var pageSize = take > 1000 ? 1000 : take;

            while (true)
            {
                var packageSearch = await resource.SearchAsync(searchTerm, new SearchFilter(false) { IncludeDelisted = false },
                    skip + count, pageSize, new NullLogger(), cancellationToken);

                foreach (var metadata in packageSearch)
                {
                    count++;
                    var plugin = new NuGetPackage(metadata);
                    if (_filter(plugin))
                    {
                        packages.Add(plugin);
                    }
                }

                if (packageSearch.Count() < pageSize || packages.Count > take)
                {
                    break;
                }
            }

            await Task.WhenAll(tasks);
            return packages.Take(take);
        }

        public async Task<(NuGetPackage, Stream)> DownloadPackageAsync(string packageName, string? packageVersion, CancellationToken cancellationToken)
        {
            var policy = Policy
                .Handle<HttpRequestException>()
                .Or<FatalProtocolException>()
                .WaitAndRetryAsync(5, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

            return await policy.ExecuteAsync(async (ct) =>
            {
                var providers = new List<Lazy<INuGetResourceProvider>>();
                providers.AddRange(Repository.Provider.GetCoreV3());

                var sourceRepository = new SourceRepository(_packageSource, providers);

                var packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>(cancellationToken);
                var package = await packageMetadataResource.GetMetadataAsync(
                    new PackageIdentity(packageName, !string.IsNullOrEmpty(packageVersion) ? new NuGetVersion(packageVersion) : null),
                        new SourceCacheContext(), new NullLogger(), cancellationToken);

                if (package == null)
                {
                    throw new PackageNotFoundException(packageName, packageVersion);
                }

                var downloadResource = sourceRepository.GetResource<DownloadResource>(cancellationToken);
                var download = await downloadResource.GetDownloadResourceResultAsync(package.Identity,
                    new PackageDownloadContext(new SourceCacheContext
                    {
                        DirectDownload = true
                    }), Path.GetTempPath(),
                    new NullLogger(), cancellationToken);

                if (download.PackageStream == null)
                {
                    throw new HttpRequestException("Package stream is empty. Retry.");
                }

                return (new NuGetPackage(package), download.PackageStream);
            }, cancellationToken);
        }
    }
}
