using System.Text.Json;
using CqrsScaffold.Configuration;
using CqrsScaffold.Tool.Configuration;

namespace CqrsScaffold.Tests;

public class ConfigLoaderTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file)) File.Delete(file);
        }
    }

    private FileInfo WriteTempConfig(string json)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, json);
        _tempFiles.Add(path);
        return new FileInfo(path);
    }

    [Fact]
    public void Load_NullConfigFile_ReturnsCliConfigUnchanged()
    {
        var cliConfig = new ScaffoldConfig { ProjectName = "CliApp" };

        var result = ConfigLoader.Load(null, cliConfig);

        Assert.Same(cliConfig, result);
    }

    [Fact]
    public void Load_NonExistentConfigFile_ReturnsCliConfigUnchanged()
    {
        var cliConfig = new ScaffoldConfig { ProjectName = "CliApp" };
        var missingFile = new FileInfo(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid()}.json"));

        var result = ConfigLoader.Load(missingFile, cliConfig);

        Assert.Same(cliConfig, result);
    }

    [Fact]
    public void Load_CliProjectNameOverridesFileProjectName()
    {
        var file = WriteTempConfig("""{ "ProjectName": "FileApp" }""");
        var cliConfig = new ScaffoldConfig { ProjectName = "CliApp" };

        var result = ConfigLoader.Load(file, cliConfig);

        Assert.Equal("CliApp", result.ProjectName);
    }

    [Fact]
    public void Load_FileProjectNameUsedWhenCliProjectNameIsNull()
    {
        var file = WriteTempConfig("""{ "ProjectName": "FileApp" }""");
        var cliConfig = new ScaffoldConfig { ProjectName = null! };

        var result = ConfigLoader.Load(file, cliConfig);

        Assert.Equal("FileApp", result.ProjectName);
    }

    [Fact]
    public void Load_FileAuthTypeUsedWhenCliAuthTypeIsNull()
    {
        var file = WriteTempConfig("""{ "AuthType": "Jwt" }""");
        var cliConfig = new ScaffoldConfig { AuthType = null };

        var result = ConfigLoader.Load(file, cliConfig);

        Assert.Equal("Jwt", result.AuthType);
    }

    [Fact]
    public void Load_IncludeServiceBusTrueWhenCliSetsIt_EvenIfFileIsFalse()
    {
        var file = WriteTempConfig("""{ "IncludeServiceBus": false }""");
        var cliConfig = new ScaffoldConfig { IncludeServiceBus = true };

        var result = ConfigLoader.Load(file, cliConfig);

        Assert.True(result.IncludeServiceBus);
    }

    [Fact]
    public void Load_IncludeServiceBusTrueWhenFileSetsIt_EvenIfCliIsFalse()
    {
        var file = WriteTempConfig("""{ "IncludeServiceBus": true }""");
        var cliConfig = new ScaffoldConfig { IncludeServiceBus = false };

        var result = ConfigLoader.Load(file, cliConfig);

        Assert.True(result.IncludeServiceBus);
    }

    [Fact]
    public void Load_IncludeDockerFollowsSameOrLogic()
    {
        var file = WriteTempConfig("""{ "IncludeDocker": true }""");
        var cliConfig = new ScaffoldConfig { IncludeDocker = false };

        var result = ConfigLoader.Load(file, cliConfig);

        Assert.True(result.IncludeDocker);
    }

    [Fact]
    public void Load_JsonDeserialisationIsCaseInsensitive()
    {
        var file = WriteTempConfig("""{ "projectName": "FileApp" }""");
        var cliConfig = new ScaffoldConfig { ProjectName = null! };

        var result = ConfigLoader.Load(file, cliConfig);

        Assert.Equal("FileApp", result.ProjectName);
    }

    [Fact]
    public void Load_MalformedJson_ThrowsJsonException()
    {
        var file = WriteTempConfig("{ not valid json !");
        var cliConfig = new ScaffoldConfig();

        Assert.Throws<JsonException>(() => ConfigLoader.Load(file, cliConfig));
    }
}
