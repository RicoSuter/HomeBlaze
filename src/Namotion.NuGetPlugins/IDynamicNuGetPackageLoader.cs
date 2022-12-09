using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Namotion.NuGetPlugins
{
    public interface IDynamicNuGetPackageLoader
    {
        IEnumerable<IDynamicNuGetPackage> LoadedPackages { get; }

        IDynamicNuGetPackage LoadPackageFromFile(string filePath);

        IDynamicNuGetPackage LoadPackageFromStream(string packageName, string packageVersion, Stream stream);

        Task<IDynamicNuGetPackage> LoadPackageFromRepositoryAsync(string packageName, string? packageVersion, CancellationToken cancellationToken);

        bool UnloadPackage(string packageName);
    }
}