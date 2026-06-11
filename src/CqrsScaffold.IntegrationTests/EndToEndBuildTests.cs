using CqrsScaffold.Tool.Validation;

namespace CqrsScaffold.IntegrationTests;

[Collection(GeneratedSolutionCollection.Name)]
[Trait("Category", "Integration")]
public class EndToEndBuildTests
{
    private readonly GeneratedSolutionFixture _fixture;

    public EndToEndBuildTests(GeneratedSolutionFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GeneratedSolution_BuildsWithZeroErrors()
    {
        var validator = new SolutionValidator();

        var success = await validator.ValidateAsync(
            _fixture.OutputPath, GeneratedSolutionFixture.ProjectName);

        Assert.True(success, "dotnet build on the generated solution did not exit 0.");
    }
}
