using System.Threading;
using System.Threading.Tasks;

namespace Namotion.NuGetPlugins
{
    public static class DynamicNuGetPackageLoaderExtensions
    {
        public static Task LoadPackageFromRepositoryAsync(this IDynamicNuGetPackageLoader loader, NuGetPackage package, CancellationToken cancellationToken)
        {
            return loader.LoadPackageFromRepositoryAsync(package.PackageName, package.PackageVersion, cancellationToken); 
        }
    }
}