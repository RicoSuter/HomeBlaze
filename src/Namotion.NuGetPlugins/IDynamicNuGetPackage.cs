using System.Collections.Generic;
using System.Reflection;

namespace Namotion.NuGetPlugins
{
    public interface IDynamicNuGetPackage
    {
        string PackageName { get; }

        IEnumerable<Assembly> PackageAssemblies { get; }

        IEnumerable<Assembly> Assemblies { get; }
    }
}
