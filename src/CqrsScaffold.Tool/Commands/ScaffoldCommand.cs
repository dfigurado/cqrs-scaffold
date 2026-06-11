using CqrsScaffold.Configuration;
using CqrsScaffold.Tool.Configuration;
using CqrsScaffold.Tool.Engine;
using CqrsScaffold.Tool.Validation;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CqrsScaffold.Tool.Commands
{
    public static class ScaffoldCommand
    {
        public static RootCommand Create()
        {
            var nameOption = new Option<string>("--name", "-n")
            {
                Description = "Project name (e.g., MyProject)",
                Required = true
            };

            var outputOption = new Option<DirectoryInfo>("--output", "-o")
            {
                DefaultValueFactory = _ => new DirectoryInfo(Directory.GetCurrentDirectory()),
                Description = "Output directory"
            };

            var serviceBusOption = new Option<bool>("--service-bus")
            {
                Description = "Include Azure Service Bus integration"
            };

            var authOption = new Option<string?>("--auth")
            {
                Description = "Authentication type (e.g., Jwt, IdentityServer, None)"
            };

            var dockerOption = new Option<bool>("--docker")
            {
                Description = "Include Docker support"
            };

            var ciOption = new Option<string?>("--ci")
            {
                Description = "CI/CD pipeline type (e.g., GitHubActions, AzurePipelines, None)"
            };

            var configOption = new Option<FileInfo>("--config")
            {
                Description = "Path to JSON configuration file"
            };

            var rootCommand = new RootCommand("CQRS/MediateR/Clean Architecture project generator")
            {
                nameOption,
                outputOption,
                serviceBusOption,
                authOption,
                dockerOption,
                ciOption,
                configOption
            };

            rootCommand.SetAction(async (parseResult, cancellationToken) =>
            {
                var name = parseResult.GetValue(nameOption);
                var output = parseResult.GetValue(outputOption);
                var includeServiceBus = parseResult.GetValue(serviceBusOption);
                var authType = parseResult.GetValue(authOption);
                var includeDocker = parseResult.GetValue(dockerOption);
                var ciType = parseResult.GetValue(ciOption);
                var configFile = parseResult.GetValue(configOption);

                var config = ConfigLoader.Load(configFile, new ScaffoldConfig
                {
                    ProjectName = name,
                    OutputPath = output.FullName,
                    IncludeServiceBus = includeServiceBus,
                    AuthType = authType,
                    IncludeDocker = includeDocker,
                    CiType = ciType
                });

                Console.WriteLine($"🚀 Generating {config.ProjectName}...");

                var engine = new TemplateEngine();
                var writer = new ProjectWriter(engine);

                await writer.GenerateAsync(config);

                var validator = new SolutionValidator();
                var success = await validator.ValidateAsync(config.OutputPath, config.ProjectName);

                Console.WriteLine(success
                    ? "✅ Project generated successfully!"
                    : "❌ Project generation failed. Please check the output for details.");
            });

            return rootCommand;
        }
    }
}