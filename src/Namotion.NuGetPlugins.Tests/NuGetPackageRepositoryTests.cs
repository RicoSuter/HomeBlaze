using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Namotion.NuGetPlugins.Tests
{
    public class NuGetPackageRepositoryTests
    {
        [Fact]
        public async Task WhenPackageDoesNotExist_ThenPackageNotFoundExceptionIsThrown()
        {
            // Arrange
            var repository = NuGetPackageRepository.CreateForNuGetOrg();

            // Act
            await Assert.ThrowsAsync<PackageNotFoundException>(async () =>
                await repository.DownloadPackageAsync(Guid.NewGuid().ToString(), "1.0.0", CancellationToken.None));
        }

        [Fact]
        public async Task WhenSearchingPackages_ThenPackagesAreFound()
        {
            // Arrange
            var repository = NuGetPackageRepository.CreateForNuGetOrg(p =>
                p.PackageName.IndexOf("xunit", StringComparison.InvariantCultureIgnoreCase) < 0 &&
                p.Title.IndexOf("xunit", StringComparison.InvariantCultureIgnoreCase) < 0 &&
                p.Description.IndexOf("xunit", StringComparison.InvariantCultureIgnoreCase) < 0 &&
                p.Tags.Contains("xunit", StringComparer.InvariantCultureIgnoreCase));

            // Act
            var packages = await repository.SearchPackagesAsync("xunit", 0, 100, CancellationToken.None);

            // Assert
            Assert.True(packages.Any());
        }

        [Fact]
        public void WhenPackageIsLoaded_ThenAssembliesHasOne()
        {
            // Arrange
            var repository = NuGetPackageRepository.CreateForNuGetOrg();
            var manager = new DynamicNuGetPackageLoader(repository);

            // Act
            var package = manager.LoadPackageFromFile("../../../Sample/Namotion.NuGetPlugins.Sample.1.0.0.nupkg");
            var obj = Activator.CreateInstance(package
                .PackageAssemblies.First()
                .ExportedTypes.FirstOrDefault(t => t.Name == "Class1")!);

            // Assert
            Assert.Single(manager.LoadedPackages);
            Assert.NotNull(obj);
        }

        [Fact]
        public void WhenPackageIsUnloaded_ThenAssembliesIsEmpty()
        {
            // Arrange
            var repository = NuGetPackageRepository.CreateForNuGetOrg();
            var manager = new DynamicNuGetPackageLoader(repository);

            // Act
            var package = manager.LoadPackageFromFile("../../../Sample/Namotion.NuGetPlugins.Sample.1.0.0.nupkg");
            Activator.CreateInstance(package
                         .PackageAssemblies.First()
                         .ExportedTypes.FirstOrDefault(t => t.Name == "Class1")!);
            manager.UnloadPackage("Namotion.NuGetPlugins.Sample");

            // Assert
            Assert.Empty(manager.LoadedPackages);

            // Act
            package = manager.LoadPackageFromFile("../../../Sample/Namotion.NuGetPlugins.Sample.1.0.0.nupkg");
            Activator.CreateInstance(package
                .PackageAssemblies.First()
                .ExportedTypes.FirstOrDefault(t => t.Name == "Class1")!);

            // Assert
            Assert.Single(manager.LoadedPackages);
        }
    }
}
