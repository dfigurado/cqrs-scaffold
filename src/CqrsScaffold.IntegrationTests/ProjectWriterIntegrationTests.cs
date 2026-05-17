using System.Diagnostics;
using CqrsScaffold.Configuration;
using CqrsScaffold.Tool.Engine;

namespace CqrsScaffold.IntegrationTests;

public class ProjectWriterIntegrationTests : IDisposable
{
    private readonly string _outputRoot = Path.Combine(
        Path.GetTempPath(),
        "cqrs-scaffold-integration",
        Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (!Directory.Exists(_outputRoot))
        {
            return;
        }

        try
        {
            Directory.Delete(_outputRoot, recursive: true);
        }
        catch (IOException)
        {
            // Temp cleanup is best-effort on shared CI agents.
        }
    }

    [Fact]
    public async Task GenerateAsync_CreatesSolutionWithLayeredProjects()
    {
        if (!IsDotNetSdkAvailable())
        {
            return;
        }

        var config = new ScaffoldConfig
        {
            ProjectName = "SampleApp",
            EntityName = "Product",
            OutputPath = _outputRoot,
        };

        var writer = new ProjectWriter(new TemplateEngine());
        await writer.GenerateAsync(config);

        var basePath = Path.Combine(_outputRoot, config.ProjectName);

        Assert.True(File.Exists(Path.Combine(basePath, $"{config.ProjectName}.sln")));
        Assert.True(File.Exists(Path.Combine(
            basePath,
            "src",
            $"{config.ProjectName}.Domain",
            "Entities",
            $"{config.EntityName}.cs")));
        Assert.True(File.Exists(Path.Combine(
            basePath,
            "src",
            $"{config.ProjectName}.Application",
            "Commands",
            $"Create{config.EntityName}",
            $"Create{config.EntityName}Command.cs")));
        Assert.True(File.Exists(Path.Combine(
            basePath,
            "src",
            $"{config.ProjectName}.Infrastructure",
            "Persistence",
            "ApplicationDbContext.cs")));
        Assert.True(File.Exists(Path.Combine(
            basePath,
            "src",
            $"{config.ProjectName}.API",
            "Program.cs")));
        Assert.True(File.Exists(Path.Combine(
            basePath,
            "tests",
            $"{config.ProjectName}.Tests",
            $"Create{config.EntityName}HandlerTest.cs")));

        var commandContent = await File.ReadAllTextAsync(Path.Combine(
            basePath,
            "src",
            $"{config.ProjectName}.Application",
            "Commands",
            $"Create{config.EntityName}",
            $"Create{config.EntityName}Command.cs"));

        Assert.Contains("CreateProductCommand", commandContent, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(
            basePath,
            "src",
            $"{config.ProjectName}.Domain",
            "Class1.cs")));
    }

    private static bool IsDotNetSdkAvailable()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process is null)
            {
                return false;
            }

            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            return false;
        }
    }
}
