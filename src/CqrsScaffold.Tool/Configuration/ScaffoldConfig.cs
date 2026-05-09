namespace CqrsScaffold.Configuration;

public class ScaffoldConfig
{
    public string ProjectName { get; set; } = "MyProject";
    public string OutputPath { get; set; } = Directory.GetCurrentDirectory();
    public string EntityName { get; set; } = "Item";
    public bool IncludeServiceBus { get; set; }
    public string? AuthType { get; set; }
    public bool IncludeDocker { get; set; }
    public string? CiType { get; set; }
}