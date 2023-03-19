using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace Namotion.NuGetPlugins
{
    internal class NuGetPackageAssemblyLoadContext : AssemblyLoadContext, IDynamicNuGetPackage
    {
        private readonly NuGetPackageRepository _repository;
        private readonly DynamicNuGetPackageLoader _loader;

        private readonly Dictionary<string, string> _packageDependencies = new Dictionary<string, string>();
        private readonly ILogger _logger;

        public NuGetPackageAssemblyLoadContext(
            string packageName,
            DynamicNuGetPackageLoader loader,
            NuGetPackageRepository repository,
            Dictionary<string, string> packageDependencies,
            ILogger logger)
            : base(nameof(NuGetPackageAssemblyLoadContext), true)
        {
            PackageName = packageName;

            _loader = loader;
            _repository = repository;
            _packageDependencies = packageDependencies;
            _logger = logger;

            Resolving += OnResolveDependency;
        }

        public string PackageName { get; }

        public List<Assembly> PackageAssemblies { get; } = new List<Assembly>();

        IEnumerable<Assembly> IDynamicNuGetPackage.PackageAssemblies => PackageAssemblies.AsEnumerable();

        IEnumerable<Assembly> IDynamicNuGetPackage.Assemblies => Assemblies.AsEnumerable();

        private Assembly? OnResolveDependency(AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName)
        {
            if (assemblyName.Name != null && _packageDependencies.ContainsKey(assemblyName.Name))
            {
                var packageVersion = _packageDependencies[assemblyName.Name];
                try
                {
                    var (package, stream) = _repository.DownloadPackageAsync(assemblyName.Name, packageVersion, CancellationToken.None).GetAwaiter().GetResult();
                    using (stream)
                    {
                        var (context, assemblies) = _loader.LoadPackageFromStream(assemblyName.Name, package.PackageVersion, stream, this);
                        return assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, $"Failed to load NuGet plugin dependency assembly {assemblyName.Name} v{packageVersion}.");
                }
            }

            return null;
        }
    }
}
