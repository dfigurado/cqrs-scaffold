using CqrsScaffold.Configuration;
using System.Text.Json;

namespace CqrsScaffold.Tool.Configuration
{
    public static class ConfigLoader
    {
        public static ScaffoldConfig Load(FileInfo? configFile, ScaffoldConfig cliConfig)
        {
            if (configFile is null || !configFile.Exists)
            {
                return cliConfig;
            }

            var json = File.ReadAllText(configFile.FullName);
            var fileConfig = JsonSerializer.Deserialize<ScaffoldConfig>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new ScaffoldConfig();

            // CLI arguments override file config
            return new ScaffoldConfig
            {
                ProjectName = cliConfig.ProjectName ?? fileConfig.ProjectName,
                OutputPath = cliConfig.OutputPath ?? fileConfig.OutputPath,
                EntityName = cliConfig.EntityName ?? fileConfig.EntityName,
                IncludeServiceBus = cliConfig.IncludeServiceBus || fileConfig.IncludeServiceBus,
                AuthType = cliConfig.AuthType ?? fileConfig.AuthType,
                IncludeDocker = cliConfig.IncludeDocker || fileConfig.IncludeDocker,
                CiType = cliConfig.CiType ?? fileConfig.CiType
            };
        }
    }
}
