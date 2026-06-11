namespace CqrsScaffold.IntegrationTests;

[Collection(GeneratedSolutionCollection.Name)]
[Trait("Category", "Integration")]
public class ProjectWriterTests
{
    private readonly GeneratedSolutionFixture _fixture;

    public ProjectWriterTests(GeneratedSolutionFixture fixture)
    {
        _fixture = fixture;
    }

    private string Root => _fixture.SolutionRoot;
    private const string App = GeneratedSolutionFixture.ProjectName;
    private const string Entity = GeneratedSolutionFixture.EntityName;

    // .NET 10 SDK's `dotnet new sln` emits the new .slnx format; older SDKs emit .sln.
    private string SolutionFilePath =>
        Directory.GetFiles(Root, $"{App}.sln*").Single();

    [Fact]
    public void SolutionFile_Exists()
    {
        Assert.True(File.Exists(SolutionFilePath));
    }

    [Theory]
    [InlineData("src", "Domain")]
    [InlineData("src", "Application")]
    [InlineData("src", "Infrastructure")]
    [InlineData("src", "API")]
    [InlineData("tests", "Tests")]
    public void ProjectFile_Exists(string folder, string layer)
    {
        var csproj = Path.Combine(Root, folder, $"{App}.{layer}", $"{App}.{layer}.csproj");

        Assert.True(File.Exists(csproj), $"Missing: {csproj}");
    }

    [Fact]
    public void DomainProject_ContainsEntity_AndNoDefaultClass()
    {
        var domainPath = Path.Combine(Root, "src", $"{App}.Domain");

        Assert.True(File.Exists(Path.Combine(domainPath, "Entities", $"{Entity}.cs")));
        Assert.False(File.Exists(Path.Combine(domainPath, "Class1.cs")));
    }

    [Fact]
    public void ApplicationProject_ContainsCommandAndQueryFiles()
    {
        var appPath = Path.Combine(Root, "src", $"{App}.Application");
        var commands = Path.Combine(appPath, "Commands", $"Create{Entity}");
        var queries = Path.Combine(appPath, "Queries", $"Get{Entity}");

        Assert.True(File.Exists(Path.Combine(commands, $"Create{Entity}Command.cs")));
        Assert.True(File.Exists(Path.Combine(commands, $"Create{Entity}Handler.cs")));
        Assert.True(File.Exists(Path.Combine(commands, $"Create{Entity}Validator.cs")));
        Assert.True(File.Exists(Path.Combine(queries, $"Get{Entity}Query.cs")));
        Assert.True(File.Exists(Path.Combine(queries, $"Get{Entity}Handler.cs")));
        Assert.False(File.Exists(Path.Combine(appPath, "Class1.cs")));
    }

    [Fact]
    public void InfrastructureProject_ContainsDbContext()
    {
        var infraPath = Path.Combine(Root, "src", $"{App}.Infrastructure");

        Assert.True(File.Exists(Path.Combine(infraPath, "Persistence", "ApplicationDbContext.cs")));
        Assert.False(File.Exists(Path.Combine(infraPath, "Class1.cs")));
    }

    [Fact]
    public void ApiProject_ProgramReferencesGeneratedCommand()
    {
        var programPath = Path.Combine(Root, "src", $"{App}.API", "Program.cs");

        Assert.True(File.Exists(programPath));
        Assert.Contains(App, File.ReadAllText(programPath));
    }

    [Fact]
    public void TestProject_ContainsHandlerTest_AndNoDefaultTest()
    {
        var testsPath = Path.Combine(Root, "tests", $"{App}.Tests");

        Assert.True(File.Exists(Path.Combine(testsPath, $"Create{Entity}HandlerTest.cs")));
        Assert.False(File.Exists(Path.Combine(testsPath, "UnitTest1.cs")));
    }

    [Theory]
    [InlineData("src", "Application", "Domain")]
    [InlineData("src", "Infrastructure", "Domain")]
    [InlineData("src", "API", "Application")]
    [InlineData("tests", "Tests", "Application")]
    public void ProjectReference_IsWired(string folder, string layer, string referenced)
    {
        var csproj = Path.Combine(Root, folder, $"{App}.{layer}", $"{App}.{layer}.csproj");

        Assert.Contains($"{App}.{referenced}.csproj", File.ReadAllText(csproj));
    }

    [Fact]
    public void SolutionFile_IncludesAllFiveProjects()
    {
        var sln = File.ReadAllText(SolutionFilePath);

        Assert.Contains($"{App}.Domain", sln);
        Assert.Contains($"{App}.Application", sln);
        Assert.Contains($"{App}.Infrastructure", sln);
        Assert.Contains($"{App}.API", sln);
        Assert.Contains($"{App}.Tests", sln);
    }
}
