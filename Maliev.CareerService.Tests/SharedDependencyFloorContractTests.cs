using System.Xml.Linq;
using Xunit;

namespace Maliev.CareerService.Tests;

public sealed class SharedDependencyFloorContractTests
{
    [Fact]
    public void SharedDependencyFloors_MatchCurrentServiceDefaults()
    {
        var root = FindRoot();
        var infrastructure = XDocument.Load(Path.Combine(
            root,
            "Maliev.CareerService.Infrastructure",
            "Maliev.CareerService.Infrastructure.csproj"));
        var tests = XDocument.Load(Path.Combine(
            root,
            "Maliev.CareerService.Tests",
            "Maliev.CareerService.Tests.csproj"));

        AssertVersion(infrastructure, "MassTransit.Abstractions", "[8.5.10, 9.0.0)");
        AssertVersion(infrastructure, "MassTransit.EntityFrameworkCore", "[8.5.10, 9.0.0)");
        AssertVersion(infrastructure, "Microsoft.EntityFrameworkCore.Design", "10.0.10");
        AssertVersion(infrastructure, "Npgsql.EntityFrameworkCore.PostgreSQL", "10.0.3");
        AssertVersion(infrastructure, "HtmlSanitizer", "9.1.949-beta");
        AssertVersion(tests, "Npgsql.EntityFrameworkCore.PostgreSQL", "10.0.3");
    }

    private static void AssertVersion(XDocument project, string packageName, string expectedVersion)
    {
        var reference = Assert.Single(
            project.Descendants("PackageReference"),
            element => element.Attribute("Include")?.Value == packageName);
        Assert.Equal(expectedVersion, reference.Attribute("Version")?.Value);
    }

    private static string FindRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Maliev.CareerService.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate CareerService repository root.");
    }
}
