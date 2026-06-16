using CqrsScaffold.Tool.Engine;

namespace CqrsScaffold.Tests.Engine;

public class TemplateEngineTests
{
    [Fact]
    public void Render_ReplacesModelProperties_InCommandTemplate()
    {
        var engine = new TemplateEngine();
        var model = new { ProjectName = "AcmeApp", EntityName = "Order" };

        var output = engine.Render("Application.Command", model);

        Assert.Contains("namespace AcmeApp.Application.Commands.CreateOrder;", output);
        Assert.Contains("CreateOrderCommand", output);
        Assert.Contains("MediatR;", output);
    }

    [Fact]
    public void Render_ReplacesModelProperties_InTestTemplate()
    {
        var engine = new TemplateEngine();
        var model = new { ProjectName = "Shop", EntityName = "Product" };

        var output = engine.Render("Tests.CommandHandlerTest", model);

        Assert.Contains("namespace Shop.Tests;", output);
        Assert.Contains("CreateProductCommand", output);
        Assert.Contains("FluentAssertions", output);
    }

    [Fact]
    public void Render_Throws_WhenTemplateIsMissing()
    {
        var engine = new TemplateEngine();

        var act = () => engine.Render("Templates/DoesNotExist", new { ProjectName = "X" });

        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Contains("DoesNotExist", exception.Message, StringComparison.Ordinal);
    }
}
