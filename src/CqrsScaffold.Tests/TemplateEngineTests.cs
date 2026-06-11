using CqrsScaffold.Configuration;
using CqrsScaffold.Tool.Engine;

namespace CqrsScaffold.Tests;

public class TemplateEngineTests
{
    private readonly TemplateEngine _engine = new();

    private static ScaffoldConfig DefaultConfig => new()
    {
        ProjectName = "SampleApp",
        EntityName = "Order"
    };

    [Fact]
    public void Render_DomainEntity_ReturnsNonEmptyString()
    {
        var result = _engine.Render("Domain.Entity.cs", DefaultConfig);

        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public void Render_DomainEntity_ContainsEntityName()
    {
        var result = _engine.Render("Domain.Entity.cs", DefaultConfig);

        Assert.Contains("public class Order", result);
    }

    [Fact]
    public void Render_DomainEntity_ContainsProjectNameInNamespace()
    {
        var result = _engine.Render("Domain.Entity.cs", DefaultConfig);

        Assert.Contains("namespace SampleApp.Domain.Entities;", result);
    }

    [Fact]
    public void Render_ApplicationCommand_ContainsIRequestOfGuid()
    {
        var result = _engine.Render("Application.Command", DefaultConfig);

        Assert.Contains("IRequest<Guid>", result);
    }

    [Fact]
    public void Render_NonexistentTemplate_ThrowsWithTemplateName()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => _engine.Render("nonexistent.template", DefaultConfig));

        Assert.Contains("nonexistent.template", ex.Message);
    }

    [Theory]
    [InlineData("Domain.Entity.cs")]
    [InlineData("Application.Command")]
    [InlineData("Application.CommandHandler")]
    [InlineData("Application.CommandValidator")]
    [InlineData("Application.Query")]
    [InlineData("Application.QueryHandler")]
    [InlineData("Infrastructure.DbContext")]
    [InlineData("API.Program")]
    [InlineData("Tests.CommandHandlerTest")]
    public void Render_AllKnownTemplates_ResolveWithoutThrowing(string templateName)
    {
        var result = _engine.Render(templateName, DefaultConfig);

        Assert.False(string.IsNullOrWhiteSpace(result));
    }
}
