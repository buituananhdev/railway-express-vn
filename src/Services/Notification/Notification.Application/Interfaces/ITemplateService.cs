namespace Notification.Application.Interfaces;

public interface ITemplateService
{
    Task<string> RenderTemplateAsync<T>(string templateName, T model);
}
