using NuGet.Protocol.Core.Types;
using System;
using System.Linq;

namespace Namotion.NuGetPlugins
{
    public class NuGetPackage
    {
        private readonly IPackageSearchMetadata _metadata;

        public NuGetPackage(IPackageSearchMetadata metadata)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));

            Tags = metadata.Tags
                .Split(",")
                .Select(t => t.Trim())
                .ToArray();
        }

        public string PackageName => _metadata.Identity.Id;

        public string PackageVersion => _metadata.Identity.Version.ToNormalizedString();

        public string Title => _metadata.Title;

        public string Description => _metadata.Description;

        public string Authors => _metadata.Authors;

        public Uri IconUrl => _metadata.IconUrl;

        public Uri ProjectUrl => _metadata.ProjectUrl;

        public string[] Tags { get; }
    }
}
