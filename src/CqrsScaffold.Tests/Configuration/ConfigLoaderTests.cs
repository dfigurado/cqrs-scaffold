using CqrsScaffold.Configuration;
using CqrsScaffold.Tool.Configuration;

namespace CqrsScaffold.Tests.Configuration;

public class ConfigLoaderTests
{
    [Fact]
    public void Load_ReturnsCliConfig_WhenConfigFileIsNull()
    {
        var cli = new ScaffoldConfig { ProjectName = "CliOnly" };

        var result = ConfigLoader.Load(null, cli);

        Assert.Equal("CliOnly", result.ProjectName);
    }

    [Fact]
    public void Load_ReturnsCliConfig_WhenConfigFileDoesNotExist()
    {
        var cli = new ScaffoldConfig { EntityName = "CliEntity" };
        var missing = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));

        var result = ConfigLoader.Load(missing, cli);

        Assert.Equal("CliEntity", result.EntityName);
    }

    [Fact]
    public void Load_MergesFileConfig_AndCliOverridesValues()
    {
        var configPath = Path.Combine(Path.GetTempPath(), $"cqrs-{Guid.NewGuid():N}.json");
        File.WriteAllText(
            configPath,
            """
            {
              "projectName": "FromFile",
              "entityName": "FileEntity",
              "includeDocker": true
            }
            """);

        try
        {
            var cli = new ScaffoldConfig
            {
                EntityName = "CliEntity",
                IncludeServiceBus = true,
            };

            var result = ConfigLoader.Load(new FileInfo(configPath), cli);

            Assert.Equal("FromFile", result.ProjectName);
            Assert.Equal("CliEntity", result.EntityName);
            Assert.True(result.IncludeDocker);
            Assert.True(result.IncludeServiceBus);
        }
        finally
        {
            File.Delete(configPath);
        }
    }

    [Fact]
    public void Load_UsesCaseInsensitiveJsonPropertyNames()
    {
        var configPath = Path.Combine(Path.GetTempPath(), $"cqrs-{Guid.NewGuid():N}.json");
        File.WriteAllText(
            configPath,
            """
            {
              "PROJECTNAME": "CaseInsensitive"
            }
            """);

        try
        {
            var result = ConfigLoader.Load(new FileInfo(configPath), new ScaffoldConfig());

            Assert.Equal("CaseInsensitive", result.ProjectName);
        }
        finally
        {
            File.Delete(configPath);
        }
    }
}
