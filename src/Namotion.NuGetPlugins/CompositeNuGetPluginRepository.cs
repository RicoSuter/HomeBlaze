using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Namotion.NuGetPlugins
{
    public class CompositeNuGetPluginRepository : INuGetPackageRepository
    {
        private readonly IEnumerable<INuGetPackageRepository> _repositories;

        public CompositeNuGetPluginRepository(IEnumerable<INuGetPackageRepository> repositories)
        {
            _repositories = repositories;
        }

        public async Task<(NuGetPackage, Stream)> DownloadPackageAsync(string packageName, string? packageVersion, CancellationToken cancellationToken)
        {
            foreach (var repository in _repositories)
            {
                try
                {
                    return await repository.DownloadPackageAsync(packageName, packageVersion, cancellationToken);
                }
                catch (PackageNotFoundException)
                {

                }
            }

            throw new PackageNotFoundException(packageName, packageVersion);
        }

        public Task<IEnumerable<NuGetPackage>> SearchPackagesAsync(string searchTerm, int skip, int take, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
