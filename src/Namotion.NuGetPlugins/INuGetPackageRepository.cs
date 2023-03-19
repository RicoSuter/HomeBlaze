using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Namotion.NuGetPlugins
{
    public interface INuGetPackageRepository
    {
        Task<IEnumerable<NuGetPackage>> SearchPackagesAsync(string searchTerm, int skip, int take, CancellationToken cancellationToken);

        Task<(NuGetPackage, Stream)> DownloadPackageAsync(string packageName, string? packageVersion, CancellationToken cancellationToken);
    }
}