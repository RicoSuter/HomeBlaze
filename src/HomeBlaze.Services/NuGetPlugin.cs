using Microsoft.Extensions.Logging;
using Namotion.NuGetPlugins;
using System.Reflection;
using System.Text.Json.Serialization;

namespace HomeBlaze.Services
{
    public class NuGetPlugin
    {
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("path")]
        public string? Path { get; set; }

        [JsonIgnore]
        public Assembly? Assembly { get; private set; }

        [JsonIgnore]
        public bool IsLoaded => Assembly != null;

        public async Task LoadAsync(IDynamicNuGetPackageLoader _nuGetPackageLoader, ILogger logger, CancellationToken cancellationToken)
        {
            if (IsActive)
            {
                if (!string.IsNullOrEmpty(Path))
                {
                    logger.LogTrace("Loading extension from path {Path}.", Path);

                    var package = _nuGetPackageLoader.LoadPackageFromFile(Path!);
                    Assembly = package.PackageAssemblies.First();
                }
                else if (!string.IsNullOrEmpty(Name))
                {
                    logger.LogTrace("Loading extension from NuGet.org with package name {PackageName}.", Name);

                    var package = await _nuGetPackageLoader.LoadPackageFromRepositoryAsync(Name, null, cancellationToken);
                    Assembly = package.PackageAssemblies.First();
                }
            }
        }
    }
}