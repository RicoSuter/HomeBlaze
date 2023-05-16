using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Namotion.NuGetPlugins
{
    public static class NuGetPackageRepositoryExtensions
    {
        public static Task<(NuGetPackage, Stream)> DownloadPackageAsync(this INuGetPackageRepository repository, NuGetPackage package, CancellationToken cancellationToken)
        {
            return repository.DownloadPackageAsync(package.PackageName, package.PackageVersion, cancellationToken);
        }
    }
}