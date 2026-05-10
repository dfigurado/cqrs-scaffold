using Scriban;
using System.Reflection;

namespace CqrsScaffold.Tool.Engine
{
    public class TemplateEngine
    {
        private readonly Assembly _assembly = typeof(TemplateEngine).Assembly;

        public string Render(string templateName, object model)
        {
            var resourceName = $"{_assembly.GetName().Name}.Templates.{templateName}.sbn";
            using var stream = _assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Template not found: {templateName}");

            using var reader = new StreamReader(stream);
            var templateContent = reader.ReadToEnd();

            var template = Template.Parse(templateContent);
            return template.Render(model, member => member.Name);
        }
    }
}
