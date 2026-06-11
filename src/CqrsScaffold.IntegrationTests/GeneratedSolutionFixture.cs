using CqrsScaffold.Configuration;
using CqrsScaffold.Tool.Engine;

namespace CqrsScaffold.IntegrationTests;

/// <summary>
/// Runs the full generation pipeline once and shares the output across all
/// tests in the collection. Generation spawns many `dotnet` processes, so a
/// single shared run keeps the suite to a few minutes instead of multiplying.
/// </summary>
public sealed class GeneratedSolutionFixture : IAsyncLifetime
{
    public const string ProjectName = "ScaffoldItApp";
    public const string EntityName = "Order";

    public string OutputPath { get; } =
        Path.Combine(Path.GetTempPath(), $"cqrs-scaffold-it-{Guid.NewGuid():N}");

    public string SolutionRoot => Path.Combine(OutputPath, ProjectName);

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(OutputPath);

        var config = new ScaffoldConfig
        {
            ProjectName = ProjectName,
            EntityName = EntityName,
            OutputPath = OutputPath
        };

        var writer = new ProjectWriter(new TemplateEngine());
        await writer.GenerateAsync(config);
    }

    public Task DisposeAsync()
    {
        try
        {
            if (Directory.Exists(OutputPath))
            {
                Directory.Delete(OutputPath, recursive: true);
            }
        }
        catch (IOException)
        {
            // Best-effort cleanup; leftover temp dirs are harmless.
        }
        catch (UnauthorizedAccessException)
        {
        }

        return Task.CompletedTask;
    }
}

[CollectionDefinition(Name)]
public sealed class GeneratedSolutionCollection : ICollectionFixture<GeneratedSolutionFixture>
{
    public const string Name = "GeneratedSolution";
}
