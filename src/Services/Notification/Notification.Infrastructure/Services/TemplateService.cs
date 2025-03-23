using Notification.Application.Interfaces;
using RazorLight;
using System.Reflection;

namespace Notification.Infrastructure.Services;

public class TemplateService : ITemplateService
{
    private readonly RazorLightEngine _engine;

    public TemplateService()
    {
        // Lấy đường dẫn thư mục gốc của Infrastructure project
        var infrastructureAssembly = Assembly.GetExecutingAssembly();
        var infrastructurePath = Path.GetDirectoryName(infrastructureAssembly.Location);
        var templatePath = Path.Combine(infrastructurePath!, "Templates");

        if (!Directory.Exists(templatePath))
        {
            throw new DirectoryNotFoundException($"Template directory not found: {templatePath}");
        }

        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templatePath)
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<string> RenderTemplateAsync<T>(string templateName, T model)
    {
        return await _engine.CompileRenderAsync($"{templateName}.cshtml", model);
    }
}
