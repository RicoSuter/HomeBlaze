using System;

namespace Namotion.NuGetPlugins
{
    public class PackageNotFoundException : Exception
    {
        public PackageNotFoundException(string packageName, string? packageVersion)
            : base($"The package {packageName} v{packageVersion} could not be found.")
        {
            PackageName = packageName;
            PackageVersion = packageVersion;
        }

        public string PackageName { get; }

        public string? PackageVersion { get; }
    }
}
