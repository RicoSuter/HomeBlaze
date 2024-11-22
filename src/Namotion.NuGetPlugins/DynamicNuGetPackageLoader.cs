using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Namotion.NuGetPlugins
{
    public class DynamicNuGetPackageLoader : IDynamicNuGetPackageLoader
    {
        private static readonly string[] TargetFrameworksOrder =
        [
            "net10.0",
            "net9.0",
            "net8.0",
            "net7.0",
            "net6.0",
            "net5.0",
            "netstandard2.1",
            "netstandard2.0"
        ];

        private readonly string _cacheDirectory;
        private readonly Dictionary<string, NuGetPackageAssemblyLoadContext> _packages = new Dictionary<string, NuGetPackageAssemblyLoadContext>();

        private readonly NuGetPackageRepository _repository;
        private readonly ILogger _logger;

        public IEnumerable<IDynamicNuGetPackage> LoadedPackages => _packages.Values;

        public DynamicNuGetPackageLoader(NuGetPackageRepository repository, ILogger? logger = null)
            : this(repository, null!, logger)
        {
        }

        public DynamicNuGetPackageLoader(NuGetPackageRepository repository, string cacheDirectory, ILogger? logger = null)
        {
            _repository = repository;
            _logger = logger ?? NullLogger.Instance;

            if (cacheDirectory == null)
            {
                _cacheDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(_cacheDirectory);
            }
            else
            {
                _cacheDirectory = cacheDirectory;
            }
        }

        public IDynamicNuGetPackage LoadPackageFromFile(string filePath)
        {
            using var archive = new ZipArchive(File.OpenRead(filePath));
            var nuspecEntry = archive.Entries.First(e => e.Name.EndsWith(".nuspec"));

            using var streamReader = new StreamReader(nuspecEntry.Open());
            var nuspecXml = streamReader.ReadToEnd();
            var packageName = Regex.Match(nuspecXml, "<id>(.*?)</id>").Groups[1].Value;
            var packageVersion = Regex.Match(nuspecXml, "<version>(.*?)</version>").Groups[1].Value;

            var (context, assemblies) = LoadPackageFromStream(packageName, packageVersion, File.OpenRead(filePath), null);
            return context;
        }

        public async Task<IDynamicNuGetPackage> LoadPackageFromRepositoryAsync(string packageName, string? packageVersion, CancellationToken cancellationToken)
        {
            var (package, stream) = await _repository.DownloadPackageAsync(packageName, packageVersion, cancellationToken);
            using (stream)
            {
                var (context, assemblies) = LoadPackageFromStream(packageName, package.PackageVersion, stream, null);
                return context;
            }
        }

        public IDynamicNuGetPackage LoadPackageFromStream(string packageName, string packageVersion, Stream stream)
        {
            var (context, assemblies) = LoadPackageFromStream(packageName, packageVersion, stream, null);
            return context;
        }

        public bool UnloadPackage(string packageName)
        {
            if (_packages.ContainsKey(packageName))
            {
                _packages[packageName].Unload();
                _packages.Remove(packageName);
                return true;
            }

            return false;
        }

        internal (NuGetPackageAssemblyLoadContext, Assembly[]) LoadPackageFromStream(string packageName, string packageVersion, Stream stream, NuGetPackageAssemblyLoadContext? context)
        {
            var packageCachePath = ExtractPackage(packageName, packageVersion, stream);

            if (_packages.ContainsKey(packageName))
            {
                throw new InvalidOperationException("NuGet Plugin already loaded.");
            }

            var nuspecXml = File.ReadAllText(Directory.GetFiles(packageCachePath, "*.nuspec")[0]);
            var packageDependencies = new Dictionary<string, string>();
            foreach (Match match in Regex.Matches(nuspecXml, "<dependency.* id=\"(.*?)\".* version=\"(.*?)\""))
            {
                var package = match.Groups[1].Value;
                var version = match.Groups[2].Value;

                packageDependencies[package] = version;
            }

            if (context == null)
            {
                context = new NuGetPackageAssemblyLoadContext(packageName, this, _repository, packageDependencies, _logger);
                _packages[packageName] = context;
            }

            var packageLibrariesPath = Path.Combine(packageCachePath, "lib");
            foreach (var targetFramework in TargetFrameworksOrder)
            {
                try
                {
                    var assemblies = LoadAssemblies(context, packageLibrariesPath, targetFramework);
                    if (assemblies.Length > 0)
                    {
                        return (context, assemblies);
                    }
                }
                catch (Exception e)
                {
                    // TODO: What to do here, might be because a .NET 7 assembly is loaded in the .NET 6 runtime
                    _logger.LogWarning(e, "Failed to load NuGet assembly (current runtime might not support assembly, might not be an issue).");
                }
            }

            return (context, Array.Empty<Assembly>());
        }

        private string ExtractPackage(string packageName, string packageVersion, Stream stream)
        {
            var packageCachePath = Path.Combine(_cacheDirectory, packageName, packageVersion);

            if (!Directory.Exists(packageCachePath))
            {
                using var archive = new ZipArchive(stream);
                Directory.CreateDirectory(packageCachePath);
                archive.ExtractToDirectory(packageCachePath);
            }

            return packageCachePath;
        }

        private Assembly[] LoadAssemblies(NuGetPackageAssemblyLoadContext context, string libraryDirectory, string targetFramework)
        {
            var targetFrameworkDirectory = Path.Combine(libraryDirectory, targetFramework);

            var assemblies = new List<Assembly>();
            if (Directory.Exists(targetFrameworkDirectory))
            {
                foreach (var file in Directory.GetFiles(targetFrameworkDirectory, "*.dll"))
                {
                    var assembly = context.LoadFromAssemblyPath(file);
                    context.PackageAssemblies.Add(assembly);
                    assemblies.Add(assembly);

                    _logger.LogInformation($"NuGet plugin assembly {Path.GetFileName(file)} loaded.");
                }
            }

            return assemblies.ToArray();
        }
    }
}
