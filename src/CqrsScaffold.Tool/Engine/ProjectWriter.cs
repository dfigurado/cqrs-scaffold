using CqrsScaffold.Configuration;
using System.Diagnostics;

namespace CqrsScaffold.Tool.Engine
{
    public class ProjectWriter
    {
        private readonly TemplateEngine _engine;

        public ProjectWriter(TemplateEngine engine)
        {
            _engine = engine;
        }

        public async Task GenerateAsync(ScaffoldConfig config)
        {
            var basePath = Path.Combine(config.OutputPath, config.ProjectName);
            Directory.CreateDirectory(basePath);

            // Create solution file
            await RunDotnetAsync($"new sln -n {config.ProjectName}", basePath);

            // Create projects
            var srcPath = Path.Combine(basePath, "src");
            var testsPath = Path.Combine(basePath, "tests");

            await CreateDomainProjectAsync(config, srcPath);
            await CreateApplicationProjectAsync(config, srcPath);
            await CreateInfrastructureProjectAsync(config, srcPath);
            await CreateApiProjectAsync(config, srcPath);
            await CreateTestProjectAsync(config, testsPath);

            // Wire up project-to-project references
            await AddProjectReferencesAsync(config, srcPath, testsPath);

            // Add projects to solution
            await AddProjectsToSolutionAsync(config, basePath);
        }

        private async Task CreateDomainProjectAsync(ScaffoldConfig config, string srcPath)
        {
            var projectPath = Path.Combine(srcPath, $"{config.ProjectName}.Domain");
            await RunDotnetAsync("new classlib", projectPath);

            var entitiesPath = Path.Combine(projectPath, "Entities");
            Directory.CreateDirectory(entitiesPath);

            var entityContent = _engine.Render("Domain.Entity.cs", config);
            await File.WriteAllTextAsync(Path.Combine(entitiesPath, $"{config.EntityName}.cs"), entityContent);

            // Remove default Class1.cs
            var defaultClassPath = Path.Combine(projectPath, "Class1.cs");
            if (File.Exists(defaultClassPath))
            {
                File.Delete(defaultClassPath);
            }
        }

        private async Task CreateApplicationProjectAsync(ScaffoldConfig config, string srcPath)
        {
            var projectPath = Path.Combine(srcPath, $"{config.ProjectName}.Application");
            await RunDotnetAsync("new classlib", projectPath);

            // Add package references
            await RunDotnetAsync("add package MediatR", projectPath);
            await RunDotnetAsync("add package FluentValidation", projectPath);
            await RunDotnetAsync("add package AutoMapper", projectPath);

            // Create directories
            var commandsPath = Path.Combine(projectPath, "Commands", $"Create{config.EntityName}");
            var queriesPath = Path.Combine(projectPath, "Queries", $"Get{config.EntityName}");
            Directory.CreateDirectory(commandsPath);
            Directory.CreateDirectory(queriesPath);

            // Generate files
            await File.WriteAllTextAsync(
                Path.Combine(commandsPath, $"Create{config.EntityName}Command.cs"),
                _engine.Render("Application.Command", config));

            await File.WriteAllTextAsync(
                Path.Combine(commandsPath, $"Create{config.EntityName}Handler.cs"),
                _engine.Render("Application.CommandHandler", config));

            await File.WriteAllTextAsync(
                Path.Combine(commandsPath, $"Create{config.EntityName}Validator.cs"),
                _engine.Render("Application.CommandValidator", config));

            await File.WriteAllTextAsync(
                Path.Combine(queriesPath, $"Get{config.EntityName}Query.cs"),
                _engine.Render("Application.Query", config));

            await File.WriteAllTextAsync(
                Path.Combine(queriesPath, $"Get{config.EntityName}Handler.cs"),
                _engine.Render("Application.QueryHandler", config));

            // Remove default Class1.cs
            var defaultClassPath = Path.Combine(projectPath, "Class1.cs");
            if (File.Exists(defaultClassPath))
            {
                File.Delete(defaultClassPath);
            }
        }

        private async Task CreateInfrastructureProjectAsync(ScaffoldConfig config, string srcPath)
        {
            var projectPath = Path.Combine(srcPath, $"{config.ProjectName}.Infrastructure");
            await RunDotnetAsync("new classlib", projectPath);

            await RunDotnetAsync("add package Microsoft.EntityFrameworkCore", projectPath);

            var persistencePath = Path.Combine(projectPath, "Persistence");
            Directory.CreateDirectory(persistencePath);

            await File.WriteAllTextAsync(
                Path.Combine(persistencePath, $"ApplicationDbContext.cs"),
                _engine.Render("Infrastructure.DbContext", config));

            var defaultClass = Path.Combine(projectPath, "Class1.cs");
            if (File.Exists(defaultClass)) File.Delete(defaultClass);
        }

        private async Task CreateApiProjectAsync(ScaffoldConfig config, string srcPath)
        {
            var projectPath = Path.Combine(srcPath, $"{config.ProjectName}.API");
            await RunDotnetAsync("new webapi", projectPath);

            await RunDotnetAsync("add package Swashbuckle.AspNetCore", projectPath);

            await File.WriteAllTextAsync(
                Path.Combine(projectPath, "Program.cs"),
                _engine.Render("API.Program", config));
        }

        private async Task CreateTestProjectAsync(ScaffoldConfig config, string testPath)
        {
            var projectPath = Path.Combine(testPath, $"{config.ProjectName}.Tests");
            await RunDotnetAsync("new xunit", projectPath);

            await RunDotnetAsync("add package Moq", projectPath);
            await RunDotnetAsync("add package FluentAssertions", projectPath);

            await File.WriteAllTextAsync(
                Path.Combine(projectPath, $"Create{config.EntityName}HandlerTest.cs"),
                _engine.Render("Tests.CommandHandlerTest", config));

            var defaultTest = Path.Combine(projectPath, "UnitTest1.cs");
            if (File.Exists(defaultTest)) File.Delete(defaultTest);
        }

        private async Task AddProjectReferencesAsync(ScaffoldConfig config, string srcPath, string testsPath)
        {
            var domainProject = Path.Combine(srcPath, $"{config.ProjectName}.Domain", $"{config.ProjectName}.Domain.csproj");
            var applicationProject = Path.Combine(srcPath, $"{config.ProjectName}.Application", $"{config.ProjectName}.Application.csproj");

            // Application -> Domain
            await RunDotnetAsync($"add reference \"{domainProject}\"",
                Path.Combine(srcPath, $"{config.ProjectName}.Application"));

            // Infrastructure -> Domain
            await RunDotnetAsync($"add reference \"{domainProject}\"",
                Path.Combine(srcPath, $"{config.ProjectName}.Infrastructure"));

            // API -> Application
            await RunDotnetAsync($"add reference \"{applicationProject}\"",
                Path.Combine(srcPath, $"{config.ProjectName}.API"));

            // Tests -> Application
            await RunDotnetAsync($"add reference \"{applicationProject}\"",
                Path.Combine(testsPath, $"{config.ProjectName}.Tests"));
        }

        private async Task AddProjectsToSolutionAsync(ScaffoldConfig config, string basePath)
        {
            var projects = new[]
            {
                $"src/{config.ProjectName}.Domain/{config.ProjectName}.Domain.csproj",
                $"src/{config.ProjectName}.Application/{config.ProjectName}.Application.csproj",
                $"src/{config.ProjectName}.Infrastructure/{config.ProjectName}.Infrastructure.csproj",
                $"src/{config.ProjectName}.API/{config.ProjectName}.API.csproj",
                $"tests/{config.ProjectName}.Tests/{config.ProjectName}.Tests.csproj"
            };

            foreach (var project in projects)
            {
                await RunDotnetAsync($"sln add {project}", basePath);
            }
        }

        private static async Task RunDotnetAsync(string arguments, string workingDirectory)
        {
            Directory.CreateDirectory(workingDirectory);

            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException($"Failed to start: dotnet {arguments}");

            // Drain output streams to avoid pipe-buffer deadlock
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();
            await Task.WhenAll(stdoutTask, stderrTask);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"'dotnet {arguments}' failed with exit code {process.ExitCode}:{Environment.NewLine}{stdoutTask.Result}{stderrTask.Result}");
            }
        }
    }
}
